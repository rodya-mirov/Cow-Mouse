using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CowMouse.InGameObjects;

namespace CowMouse.NPCs
{
    /// <summary>
    /// The physical part of an NPC, including textures etc.
    /// Has no brain, and must be extended to provide this service.
    /// </summary>
    public abstract class Person : InWorldObject
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
        public Person(CowMouseGame game, int xCoordinate, int yCoordinate, bool usingTileCoordinates, TileMap map)
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

            this.QueuedDestinations = new Queue<Point>();
            this.HasDestination = false;
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

        protected bool HasDestination;
        protected Point CurrentDestination;
        protected Queue<Point> QueuedDestinations;

        /// <summary>
        /// Move one "in-game pixel" toward the current destination.
        /// Sets HasDestination to true if we are not there yet, or false
        /// otherwise.
        /// 
        /// This should not be used for path-finding, but rather, when
        /// used with SetDestination, is used for moving smoothly along
        /// a path when it's found.
        /// 
        /// Notably, this does NOT check passability or anything.  It
        /// just moves inexorably toward the destination.
        /// </summary>
        protected virtual void MoveTowardDestination()
        {
            int speed = 1;

            if (CurrentDestination.X < xPositionWorld)
            {
                sourceIndex = 2;
                xPos -= speed;
            }
            else if (CurrentDestination.X > xPositionWorld)
            {
                sourceIndex = 1;
                xPos += speed;
            }
            else if (CurrentDestination.Y < yPositionWorld)
            {
                sourceIndex = 0;
                yPos -= speed;
            }
            else if (CurrentDestination.Y > yPositionWorld)
            {
                sourceIndex = 3;
                yPos += speed;
            }

            HasDestination = (CurrentDestination.X != xPositionWorld || CurrentDestination.Y != yPositionWorld);
        }

        public override Texture2D Texture
        {
            get { return townsManTexture; }
        }

        /// <summary>
        /// Sets the current destination to a specified TILE coordinate.
        /// </summary>
        /// <param name="xSquare"></param>
        /// <param name="ySquare"></param>
        protected void SetDestination(int xSquare, int ySquare)
        {
            CurrentDestination.X = FindXCoordinate(xSquare, ySquare);
            CurrentDestination.Y = FindYCoordinate(xSquare, ySquare);

            HasDestination = true;
        }

        /// <summary>
        /// Sets the current destination to a specified TILE coordinate.
        /// </summary>
        /// <param name="tilePoint"></param>
        protected void SetDestination(Point tilePoint)
        {
            SetDestination(tilePoint.X, tilePoint.Y);
        }
    }
}
