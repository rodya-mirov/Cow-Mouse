﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CowMouse.InGameObjects
{
    public class Log : InanimateObject
    {
        protected CowMouseGame Game { get; set; }
        protected int xPos { get; set; }
        protected int yPos { get; set; }
        protected TileMap Map { get; set; }

        #region Carrying business
        protected bool isBeingCarried;
        protected InGameObject carryingPerson;

        public override bool CanBePickedUp { get { return !IsBeingCarried; } }
        public override bool IsBeingCarried { get { return isBeingCarried; } }
        protected override InGameObject CarryingPerson { get { return carryingPerson; } }

        public override void GetPickedUp(InGameObject picker)
        {
            if (!CanBePickedUp)
                throw new InvalidOperationException("Can't be picked up right now.");

            isBeingCarried = true;
            carryingPerson = picker;
        }

        public override void GetPutDown()
        {
            if (!IsBeingCarried)
                throw new InvalidOperationException("Isn't being carried right now.");

            isBeingCarried = false;
            carryingPerson = null;
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
        public Log(CowMouseGame game, int xCoordinate, int yCoordinate, bool usingTileCoordinates, TileMap map)
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

        public override void Update()
        {
            if (IsBeingCarried)
            {
                xPos = CarryingPerson.xPositionWorld;
                yPos = CarryingPerson.yPositionWorld;
            }
        }

        public override Texture2D Texture
        {
            get { return logTexture; }
        }

        public override int xPositionDraw
        {
            get
            {
                return base.xPositionDraw;
            }
        }

        private const int carryHeight = -20;

        public override int yPositionDraw
        {
            get
            {
                return base.yPositionDraw + (IsBeingCarried ? carryHeight : 0);
            }
        }
    }
}