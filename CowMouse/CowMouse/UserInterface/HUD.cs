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

        private ResourceType[] ResourceTypes;
        private ResourceTracker ResourceTracker { get { return worldManager.Resources; } }

        private int height, width;
        private int screenHeight, screenWidth;
        private int drawOffsetX, drawOffsetY;

        #region Source Rectangles
        private Rectangle BackgroundSourceRectangle;
        private Rectangle BackgroundTargetRectangle;

        private Dictionary<ResourceType, Rectangle> ResourceSourceRectangles;
        private Dictionary<ResourceType, Rectangle> ResourceTargetRectangles;
        private Dictionary<ResourceType, Vector2> ResourceTextLocations;
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

            ResourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

            SetupSourceRectangles();
        }

        private void SetupSourceRectangles()
        {
            this.BackgroundSourceRectangle = new Rectangle(0, 0, 200, 70);
            this.BackgroundTargetRectangle = new Rectangle(drawOffsetX, drawOffsetY, width, height);

            this.ResourceSourceRectangles = new Dictionary<ResourceType, Rectangle>();
            this.ResourceTargetRectangles = new Dictionary<ResourceType, Rectangle>();
            this.ResourceTextLocations = new Dictionary<ResourceType, Vector2>();

            ResourceSourceRectangles[ResourceType.WOOD] = new Rectangle(1, 71, 18, 17);
            ResourceSourceRectangles[ResourceType.MONEY] = new Rectangle(20, 71, 17, 17);

            int xpos = drawOffsetX + 15;
            int ypos = drawOffsetY + 10;

            foreach (ResourceType value in ResourceTypes)
            {
                Rectangle sourceRect = ResourceSourceRectangles[value];
                ResourceTargetRectangles[value] = new Rectangle(xpos, ypos, sourceRect.Width, sourceRect.Height);
                ResourceTextLocations[value] = new Vector2(xpos + 30, ypos);

                ypos += 25;
            }
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
            batch.Begin();

            batch.Draw(Texture, BackgroundTargetRectangle, BackgroundSourceRectangle, Color.White);

            foreach (ResourceType value in ResourceTypes)
            {
                batch.Draw(Texture,
                    ResourceTargetRectangles[value],
                    ResourceSourceRectangles[value],
                    Color.White);

                batch.DrawString(Font,
                    ResourceTracker.CurrentHoldings(value).ToString(),
                    ResourceTextLocations[value],
                    Color.Black);
            }

            batch.End();
        }

        public void Update(GameTime gameTime)
        {
            //does nothing for now
        }
    }
}
