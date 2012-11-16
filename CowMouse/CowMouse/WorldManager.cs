using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TileEngine;
using Microsoft.Xna.Framework.Input;
using CowMouse.UserInterface;
using CowMouse.NPCs;
using CowMouse.Buildings;
using CowMouse.InGameObjects;

namespace CowMouse
{
    /// <summary>
    /// This handles the world in the main game.  It's actually that functionality,
    /// along with all the base functionality of the actual TileMapComponent, which
    /// it extends.
    /// </summary>
    public class WorldManager : TileMapManager
    {
        private const string TileSheetPath = @"Images\Tilesets\TileSheet";

        public ResourceTracker Resources { get; private set; }

        private MapCell defaultHighlightCell { get; set; }
        private MapCell defaultInvalidCell { get; set; }

        private SortedSet<Building> buildings;

        private Queue<LogHunter> logHunters;
        private Queue<Carryable> inanimateObjects;

        public WorldManager(Game game)
            : base(game, TileSheetPath)
        {
            Resources = new ResourceTracker();

            buildings = new SortedSet<Building>();

            makeStartingInGameObjects();
        }

        #region Starting Object Creation
        private void makeStartingInGameObjects()
        {
            inanimateObjects = new Queue<Carryable>();
            makeRandomLogs();

            logHunters = new Queue<LogHunter>();
            logHunters.Enqueue(new LogHunter((CowMouseGame)game, 0, 0, true, this.MyMap));
            logHunters.Enqueue(new LogHunter((CowMouseGame)game, 10, 10, true, this.MyMap));
        }

        private void makeRandomLogs()
        {
            Random r = new Random();

            int numLogs = 40;

            List<Point> placed = new List<Point>(numLogs);

            for (int i = 0; i < numLogs; i++)
            {
                int x = 0;
                int y = 0;
                Point p = Point.Zero;

                Random ran = new Random();

                bool isNew = false;

                while (!isNew)
                {
                    x = ran.Next(20);
                    y = ran.Next(20);

                    isNew = true;
                    p = new Point(x, y);

                    foreach (Point q in placed)
                    {
                        if (p.X == q.X && p.Y == q.Y)
                        {
                            isNew = false;
                            break;
                        }
                    }
                }

                placed.Add(p);
                inanimateObjects.Enqueue(new Log((CowMouseGame)game, x, y - x, true, this.MyMap));
            }
        }
        #endregion

        /// <summary>
        /// The list of ingameobjects for the purpose of updating, etc.
        /// </summary>
        protected override IEnumerable<InGameObject> InGameObjects
        {
            get
            {
                foreach (Carryable obj in inanimateObjects)
                    yield return obj;

                foreach (LogHunter obj in logHunters)
                    yield return obj;
            }
        }

        /// <summary>
        /// Enumerates all the valid carryables in the world.
        /// 
        /// Does NOT cull based on CanBePickedUp or IsMarkedForCollection
        /// or anything else like that.
        /// </summary>
        public IEnumerable<Carryable> Carryables
        {
            get
            {
                foreach (Carryable obj in inanimateObjects)
                {
                    yield return obj;
                }
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();

            TownsMan.LoadContent(this.game);
            Log.LoadContent(this.game);
        }

        public override void Initialize()
        {
            base.Initialize();

            Resources.GetIncome(ResourceType.WOOD, 500);
            defaultHighlightCell = new MapCell(2, 0, 0);
            defaultInvalidCell = new MapCell(3, 0, 0);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        protected override TileMap makeMap()
        {
            return new TileMap();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            KeyboardState ks = Keyboard.GetState();
            keyboardMove(ks);

            MouseState ms = Mouse.GetState();
            updateMouseActions(ms);

            foreach (Building building in buildings)
                building.Update();
        }

        private bool Dragging { get; set; }
        private ButtonState leftMouseButtonHeld { get; set; }
        private ButtonState rightMouseButtonHeld { get; set; }
        private Point MouseClickStartSquare { get; set; }
        private Point MouseClickEndSquare { get; set; }

        private void updateMouseActions(MouseState ms)
        {
            //if we changed the button, start that process
            if (ms.LeftButton != leftMouseButtonHeld)
            {
                leftMouseButtonHeld = ms.LeftButton;

                //start a drag box
                if (leftMouseButtonHeld == ButtonState.Pressed)
                {
                    Dragging = true;
                    MouseClickStartSquare = MouseSquare;
                    updateSelectedBlock();
                }

                //end a drag box in loss of interest
                else
                {
                    Dragging = false;
                    MyMap.ClearOverrides();
                }
            }
            
            //alternately, just continue a drag, if appropriate
            else if (Dragging)
            {
                if (MouseClickEndSquare != MouseSquare)
                    updateSelectedBlock();

                //NEW right click means save
                if (rightMouseButtonHeld == ButtonState.Released && ms.RightButton == ButtonState.Pressed)
                    saveDraggedBlock();

                rightMouseButtonHeld = ms.RightButton;
            }
        }

        #region Making buildings code
        private void saveDraggedBlock()
        {
            Dragging = false;

            MyMap.ClearOverrides();

            int xmin = Math.Min(MouseClickStartSquare.X, MouseClickEndSquare.X);
            int xmax = Math.Max(MouseClickStartSquare.X, MouseClickEndSquare.X);

            int ymin = Math.Min(MouseClickStartSquare.Y, MouseClickEndSquare.Y);
            int ymax = Math.Max(MouseClickStartSquare.Y, MouseClickEndSquare.Y);

            if (isValidSelection(xmin, xmax, ymin, ymax))
            {
                Building building = new House(xmin, xmax, ymin, ymax, MyMap);
                if (Resources.SafeSpend(building.GetCosts()))
                    addBuilding(building);
            }
        }

        private void addBuilding(Building building)
        {
            buildings.Add(building);
        }

        private void updateSelectedBlock()
        {
            MouseClickEndSquare = MouseSquare;

            MyMap.ClearOverrides();

            int xmin = Math.Min(MouseClickStartSquare.X, MouseClickEndSquare.X);
            int xmax = Math.Max(MouseClickStartSquare.X, MouseClickEndSquare.X);

            int ymin = Math.Min(MouseClickStartSquare.Y, MouseClickEndSquare.Y);
            int ymax = Math.Max(MouseClickStartSquare.Y, MouseClickEndSquare.Y);

            MapCell cell;

            if (isValidSelection(xmin, xmax, ymin, ymax))
                cell = defaultHighlightCell;
            else
                cell = defaultInvalidCell;

            for (int x = xmin; x <= xmax; x++)
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    MyMap.SetOverride(cell, x, y);
                }
            }
        }

        private bool isValidSelection(int xmin, int xmax, int ymin, int ymax)
        {
            if (xmax - xmin <= 0)
                return false;

            if (ymax - ymin <= 0)
                return false;

            if (xmax - xmin > 1 && ymax - ymin > 1)
                return false;

            Building newBuilding = new House(xmin, xmax, ymin, ymax, MyMap);
            if (!Resources.CanAfford(newBuilding.GetCosts()))
                return false;

            foreach (Building build in buildings)
            {
                if (build.OverlapsRectangle(xmin, xmax, ymin, ymax))
                    return false;
            }

            return true;
        }
        #endregion

        private void keyboardMove(KeyboardState ks)
        {
            if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W))
                Camera.Move(0, -2);

            if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S))
                Camera.Move(0, 2);

            if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A))
                Camera.Move(-2, 0);

            if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D))
                Camera.Move(2, 0);
        }

        #region Pathing assistance
        /// <summary>
        /// Determines whether one can move directly from the start square to the end square.
        /// 
        /// Currently, this returns true iff the squares are adjacent and are either in the
        /// same building or both outside every building.
        /// 
        /// Note it returns FALSE when the squares are the same square, because, what?
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <returns></returns>
        public bool CanMoveFromSquareToSquare(int startX, int startY, int endX, int endY)
        {
            int dist = Math.Abs(startX - endX) + Math.Abs(startY - endY);
            if (dist != 1)
            {
                return false;
            }

            Point start = new Point(startX, startY);
            Point end = new Point(endX, endY);

            foreach (Building b in buildings)
            {
                //this is XOR; so return false if one of the points is inside the building
                //but the other one is out of the building
                if (b.ContainsCell(start.X, start.Y) ^ b.ContainsCell(end.X, end.Y))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns an enumeration of the list of points which can be directly moved to
        /// from the specified point on the map.  Any point (at all!) should be in this
        /// enumeration if and only if it will pass CanMoveFromSquareToSquare
        /// with the specified start point.
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <returns></returns>
        public IEnumerable<Point> GetAdjacentPoints(int startX, int startY)
        {
            //try the four cardinal directions, return them if they work

            //left
            if (CanMoveFromSquareToSquare(startX, startY, startX - 1, startY))
                yield return new Point(startX - 1, startY);

            //right
            if (CanMoveFromSquareToSquare(startX, startY, startX + 1, startY))
                yield return new Point(startX + 1, startY);

            //up
            if (CanMoveFromSquareToSquare(startX, startY, startX, startY - 1))
                yield return new Point(startX, startY - 1);

            //down
            if (CanMoveFromSquareToSquare(startX, startY, startX, startY + 1))
                yield return new Point(startX, startY + 1);
        }
        #endregion
    }
}
