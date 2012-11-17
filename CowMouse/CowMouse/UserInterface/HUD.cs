using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CowMouse.UserInterface
{
    /// <summary>
    /// General information display for the user.
    /// </summary>
    public class HUD
    {
        private const string TexturePath = @"Images\HUD\HUD";
        private static Texture2D Texture { get; set; }

        private const string FontPath = @"Fonts\Segoe";
        private static SpriteFont Font { get; set; }

        private WorldManager worldManager { get; set; }
        private SpriteBatch batch { get; set; }
        private GraphicsDevice GraphicsDevice { get { return worldManager.game.GraphicsDevice; } }

        private int height, width;
        private int screenHeight, screenWidth;
        private int drawOffsetX, drawOffsetY;

        #region Source Rectangles
        private Rectangle BackgroundSourceRectangle;
        private Rectangle BackgroundTargetRectangle;
        #endregion

        public HUD(WorldManager worldManager)
        {
            this.worldManager = worldManager;

            this.batch = new SpriteBatch(GraphicsDevice);

            this.screenWidth = GraphicsDevice.Viewport.Width;
            this.screenHeight = GraphicsDevice.Viewport.Height;

            this.height = 70;
            this.drawOffsetY = this.screenHeight - height;

            this.width = this.screenWidth;
            this.drawOffsetX = 0;

            SetupSourceRectangles();
        }

        private void SetupSourceRectangles()
        {
            this.BackgroundSourceRectangle = new Rectangle(0, 0, 200, 70);
            this.BackgroundTargetRectangle = new Rectangle(drawOffsetX, drawOffsetY, width, height);
        }

        public void LoadContent()
        {
            if (Texture == null)
                Texture = worldManager.game.Content.Load<Texture2D>(TexturePath);

            if (Font == null)
                Font = worldManager.game.Content.Load<SpriteFont>(FontPath);
        }

        public void Draw(GameTime gameTime)
        {
            if (Visible)
            {
                batch.Begin();

                batch.Draw(Texture, BackgroundTargetRectangle, BackgroundSourceRectangle, Color.White);

                batch.End();
            }
        }

        public void Update(GameTime gameTime)
        {
            //does nothing for now
        }

        private bool Visible { get; set; }

        public void ToggleVisible()
        {
            Visible = !Visible;
        }
    }
}
