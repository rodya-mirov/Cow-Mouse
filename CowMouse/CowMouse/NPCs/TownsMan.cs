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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="xCoordinate"></param>
        /// <param name="yCoordinate"></param>
        /// <param name="usingTileCoordinates">True if x,y are referring to SQUARES, or False if they are in-world "pixels"</param>
        /// <param name="map"></param>
        public TownsMan(CowMouseGame game, int xCoordinate, int yCoordinate, bool usingTileCoordinates, TileMap map)
        {
            this.Game = game;

            if (usingTileCoordinates)
            {
                this.xPos = xCoordinate * Tile.TileInGameWidth;
                this.yPos = yCoordinate * Tile.TileInGameHeight;
            }
            else
            {
                this.xPos = xCoordinate;
                this.yPos = yCoordinate;
            }

            this.Map = map;

            this.destinations = new Queue<Point>();

            destinations.Enqueue(new Point(0, 0));
            destinations.Enqueue(new Point(3, 0));
            destinations.Enqueue(new Point(1, 0));
            destinations.Enqueue(new Point(2, 2));
            destinations.Enqueue(new Point(4, 4));
            destinations.Enqueue(new Point(2, 5));
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

        protected const int halfWidth = 10;
        protected const int width = halfWidth * 2;
        protected const int halfHeight = 10;
        protected const int height = halfHeight * 2;

        public override Rectangle InWorldPixelBoundingBox
        {
            get
            {
                return new Rectangle(
                    xPos - halfWidth,
                    yPos - halfHeight,
                    width,
                    height
                    );
            }
        }

        protected int xDestination { get; set; }
        protected int yDestination { get; set; }

        protected Queue<Point> destinations;

        public override void Update()
        {
            if (xDestination < xPositionWorld)
            {
                sourceIndex = 2;
                xPos -= 1;
            }
            else if (xDestination > xPositionWorld)
            {
                sourceIndex = 1;
                xPos += 1;
            }
            else if (yDestination < yPositionWorld)
            {
                sourceIndex = 0;
                yPos -= 1;
            }
            else if (yDestination > yPositionWorld)
            {
                sourceIndex = 3;
                yPos += 1;
            }
            else
            {
                Point p = destinations.Dequeue();
                SetDestination(p.X, p.Y);
                destinations.Enqueue(p);
            }
        }

        public override Texture2D Texture
        {
            get { return townsManTexture; }
        }

        protected void SetDestination(int xSquare, int ySquare)
        {
            xDestination = FindXCoordinate(xSquare, ySquare);
            yDestination = FindYCoordinate(xSquare, ySquare);
        }
    }
}
