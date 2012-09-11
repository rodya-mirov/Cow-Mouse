using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CowMouse.NPCs
{
    public class TownsMan : InGameObject
    {
        protected CowMouseGame Game { get; set; }
        protected int xPos { get; set; }
        protected int yPos { get; set; }
        protected TileMap Map { get; set; }

        protected static Texture2D townsManTexture { get; set; }
        protected const string townsManTexturePath = @"Images\NPCs\TownsMan";

        protected static Rectangle[] sources;

        protected int sourceIndex = 0;

        public TownsMan(CowMouseGame game, int x, int y, TileMap map)
        {
            this.Game = game;
            this.xPos = x;
            this.yPos = y;
            this.Map = map;
        }

        public static void LoadContent(Game game)
        {
            if (townsManTexture == null)
                townsManTexture = game.Content.Load<Texture2D>(townsManTexturePath);

            sources = new Rectangle[4];

            for (int i = 0; i < 4; i++)
                sources[i] = new Rectangle(0, 64 * i, 64, 64);
        }

        public override Rectangle SourceRectangle
        {
            get { return sources[sourceIndex]; }
        }

        public override int xPositionWorld { get { return xPos; } }
        public override int yPositionWorld { get { return yPos; } }

        public override void Update()
        {
            //does nothing
        }

        public override Texture2D Texture
        {
            get { return townsManTexture; }
        }
    }
}
