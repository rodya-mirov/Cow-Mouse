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
        public WorldComponent DrawComponent { get; private set; }

        private FPSComponent fpsCounter { get; set; }

        public CowMouseGame()
        {
            graphics = new GraphicsDeviceManager(this);
            preferredWindowedHeight = graphics.PreferredBackBufferHeight;
            preferredWindowedWidth = graphics.PreferredBackBufferWidth;

            Content.RootDirectory = "Content";

            keysHeld = new HashSet<Keys>();
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
            DrawComponent = new WorldComponent(this, manager);

            DrawComponent.Enabled = true;
            DrawComponent.Visible = true;

            Components.Add(DrawComponent);

            fpsCounter = new FPSComponent(this);
            fpsCounter.Visible = false;
            Components.Add(fpsCounter);

            base.Initialize();
        }

        private int preferredWindowedWidth, preferredWindowedHeight;

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

            fpsCounter.Font = Content.Load<SpriteFont>("Fonts/Segoe");

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

        private HashSet<Keys> keysHeld;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            // Allows the game to exit
            if (ks.IsKeyDown(Keys.Escape))
                this.Exit();

            //toggle fullscreen for F11
            if (ks.IsKeyDown(Keys.F11))
            {
                if (!keysHeld.Contains(Keys.F11))
                {
                    keysHeld.Add(Keys.F11);
                    ToggleFullScreen();
                }
            }
            else
            {
                keysHeld.Remove(Keys.F11);
            }

            //toggle FPS counter for F12
            if (ks.IsKeyDown(Keys.F12))
            {
                if (!keysHeld.Contains(Keys.F12))
                {
                    keysHeld.Add(Keys.F12);
                    fpsCounter.Visible = !fpsCounter.Visible;
                }
            }
            else
            {
                keysHeld.Remove(Keys.F12);
            }

            base.Update(gameTime);
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
