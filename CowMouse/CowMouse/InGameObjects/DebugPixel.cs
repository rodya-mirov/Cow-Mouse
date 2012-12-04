using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CowMouse.NPCs;
using TileEngine;

namespace CowMouse.InGameObjects
{
    public class DebugPixel : InWorldObject
    {
        private int xOffset, yOffset;

        private Texture2D texture;
        private InWorldObject parent;

        public DebugPixel(int xOffset, int yOffset, CowMouseGame game, Color c, InWorldObject parent)
            : base(game.WorldManager)
        {
            this.xOffset = xOffset;
            this.yOffset = yOffset;

            this.parent = parent;

            loadTexture(game, c);
        }

        private void loadTexture(CowMouseGame game, Color c)
        {
            texture = new Texture2D(game.GraphicsDevice, 1, 1);
            texture.SetData<Color>(new Color[] { c });
        }

        public override int VisualOffsetX
        {
            get { return 0; }
        }

        public override int VisualOffsetY
        {
            get { return 0; }
        }

        public override Texture2D Texture
        {
            get { return texture; }
        }

        public override Rectangle SourceRectangle
        {
            get { return new Rectangle(0, 0, 1, 1); }
        }

        public override int xPositionWorld
        {
            get { return parent.xPositionWorld + xOffset; }
        }

        public override int yPositionWorld
        {
            get { return parent.yPositionWorld + yOffset; }
        }

        public override void Update(GameTime time)
        {
            //do nothing!
        }
    }
}
