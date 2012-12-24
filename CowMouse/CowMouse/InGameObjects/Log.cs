using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CowMouse.Buildings;
using CowMouse.Tasks;

namespace CowMouse.InGameObjects
{
    public class Log : Carryable
    {
        protected int xPos { get; set; }
        protected int yPos { get; set; }

        protected CowMouseTileMap Map { get { return WorldManager.MyMap; } }

        #region Carrying business
        protected InWorldObject carryingPerson = null;
        protected FullTask intendedCollector = null;

        protected override InWorldObject CarryingPerson { get { return carryingPerson; } }

        public override void GetPickedUp(InWorldObject picker)
        {
            if (!CanBePickedUp)
                throw new InvalidOperationException("Can't be picked up right now.");

            this.currentState = CarryableState.CARRIED;
            carryingPerson = picker;
        }

        public override void Drop()
        {
            if (!IsBeingCarried)
                throw new InvalidOperationException("Isn't being carried right now.");

            carryingPerson = null;
            intendedCollector = null;

            currentState = CarryableState.LOOSE;
        }

        public override void GetPutInStockpile()
        {
            if (!IsBeingCarried)
                throw new InvalidOperationException("Isn't being carried right now.");

            carryingPerson = null;
            intendedCollector = null;

            this.currentState = CarryableState.IN_STOCKPILE;
        }

        public override void GetUsedAsMaterial()
        {
            if (!IsBeingCarried)
                throw new InvalidOperationException("Isn't being carried right now.");

            carryingPerson = null;
            intendedCollector = null;

            this.currentState = CarryableState.LOCKED_AS_MATERIAL;
        }

        public override FullTask IntendedCollector { get { return intendedCollector; } }

        public override void MarkForCollection(FullTask collector)
        {
            if (!this.IsAvailableForUse)
                throw new InvalidOperationException("Not available for use!");

            currentState = CarryableState.MARKED_FOR_COLLECTION;
            intendedCollector = collector;
        }

        public override void UnMarkForCollection(FullTask collector)
        {
            if (this.currentState != CarryableState.MARKED_FOR_COLLECTION)
                throw new InvalidOperationException("This is not marked for collection!");

            if (IntendedCollector != collector)
                throw new InvalidOperationException("Only the marker can unmark the marked, and you are not it!");

            this.currentState = CarryableState.LOOSE;
            intendedCollector = null;
        }
        #endregion

        #region Tags
        public override bool IsResource
        {
            get { return true; }
        }

        public override bool IsWood
        {
            get { return true; }
        }
        #endregion

        protected static Texture2D logTexture { get; set; }
        protected const string logTexturePath = @"Images\Objects\Logs";

        protected static Rectangle[] sources;

        protected int sourceIndex = 0;

        /// <summary>
        /// Constructs a log at the specified coordinates
        /// </summary>
        /// <param name="game"></param>
        /// <param name="xCoordinate"></param>
        /// <param name="yCoordinate"></param>
        /// <param name="usingTileCoordinates">True if x,y are referring to SQUARES, or False if they are in-world "pixels"</param>
        /// <param name="map"></param>
        public Log(int xCoordinate, int yCoordinate, bool usingTileCoordinates, WorldManager manager)
            : base(manager)
        {
            if (usingTileCoordinates)
            {
                this.xPos = xCoordinate * Tile.TileInGameWidth + Tile.TileInGameWidthHalf;
                this.yPos = yCoordinate * Tile.TileInGameHeight + Tile.TileInGameHeightHalf;
            }
            else
            {
                this.xPos = xCoordinate;
                this.yPos = yCoordinate;
            }

            this.currentState = CarryableState.LOOSE;
        }

        public static void LoadContent(Game game)
        {
            if (logTexture == null)
                logTexture = game.Content.Load<Texture2D>(logTexturePath);

            sources = new Rectangle[1];
            sources[0] = new Rectangle(0, 0, 64, 64);
        }

        public override Rectangle SourceRectangle
        {
            //if this throws a NullReferenceException then you forgot to call LoadContent
            get { return sources[0]; }
        }

        public override int xPositionWorld { get { return xPos; } }
        public override int yPositionWorld { get { return yPos; } }

        protected const int halfWidth = 5;
        protected const int width = halfWidth * 2;
        protected const int halfHeight = 5;
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

        public override void Update(GameTime time)
        {
            if (IsBeingCarried)
            {
                xPos = CarryingPerson.xPositionWorld + 1;
                yPos = CarryingPerson.yPositionWorld + 1;
            }
        }

        public override Texture2D Texture
        {
            get { return logTexture; }
        }

        private const int carryHeight = 20;

        public override int VisualOffsetX
        {
            get { return 32; }
        }

        public override int VisualOffsetY
        {
            get
            {
                return 48 + (IsBeingCarried ? carryHeight : 0);
            }
        }
    }
}
