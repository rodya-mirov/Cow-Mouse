using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CowMouse.InGameObjects;
using CowMouse.Utilities;
using TileEngine.Utilities.Pathfinding;

namespace CowMouse.NPCs
{
    /// <summary>
    /// The physical part of an NPC, including textures etc.
    /// Has no brain, and must be extended to provide this service.
    /// </summary>
    public abstract class Person : InWorldObject
    {
        protected int xPos { get; set; }
        protected int yPos { get; set; }
        protected CowMouseTileMap Map { get { return WorldManager.MyMap; } }

        protected static Texture2D[] townsManTextures { get; set; }
        protected int textureIndex { get; set; }

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

            this.remainingMovement = 0;
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
        public Person(int xCoordinate, int yCoordinate, bool usingTileCoordinates, WorldManager manager)
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

            pickRandomTextureIndex();
        }

        #region Texture indices
        private static int nextTexture = 0;
        private void pickRandomTextureIndex()
        {
            nextTexture = (nextTexture + 1) % 3;
            this.textureIndex = nextTexture;
        }
        #endregion

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
                aiUpdate(time);
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
        /// Sets the Player_WorldDirection values (that is, directions in
        /// in-world coordinates) to what they are supposed to be, based
        /// on the Player_ViewDirection values (that is, directions on
        /// screen, corresponding to keyboard movement).
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

            FixSourceRectangleInhabited();
        }

        private void FixSourceRectangleInhabited()
        {
            if (Player_WorldHDirection == HorizontalDirection.LEFT)
            {
                if (Player_WorldVDirection == VerticalDirection.NEUTRAL)
                    sourceIndex = 2;
            }
            else if (Player_WorldHDirection == HorizontalDirection.RIGHT)
            {
                if (Player_WorldVDirection == VerticalDirection.NEUTRAL)
                    sourceIndex = 1;
            }
            else if (Player_WorldVDirection == VerticalDirection.UP)
            {
                if (Player_WorldHDirection == HorizontalDirection.NEUTRAL)
                    sourceIndex = 0;
            }
            else if (Player_WorldVDirection == VerticalDirection.DOWN)
            {
                if (Player_WorldHDirection == HorizontalDirection.NEUTRAL)
                    sourceIndex = 3;
            }
        }

        private float PlayerSpeed = 2f;
        private const float DiagonalSpeedAdjustment = .707f; //1/sqrt(2); multiply by this to get diagonal speeds :)

        private float remainingMovement = 0;

        /// <summary>
        /// Process player movement
        /// </summary>
        protected void DoPlayerMovement()
        {
            float speed = PlayerSpeed;

            if (Player_WorldHDirection != HorizontalDirection.NEUTRAL && Player_WorldVDirection != VerticalDirection.NEUTRAL)
                speed *= DiagonalSpeedAdjustment;

            remainingMovement += speed;

            while (remainingMovement >= 1)
            {
                remainingMovement--;

                this.TryPlayerMove((int)Player_WorldHDirection, 0);
                this.TryPlayerMove(0, (int)Player_WorldVDirection);
            }
        }

        /// <summary>
        /// Attempts to move the specified amount.
        /// If impossible, reverts all changes.
        /// </summary>
        /// <param name="xChange"></param>
        /// <param name="yChange"></param>
        private void TryPlayerMove(int xChange, int yChange)
        {
            xPos += xChange;
            yPos += yChange;

            if (WorldManager.DoesBoundingBoxTouchObstacles(this))
            {
                xPos -= xChange;
                yPos -= yChange;
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// This is the main method that extensions will have to override.
        /// This is what the Person will do when left to its own devices.
        /// </summary>
        protected abstract void aiUpdate(GameTime gametime);

        #region Drawing
        /// <summary>
        /// Load all the textures
        /// </summary>
        /// <param name="game"></param>
        public static void LoadContent(Game game)
        {
            townsManTextures = new Texture2D[3];
            townsManTextures[0] = game.Content.Load<Texture2D>(@"Images\NPCs\TownsMan1");
            townsManTextures[1] = game.Content.Load<Texture2D>(@"Images\NPCs\TownsMan2");
            townsManTextures[2] = game.Content.Load<Texture2D>(@"Images\NPCs\TownsWoman1");

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
            get { return townsManTextures[textureIndex]; }
        }
        #endregion

        #region Path following
        protected bool HasDestination = false;
        protected Point CurrentDestination;
        protected Queue<Point> QueuedDestinations = new Queue<Point>();

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
        /// Sets the current destination to a specified SQUARE coordinate.
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
        /// Sets the current destination to a specified SQUARE coordinate.
        /// </summary>
        /// <param name="squareCoordinate"></param>
        protected void SetDestination(Point squareCoordinate)
        {
            SetDestination(squareCoordinate.X, squareCoordinate.Y);
        }

        /// <summary>
        /// Sets the QueuedDestinations to be the queue of Points in a path.
        /// </summary>
        /// <param name="path"></param>
        protected void LoadPathIntoQueuedDestinations(Path path)
        {
            QueuedDestinations.Clear();

            foreach (Point p in path.PointsTraveled())
                QueuedDestinations.Enqueue(p);
        }

        /// <summary>
        /// Whether or not this Person should continue along the current
        /// path.  The default behavior is just to follow it when this is
        /// possible.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldFollowPath()
        {
            if (HasDestination)
                return true;

            if (QueuedDestinations.Count > 0)
                return true;

            return false;
        }

        protected void FollowPath()
        {
            if (HasDestination)
            {
                MoveTowardDestination();
            }
            else if (QueuedDestinations.Count > 0)
            {
                SetDestination(QueuedDestinations.Dequeue());
            }
            else
            {
                throw new NotImplementedException("What did you want, exactly?  I've got no path to follow!");
            }
        }
        #endregion
    }
}
