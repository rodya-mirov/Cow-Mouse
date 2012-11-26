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

        public CowMouseGame()
        {
            graphics = new GraphicsDeviceManager(this);
            preferredWindowedHeight = graphics.PreferredBackBufferHeight;
            preferredWindowedWidth = graphics.PreferredBackBufferWidth;

            Content.RootDirectory = "Content";

            keysHeld = new HashSet<Keys>();

            keyTapBindings = new Dictionary<Keys, Action>();
            keyHoldBindings = new Dictionary<Keys, Action>();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
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

            setupKeyBindings();

            base.Initialize();
        }



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

            keyTapBindings[Keys.Q] = new Action(WorldManager.FollowPreviousNPC);
            keyTapBindings[Keys.E] = new Action(WorldManager.FollowNextNPC);

            keyHoldBindings[Keys.W] = new Action(() => KeyboardMove(0, -KeyboardMoveSpeed));
            keyHoldBindings[Keys.A] = new Action(() => KeyboardMove(-KeyboardMoveSpeed, 0));
            keyHoldBindings[Keys.S] = new Action(() => KeyboardMove(0, KeyboardMoveSpeed));
            keyHoldBindings[Keys.D] = new Action(() => KeyboardMove(KeyboardMoveSpeed, 0));

            keyHoldBindings[Keys.Left] = new Action(() => KeyboardMove(-KeyboardMoveSpeed, 0));
            keyHoldBindings[Keys.Right] = new Action(() => KeyboardMove(KeyboardMoveSpeed, 0));
            keyHoldBindings[Keys.Up] = new Action(() => KeyboardMove(0, -KeyboardMoveSpeed));
            keyHoldBindings[Keys.Down] = new Action(() => KeyboardMove(0, KeyboardMoveSpeed));
        }

        /// <summary>
        /// Move the Camera with respect to the specified change.
        /// Also deselects follow mode.
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        private void KeyboardMove(int dx, int dy)
        {
            Camera.Move(dx, dy);
            WorldManager.Unfollow();
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

            fpsCounter.Font = font;
            this.SideMenu.Font = font;

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
            ProcessKeyboardInput();

            base.Update(gameTime);
        }

        #region UI_switches
        public void SetMouseMode_Stockpiles()
        {
            WorldManager.SetUserMode(UserMode.MAKE_STOCKPILE);
        }

        public void SetMouseMode_Barriers()
        {
            WorldManager.SetUserMode(UserMode.MAKE_BARRIER);
        }

        public void SetMouseMode_Bedrooms()
        {
            WorldManager.SetUserMode(UserMode.MAKE_BEDROOM);
        }

        public void SetMouseMode_NoAction()
        {
            WorldManager.SetUserMode(UserMode.NO_ACTION);
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
}
