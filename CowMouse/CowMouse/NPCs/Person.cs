using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CowMouse.InGameObjects;
using CowMouse.Utilities;

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

        private const int xDrawOffset = 31; //the true center of this thing is about 31 pixels right of xPos
        private const int yDrawOffset = 49; //the true center of this thing is about 49 pixels down of yPos

        protected static Rectangle[] sources;

        protected int sourceIndex = 0;

        #region Habitation
        /// <summary>
        /// Whether or not the Player is inhabiting this
        /// NPC at this time.
        /// </summary>
        public bool IsInhabited
        {
            get;
            private set;
        }

        /// <summary>
        /// Sets IsInhabited to true.
        /// </summary>
        public virtual void Inhabit()
        {
            this.IsInhabited = true;
        }

        /// <summary>
        /// Sets IsInhabited to false.
        /// </summary>
        public virtual void Release()
        {
            this.IsInhabited = false;
        }
        #endregion

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
                this.xPos = xCoordinate * Tile.TileInGameWidth + Tile.TileInGameWidthHalf;
                this.yPos = yCoordinate * Tile.TileInGameHeight + Tile.TileInGameHeightHalf;
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

        protected GameTime lastUpdateTime;

        public override sealed void Update(GameTime time)
        {
            this.lastUpdateTime = time;

            if (IsInhabited)
            {
                playerUpdate();
            }
            else
            {
                aiUpdate();
            }
        }

        #region Player controls
        /// <summary>
        /// The general update method when this NPC is being
        /// controlled by the Player.
        /// </summary>
        protected void playerUpdate()
        {
            DoPlayerMovement();
        }

        #region Player movement
        protected HorizontalDirection Player_ViewHDirection;
        protected VerticalDirection Player_ViewVDirection;

        protected HorizontalDirection Player_WorldHDirection;
        protected VerticalDirection Player_WorldVDirection;

        public void ClearDirectionalMovement()
        {
            Player_ViewHDirection = HorizontalDirection.NEUTRAL;
            Player_ViewVDirection = VerticalDirection.NEUTRAL;

            FixWorldDirection();
        }

        public void SetDirectionalMovement(HorizontalDirection h)
        {
            if (h != Player_ViewHDirection)
            {
                Player_ViewHDirection = h;
                FixWorldDirection();
            }
        }

        public void SetDirectionalMovement(VerticalDirection v)
        {
            if (v != Player_ViewVDirection)
            {
                Player_ViewVDirection = v;
                FixWorldDirection();
            }
        }

        /// <summary>
        /// Massively gross nested switch statement to turn a 2D
        /// view direction into a 2D world direction.
        /// </summary>
        private void FixWorldDirection()
        {
            switch (Player_ViewHDirection)
            {
                case HorizontalDirection.LEFT:
                    switch (Player_ViewVDirection)
                    {
                        case VerticalDirection.DOWN:
                            Player_WorldHDirection = HorizontalDirection.NEUTRAL;
                            Player_WorldVDirection = VerticalDirection.UP;
                            break;

                        case VerticalDirection.NEUTRAL:
                            Player_WorldHDirection = HorizontalDirection.LEFT;
                            Player_WorldVDirection = VerticalDirection.UP;
                            break;

                        case VerticalDirection.UP:
                            Player_WorldHDirection = HorizontalDirection.LEFT;
                            Player_WorldVDirection = VerticalDirection.NEUTRAL;
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case HorizontalDirection.NEUTRAL:
                    switch (Player_ViewVDirection)
                    {
                        case VerticalDirection.DOWN:
                            Player_WorldHDirection = HorizontalDirection.RIGHT;
                            Player_WorldVDirection = VerticalDirection.UP;
                            break;

                        case VerticalDirection.NEUTRAL:
                            Player_WorldHDirection = HorizontalDirection.NEUTRAL;
                            Player_WorldVDirection = VerticalDirection.NEUTRAL;
                            break;

                        case VerticalDirection.UP:
                            Player_WorldHDirection = HorizontalDirection.LEFT;
                            Player_WorldVDirection = VerticalDirection.DOWN;
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case HorizontalDirection.RIGHT:
                    switch (Player_ViewVDirection)
                    {
                        case VerticalDirection.DOWN:
                            Player_WorldHDirection = HorizontalDirection.RIGHT;
                            Player_WorldVDirection = VerticalDirection.NEUTRAL;
                            break;

                        case VerticalDirection.NEUTRAL:
                            Player_WorldHDirection = HorizontalDirection.RIGHT;
                            Player_WorldVDirection = VerticalDirection.DOWN;
                            break;

                        case VerticalDirection.UP:
                            Player_WorldHDirection = HorizontalDirection.NEUTRAL;
                            Player_WorldVDirection = VerticalDirection.DOWN;
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (Player_ViewHDirection != HorizontalDirection.NEUTRAL || Player_ViewVDirection != VerticalDirection.NEUTRAL)
            {
            }
        }

        private int PlayerKeyboardMovementSpeedStraight = 3;
        private int PlayerKeyboardMovementSpeedDiagonal = 2;

        /// <summary>
        /// Process player movement
        /// </summary>
        protected void DoPlayerMovement()
        {
            int speed;

            if (Player_WorldHDirection != HorizontalDirection.NEUTRAL && Player_WorldVDirection != VerticalDirection.NEUTRAL)
                speed = PlayerKeyboardMovementSpeedDiagonal;
            else
                speed = PlayerKeyboardMovementSpeedStraight;

            for (int x = 0; x < speed; x++)
            {
                xPos += (int)Player_WorldHDirection;
                yPos += (int)Player_WorldVDirection;
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// This is the main method that extensions will have to override.
        /// This is what the Person will do when left to its own devices.
        /// </summary>
        protected abstract void aiUpdate();

        #region Drawing
        /// <summary>
        /// Load all the textures
        /// </summary>
        /// <param name="game"></param>
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

        public override int VisualOffsetX { get { return 32; } }
        public override int VisualOffsetY { get { return 48; } }

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
                    width + 1,
                    height + 1
                    );
            }
        }

        public override Texture2D Texture
        {
            get { return townsManTexture; }
        }
        #endregion

        #region Path following
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

            for (int n = 0; n < speed; n++)
            {
                if (CurrentDestination.X < xPositionWorld)
                {
                    sourceIndex = 2;
                    xPos--;
                }
                else if (CurrentDestination.X > xPositionWorld)
                {
                    sourceIndex = 1;
                    xPos++;
                }
                else if (CurrentDestination.Y < yPositionWorld)
                {
                    sourceIndex = 0;
                    yPos--;
                }
                else if (CurrentDestination.Y > yPositionWorld)
                {
                    sourceIndex = 3;
                    yPos++;
                }
            }

            HasDestination = (CurrentDestination.X != xPositionWorld || CurrentDestination.Y != yPositionWorld);
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
        #endregion
    }
}
