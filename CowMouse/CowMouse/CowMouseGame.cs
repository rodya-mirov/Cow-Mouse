using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TileEngine;
using CowMouse.UserInterface;
using CowMouse.Buildings;
using CowMouse.Utilities;
using CowMouse.NPCs;

namespace CowMouse
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CowMouseGame : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager graphics { get; private set; }
        public SpriteBatch spriteBatch { get; private set; }

        public WorldManager WorldManager { get { return DrawComponent.WorldManager; } }
        public CowMouseComponent DrawComponent { get; private set; }
        public ClockViewer ClockViewer { get; private set; }

        private FPSComponent fpsCounter { get; set; }

        private SideMenu SideMenu { get; set; }

        //these three are for keyboard input
        private HashSet<Keys> keysHeld;
        private Dictionary<Keys, Action> keyTapBindings;
        private Dictionary<Keys, Action> keyHoldBindings;

        //for the window itself
        private int preferredWindowedWidth;
        private int preferredWindowedHeight;

        //scrollin
        private int KeyboardMoveSpeed = 2;

        #region Game Mode
        private GameMode gameMode;

        /// <summary>
        /// The current GameMode we're playing in.
        /// </summary>
        public GameMode GameMode
        {
            get
            {
                return gameMode;
            }

            set
            {
                if (gameMode == value)
                    return;

                gameMode = value;
                switch (gameMode)
                {
                    case CowMouse.GameMode.ADVENTURE:
                        SideMenu.Visible = false;
                        break;

                    case CowMouse.GameMode.TOWN:
                        SideMenu.Visible = true;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Inhabits the followtarget, if one exists and we're in Town mode.
        /// Otherwise does nothing.
        /// </summary>
        private void InhabitFollowedNPC()
        {
            switch (GameMode)
            {
                case CowMouse.GameMode.TOWN:
                    if (WorldManager.FollowMode && WorldManager.FollowTarget != null)
                    {
                        GameMode = CowMouse.GameMode.ADVENTURE;
                        WorldManager.FollowTarget.Inhabit();
                    }
                    break;

                case CowMouse.GameMode.ADVENTURE:
                    //do nothing
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Releases the followtarget, if one exists and we're in Adventure mode.
        /// Otherwise does nothing.
        /// </summary>
        private void ReleaseFollowedNPC()
        {
            switch (GameMode)
            {
                case CowMouse.GameMode.ADVENTURE:
                    if (WorldManager.FollowMode && WorldManager.FollowTarget != null)
                    {
                        GameMode = CowMouse.GameMode.TOWN;
                        WorldManager.FollowTarget.Release();
                    }
                    break;

                case CowMouse.GameMode.TOWN:
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        public CowMouseGame()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            preferredWindowedHeight = graphics.PreferredBackBufferHeight;
            preferredWindowedWidth = graphics.PreferredBackBufferWidth;

            Content.RootDirectory = "Content";

            keysHeld = new HashSet<Keys>();

            keyTapBindings = new Dictionary<Keys, Action>();
            keyHoldBindings = new Dictionary<Keys, Action>();

            this.UserMode = CowMouse.UserMouseMode.NO_ACTION;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Tile.TileVisualOffsetX = 0;
            Tile.TileVisualOffsetY = 48;

            WorldManager manager = new WorldManager(this);

            DrawComponent = new CowMouseComponent(this, manager);
            DrawComponent.Enabled = true;
            DrawComponent.Visible = true;
            Components.Add(DrawComponent);

            SideMenu = new SideMenu(this);
            SideMenu.Visible = true;
            Components.Add(SideMenu);

            fpsCounter = new FPSComponent(this);
            fpsCounter.Visible = false;
            Components.Add(fpsCounter);

            ClockViewer = new CowMouse.ClockViewer(manager.Clock, this);
            ClockViewer.Visible = true;
            Components.Add(ClockViewer);

            setupKeyBindings();

            base.Initialize();
        }

        /// <summary>
        /// Toggle full screen on or off.  Also keeps the camera so that it's centered on the same point
        /// (assuming the window itself is centered)
        /// </summary>
        private void ToggleFullScreen()
        {
            int newWidth, newHeight;
            int oldWidth = graphics.PreferredBackBufferWidth;
            int oldHeight = graphics.PreferredBackBufferHeight;

            if (graphics.IsFullScreen)
            {
                newWidth = preferredWindowedWidth;
                newHeight = preferredWindowedHeight;
            }
            else
            {
                newWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                newHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }

            graphics.PreferredBackBufferWidth = newWidth;
            graphics.PreferredBackBufferHeight = newHeight;

            Point center = Camera.GetCenter();

            graphics.IsFullScreen = !graphics.IsFullScreen;

            this.WorldManager.SetViewDimensions(newWidth, newHeight);

            Camera.CenterOnPoint(center);

            graphics.ApplyChanges();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            WorldManager.SetViewDimensions(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            SpriteFont font = Content.Load<SpriteFont>("Fonts/Segoe");

            this.fpsCounter.Font = font;
            this.SideMenu.Font = font;
            this.ClockViewer.Font = font;

            this.IsMouseVisible = true;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            ClearMovementDirections();

            ProcessKeyboardInput();
            ProcessMouseInput();

            DoCameraMovement();

            base.Update(gameTime);
        }

        #region UI_switches
        public void SetMouseMode_Stockpiles()
        {
            this.UserMode = UserMouseMode.MAKE_STOCKPILE;
        }

        public void SetMouseMode_Barriers()
        {
            this.UserMode = UserMouseMode.MAKE_BARRIER;
        }

        public void SetMouseMode_Bedrooms()
        {
            this.UserMode = UserMouseMode.MAKE_BEDROOM;
        }

        public void SetMouseMode_NoAction()
        {
            this.UserMode = UserMouseMode.NO_ACTION;
        }
        #endregion

        #region Mouse Stuff
        private bool Dragging { get; set; }
        private ButtonState leftMouseButtonHeld { get; set; }
        private ButtonState rightMouseButtonHeld { get; set; }
        private Point MouseClickStartSquare { get; set; }
        private Point MouseClickEndSquare { get; set; }

        private bool UserModeLocked = false;
        private UserMouseMode SavedUserMode = UserMouseMode.NO_ACTION;

        public UserMouseMode UserMode { get; set; }

        private void ProcessMouseInput()
        {
            MouseState ms = Mouse.GetState();

            UserMouseMode relevantUserMode = (UserModeLocked ? SavedUserMode : this.UserMode);

            switch (MouseModeOf(relevantUserMode))
            {
                case MouseAction.DRAG:
                    ProcessDragMode(ms);
                    break;

                case MouseAction.NO_ACTION:
                    //do nothing?
                    break;

                default:
                    throw new NotImplementedException();
            }

            //save current mouse state for next frame
            rightMouseButtonHeld = ms.RightButton;
            leftMouseButtonHeld = ms.LeftButton;
        }

        /// <summary>
        /// Unlocks the usermode.
        /// </summary>
        private void unlockUserMode()
        {
            this.UserModeLocked = false;
        }

        /// <summary>
        /// Locks the usermode.
        /// </summary>
        private void lockUserMode()
        {
            this.UserModeLocked = true;
            this.SavedUserMode = this.UserMode;
        }

        /// <summary>
        /// Determines which MouseMode corresponds to the
        /// specified UserMode.
        /// </summary>
        /// <param name="um"></param>
        /// <returns></returns>
        private MouseAction MouseModeOf(UserMouseMode um)
        {
            switch (um)
            {
                case CowMouse.UserMouseMode.MAKE_STOCKPILE:
                case CowMouse.UserMouseMode.MAKE_BARRIER:
                case CowMouse.UserMouseMode.MAKE_BEDROOM:
                    return MouseAction.DRAG;

                case CowMouse.UserMouseMode.NO_ACTION:
                    return MouseAction.NO_ACTION;

                default:
                    throw new NotImplementedException();
            }
        }

        #region Dragging

        /// <summary>
        /// This updates the part of the map that's highlighted / selected
        /// by a mouse drag.
        /// </summary>
        private void UpdateSelectionVisualIndicator()
        {
            MouseClickEndSquare = WorldManager.MouseSquare;

            int xmin = Math.Min(MouseClickStartSquare.X, MouseClickEndSquare.X);
            int xmax = Math.Max(MouseClickStartSquare.X, MouseClickEndSquare.X);

            int ymin = Math.Min(MouseClickStartSquare.Y, MouseClickEndSquare.Y);
            int ymax = Math.Max(MouseClickStartSquare.Y, MouseClickEndSquare.Y);

            bool valid = WorldManager.IsValidSelection(xmin, xmax, ymin, ymax, IsSelectionBlockedByObjects());

            WorldManager.SetVisualOverrides(xmin, ymin, xmax, ymax, valid);
        }

        /// <summary>
        /// Update the dragged block, or deal with releasing
        /// or pressing a mouse button as appropriate.
        /// </summary>
        private void ProcessDragMode(MouseState ms)
        {
            //if we changed the button, start that process
            if (ms.LeftButton != leftMouseButtonHeld)
            {
                //if we just pressed the button, start dragging
                if (ms.LeftButton == ButtonState.Pressed)
                {
                    Dragging = true;
                    MouseClickStartSquare = WorldManager.MouseSquare;
                    UpdateSelectionVisualIndicator();

                    lockUserMode();
                }
                else //if we just let go of a button, clear everything out
                {
                    Dragging = false;
                    WorldManager.MyMap.ClearVisualOverrides();

                    unlockUserMode();
                }
            }

            //if we didn't change any buttons, just continue a drag, if appropriate
            //if we're already dragging and we hit the RIGHT mouse button, that means we're saving something
            else if (Dragging)
            {
                //we need to updateSelectedBlock every turn, even if we don't move the mouse;
                //something could have happened to make this invalid
                UpdateSelectionVisualIndicator();

                //NEW right click means save
                if (rightMouseButtonHeld == ButtonState.Released && ms.RightButton == ButtonState.Pressed)
                    SaveDraggedBlock();
            }
        }

        /// <summary>
        /// Saves the selected dragged block as the relevant kind of building,
        /// if it's a valid selection.  Adds this building to the WorldManager.
        /// </summary>
        private void SaveDraggedBlock()
        {
            Dragging = false;

            WorldManager.MyMap.ClearVisualOverrides();

            int xmin = Math.Min(MouseClickStartSquare.X, MouseClickEndSquare.X);
            int xmax = Math.Max(MouseClickStartSquare.X, MouseClickEndSquare.X);

            int ymin = Math.Min(MouseClickStartSquare.Y, MouseClickEndSquare.Y);
            int ymax = Math.Max(MouseClickStartSquare.Y, MouseClickEndSquare.Y);

            if (WorldManager.IsValidSelection(xmin, xmax, ymin, ymax, IsSelectionBlockedByObjects()))
            {
                switch (this.UserMode)
                {
                    case CowMouse.UserMouseMode.MAKE_STOCKPILE:
                        Stockpile pile = new Stockpile(xmin, xmax, ymin, ymax, WorldManager);
                        WorldManager.addBuilding(pile);
                        break;

                    case CowMouse.UserMouseMode.MAKE_BARRIER:
                        Barrier wall = new Barrier(xmin, xmax, ymin, ymax, WorldManager);
                        WorldManager.addBuilding(wall);
                        break;

                    case CowMouse.UserMouseMode.MAKE_BEDROOM:
                        Bedroom bedroom = new Bedroom(xmin, xmax, ymin, ymax, WorldManager);
                        WorldManager.addBuilding(bedroom);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Determines whether the selection indicated by the UserMode
        /// would be blocked by existing objects (if they touch it).
        /// </summary>
        /// <returns></returns>
        private bool IsSelectionBlockedByObjects()
        {
            bool blockedByObjects;
            switch (UserMode)
            {
                case CowMouse.UserMouseMode.MAKE_BARRIER:
                    blockedByObjects = true;
                    break;

                case CowMouse.UserMouseMode.MAKE_BEDROOM:
                case CowMouse.UserMouseMode.MAKE_STOCKPILE:
                case CowMouse.UserMouseMode.NO_ACTION:
                    blockedByObjects = false;
                    break;

                default:
                    throw new NotImplementedException();
            }
            return blockedByObjects;
        }
        #endregion

        #endregion

        #region Keyboard Stuff
        /// <summary>
        /// Sets up the key bindings!  No more typing the same
        /// recipe every time!
        /// </summary>
        private void setupKeyBindings()
        {
            keyTapBindings[Keys.Escape] = new Action(this.Exit);
            keyTapBindings[Keys.M] = new Action(SideMenu.ToggleVisible);
            keyTapBindings[Keys.F11] = new Action(this.ToggleFullScreen);
            keyTapBindings[Keys.F12] = new Action(fpsCounter.ToggleVisible);

            keyTapBindings[Keys.Q] = new Action(FollowPreviousNPC);
            keyTapBindings[Keys.E] = new Action(FollowNextNPC);

            keyTapBindings[Keys.Enter] = new Action(InhabitFollowedNPC);
            keyTapBindings[Keys.Back] = new Action(ReleaseFollowedNPC);

            keyTapBindings[Keys.Delete] = new Action(BreakpointDoesNothing);

            keyHoldBindings[Keys.A] = new Action(() => SetDirectionalMovement(HorizontalDirection.LEFT));
            keyHoldBindings[Keys.W] = new Action(() => SetDirectionalMovement(VerticalDirection.UP));
            keyHoldBindings[Keys.S] = new Action(() => SetDirectionalMovement(VerticalDirection.DOWN));
            keyHoldBindings[Keys.D] = new Action(() => SetDirectionalMovement(HorizontalDirection.RIGHT));

            keyHoldBindings[Keys.Left] = new Action(() => SetDirectionalMovement(HorizontalDirection.LEFT));
            keyHoldBindings[Keys.Right] = new Action(() => SetDirectionalMovement(HorizontalDirection.RIGHT));
            keyHoldBindings[Keys.Up] = new Action(() => SetDirectionalMovement(VerticalDirection.UP));
            keyHoldBindings[Keys.Down] = new Action(() => SetDirectionalMovement(VerticalDirection.DOWN));
        }

        private void BreakpointDoesNothing()
        {
            Person ofInterest = WorldManager.FollowTarget;

            Point pixelPoint = new Point(ofInterest.xPositionWorld, ofInterest.yPositionWorld);
            Point SquareCoordinate = ofInterest.SquareCoordinate;
            Rectangle pixelBox = ofInterest.InWorldPixelBoundingBox;
            Rectangle squareBox = ofInterest.InWorldSquareBoundingBox;

            if (true)
            {
            }
        }

        #region Keyboard Movement
        private VerticalDirection VerticalMovementDirection;
        private HorizontalDirection HorizontalMovementDirection;

        /// <summary>
        /// No longer moving :)
        /// </summary>
        private void ClearMovementDirections()
        {
            switch (GameMode)
            {
                case CowMouse.GameMode.ADVENTURE:
                    WorldManager.FollowTarget.ClearDirectionalMovement();
                    break;

                case CowMouse.GameMode.TOWN:
                    this.VerticalMovementDirection = VerticalDirection.NEUTRAL;
                    this.HorizontalMovementDirection = HorizontalDirection.NEUTRAL;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void SetDirectionalMovement(HorizontalDirection h)
        {
            switch (GameMode)
            {
                case CowMouse.GameMode.TOWN:
                    this.HorizontalMovementDirection = h;
                    break;

                case CowMouse.GameMode.ADVENTURE:
                    WorldManager.FollowTarget.SetDirectionalMovement(h);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void SetDirectionalMovement(VerticalDirection v)
        {
            switch (GameMode)
            {
                case CowMouse.GameMode.TOWN:
                    this.VerticalMovementDirection = v;
                    break;

                case CowMouse.GameMode.ADVENTURE:
                    WorldManager.FollowTarget.SetDirectionalMovement(v);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void DoCameraMovement()
        {
            switch (GameMode)
            {
                case CowMouse.GameMode.ADVENTURE:
                    break;

                case CowMouse.GameMode.TOWN:
                    if (Math.Abs((int)this.HorizontalMovementDirection) + Math.Abs((int)this.VerticalMovementDirection) > 0)
                        WorldManager.Unfollow();

                    Camera.Move(KeyboardMoveSpeed * (int)HorizontalMovementDirection, KeyboardMoveSpeed * (int)VerticalMovementDirection);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region Following
        private void FollowNextNPC()
        {
            switch (GameMode)
            {
                case CowMouse.GameMode.ADVENTURE:
                    //do nothing
                    break;

                case CowMouse.GameMode.TOWN:
                    WorldManager.FollowNextNPC();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void FollowPreviousNPC()
        {
            switch (GameMode)
            {
                case CowMouse.GameMode.ADVENTURE:
                    //do nothing
                    break;

                case CowMouse.GameMode.TOWN:
                    WorldManager.FollowPreviousNPC();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        /// <summary>
        /// Mash each of the supplied key bindings.
        /// </summary>
        private void ProcessKeyboardInput()
        {
            KeyboardState ks = Keyboard.GetState();

            foreach (Keys key in keyHoldBindings.Keys)
            {
                if (ks.IsKeyDown(key))
                    keyHoldBindings[key].Invoke();
            }

            foreach (Keys key in keyTapBindings.Keys)
            {
                if (ks.IsKeyDown(key))
                {
                    if (!keysHeld.Contains(key))
                    {
                        keyTapBindings[key].Invoke();
                        keysHeld.Add(key);
                    }
                }
                else
                {
                    keysHeld.Remove(key);
                }
            }
        }
        #endregion

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }

    public enum GameMode
    {
        TOWN,
        ADVENTURE
    }

    public enum UserMouseMode
    {
        NO_ACTION,

        MAKE_STOCKPILE,
        MAKE_BEDROOM,
        MAKE_BARRIER
    }

    public enum MouseAction
    {
        NO_ACTION,
        DRAG
    }
}
