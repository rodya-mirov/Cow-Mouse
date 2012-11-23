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

        //these two are for keyboard input
        private HashSet<Keys> keysHeld;
        private Dictionary<Keys, Action> keyBindings;

        //for the window itself
        private int preferredWindowedWidth;
        private int preferredWindowedHeight;

        public CowMouseGame()
        {
            graphics = new GraphicsDeviceManager(this);
            preferredWindowedHeight = graphics.PreferredBackBufferHeight;
            preferredWindowedWidth = graphics.PreferredBackBufferWidth;

            Content.RootDirectory = "Content";

            keysHeld = new HashSet<Keys>();

            keyBindings = new Dictionary<Keys, Action>();
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
            SideMenu.Visible = false;
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
            keyBindings[Keys.Escape] = new Action(this.Exit);
            keyBindings[Keys.M] = new Action(SideMenu.ToggleVisible);
            keyBindings[Keys.F11] = new Action(this.ToggleFullScreen);
            keyBindings[Keys.F12] = new Action(fpsCounter.ToggleVisible);
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

            int centerShiftX = (newWidth - oldWidth) / 2;
            int centerShiftY = (newHeight - oldHeight) / 2;

            Camera.Move(-centerShiftX, -centerShiftY);

            graphics.IsFullScreen = !graphics.IsFullScreen;

            this.WorldManager.SetViewDimensions(newWidth, newHeight);

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
            WorldManager.SetMouseMode(MouseMode.MAKE_STOCKPILE);
        }

        public void SetMouseMode_NoAction()
        {
            WorldManager.SetMouseMode(MouseMode.NO_ACTION);
        }
        #endregion

        private void ProcessKeyboardInput()
        {
            KeyboardState ks = Keyboard.GetState();

            foreach (Keys key in keyBindings.Keys)
            {
                if (ks.IsKeyDown(key))
                {
                    if (!keysHeld.Contains(key))
                    {
                        keyBindings[key].Invoke();
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
