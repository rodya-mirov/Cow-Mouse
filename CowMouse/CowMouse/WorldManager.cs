using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TileEngine;
using Microsoft.Xna.Framework.Input;
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

        private MapCell defaultHighlightCell { get; set; }
        private MapCell defaultInvalidCell { get; set; }

        private SortedSet<Building> buildings;
        private Queue<Building> buildingQueue;

        private Queue<TownsMan> npcs;
        private Queue<Carryable> carryables;

        public new CowMouseGame game { get; set; }

        public UserMode UserMode { get; private set; }
        public void SetUserMode(UserMode mode)
        {
            this.UserMode = mode;
        }

        public WorldManager(CowMouseGame game)
            : base(game, TileSheetPath)
        {
            this.game = game;

            buildings = new SortedSet<Building>();
            buildingQueue = new Queue<Building>();

            makeStartingInGameObjects();

            heldButtons = new HashSet<Keys>();

            this.UserMode = CowMouse.UserMode.NO_ACTION;
        }

        #region Starting Object Creation
        private void makeStartingInGameObjects()
        {
            Console.WriteLine("Setting up world ...");

            carryables = new Queue<Carryable>();
            int radius = 200;
            makeRandomLogs(radius, -radius, radius, -radius, radius);

            Console.WriteLine("Logged");

            npcs = new Queue<TownsMan>();
            makeRandomNPCs(30);

            Console.WriteLine("NPCed");
        }

        private void makeRandomNPCs(int numPeople)
        {
            Random r = new Random();

            for (int i = 0; i < numPeople; i++)
            {
                LogHunter npc = new LogHunter(
                    game,
                    r.Next(numPeople * 2 + 1) - numPeople,
                    r.Next(numPeople * 2 + 1) - numPeople,
                    true,
                    this.MyMap
                    );

                npcs.Enqueue(npc);
            }
        }

        private void makeRandomLogs(int numLogs, int xmin, int xmax, int ymin, int ymax)
        {
            Random r = new Random();

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
                    x = ran.Next(xmax - xmin + 1) + xmin;
                    y = ran.Next(ymax - ymin + 1) + ymin;

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
                carryables.Enqueue(new Log(game, x, y - x, true, this.MyMap));
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
                foreach (Carryable obj in carryables)
                    yield return obj;

                foreach (LogHunter obj in npcs)
                    yield return obj;
            }
        }

        #region Enumerators and Accessors
        /// <summary>
        /// Enumerates all the carryables in the world.
        /// 
        /// Does NOT cull based on CanBePickedUp or IsMarkedForCollection
        /// or anything else like that.
        /// </summary>
        public IEnumerable<Carryable> Carryables
        {
            get
            {
                foreach (Carryable obj in carryables)
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Enumerates all the buildings in the world.
        /// </summary>
        public IEnumerable<Building> Buildings
        {
            get
            {
                foreach (Building b in buildings)
                    yield return b;
            }
        }

        /// <summary>
        /// Enumerates all the stockpiles in the world.
        /// Note that stockpiles are buildings, so this is
        /// a subset of Buildings.
        /// </summary>
        public IEnumerable<Point> StockpilePositions
        {
            get
            {
                foreach (Building b in buildings)
                {
                    if (b.IsStockpile)
                    {
                        foreach (Point p in b.InternalPoints)
                            yield return p;
                    }
                }
            }
        }
        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            TownsMan.LoadContent(this.game);
            Log.LoadContent(this.game);
        }

        public override void Initialize()
        {
            base.Initialize();

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

            if (buildingQueue.Count > 0)
            {
                SortedSet<Building> newBuildings = new SortedSet<Building>();

                foreach (Building b in buildings)
                    newBuildings.Add(b);

                foreach (Building b in buildingQueue)
                    newBuildings.Add(b);

                buildingQueue.Clear();
                this.buildings = newBuildings;
                this.UpdatePassability(gameTime);
            }
        }

        #region Mouse Activities
        private bool Dragging { get; set; }
        private ButtonState leftMouseButtonHeld { get; set; }
        private ButtonState rightMouseButtonHeld { get; set; }
        private Point MouseClickStartSquare { get; set; }
        private Point MouseClickEndSquare { get; set; }

        private HashSet<Keys> heldButtons;

        private bool UserModeLocked = false;
        private UserMode SavedUserMode = UserMode.NO_ACTION;

        private MouseMode MouseModeOf(UserMode um)
        {
            switch (um)
            {
                case UserMode.MAKE_STOCKPILE:
                case CowMouse.UserMode.MAKE_BARRIER:
                    return MouseMode.DRAG;

                case UserMode.NO_ACTION:
                    return MouseMode.NO_ACTION;

                default:
                    throw new NotImplementedException();
            }
        }

        private void updateMouseActions(MouseState ms)
        {
            UserMode relevantUserMode = (UserModeLocked ? SavedUserMode : this.UserMode);

            switch (MouseModeOf(relevantUserMode))
            {
                case MouseMode.DRAG:
                    processDragMode(ms);
                    break;

                case MouseMode.NO_ACTION:
                    //do nothing?
                    break;

                default:
                    throw new NotImplementedException();
            }

            //save current mouse state for next frame
            rightMouseButtonHeld = ms.RightButton;
            leftMouseButtonHeld = ms.LeftButton;
        }

        /// <summary>
        /// Update the dragged block, or deal with releasing
        /// or pressing a mouse button as appropriate.
        /// </summary>
        /// <param name="ms"></param>
        private void processDragMode(MouseState ms)
        {
            //if we changed the button, start that process
            if (ms.LeftButton != leftMouseButtonHeld)
            {
                //if we just pressed the button, start dragging
                if (ms.LeftButton == ButtonState.Pressed)
                {
                    Dragging = true;
                    MouseClickStartSquare = MouseSquare;
                    updateSelectedBlock();

                    lockUserMode();
                }
                else //if we just let go of a button, clear everything out
                {
                    Dragging = false;
                    MyMap.ClearOverrides();

                    unlockUserMode();
                }
            }

            //if we didn't change any buttons, just continue a drag, if appropriate
            //if we're already dragging and we hit the RIGHT mouse button, that means we're saving something
            else if (Dragging)
            {
                if (MouseClickEndSquare != MouseSquare)
                    updateSelectedBlock();

                //NEW right click means save
                if (rightMouseButtonHeld == ButtonState.Released && ms.RightButton == ButtonState.Pressed)
                    saveDraggedBlock();
            }
        }

        private void unlockUserMode()
        {
            this.UserModeLocked = false;
        }

        private void lockUserMode()
        {
            this.UserModeLocked = true;
            this.SavedUserMode = this.UserMode;
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
                switch (this.UserMode)
                {
                    case UserMode.MAKE_STOCKPILE:
                        Stockpile pile = new Stockpile(xmin, xmax, ymin, ymax, MyMap);
                        addBuilding(pile);
                        break;

                    case CowMouse.UserMode.MAKE_BARRIER:
                        Barrier wall = new Barrier(xmin, xmax, ymin, ymax, MyMap);
                        addBuilding(wall);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Adds the specified building to the list of buildings.
        /// </summary>
        /// <param name="building"></param>
        private void addBuilding(Building building)
        {
            buildingQueue.Enqueue(building);
        }

        /// <summary>
        /// This updates the part of the map that's highlighted / selected
        /// by a mouse drag.
        /// </summary>
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

        /// <summary>
        /// Determines whether the box has positive area.
        /// Also checks if this box overlaps any of the existing buildings.
        /// 
        /// Does not check if you can afford whatever it is you're doing!
        /// </summary>
        /// <param name="xmin"></param>
        /// <param name="xmax"></param>
        /// <param name="ymin"></param>
        /// <param name="ymax"></param>
        /// <returns></returns>
        private bool isValidSelection(int xmin, int xmax, int ymin, int ymax)
        {
            if (xmax < xmin)
                return false;

            if (ymax < ymin)
                return false;

            foreach (Building build in buildings)
            {
                if (build.OverlapsRectangle(xmin, xmax, ymin, ymax))
                    return false;
            }

            return true;
        }
        #endregion
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
        /// same building or both outside every building which is not marked passable.
        /// 
        /// Note it returns FALSE when the squares are the same square, because, what?
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <returns></returns>
        public override bool CanMoveFromSquareToSquare(int startX, int startY, int endX, int endY)
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
                //we're only concerned with passable buildings
                if (b.Passable)
                    continue;

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
        public override IEnumerable<Point> GetAdjacentPoints(int startX, int startY)
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

    public enum UserMode
    {
        NO_ACTION,

        MAKE_STOCKPILE,
        MAKE_BARRIER
    }

    public enum MouseMode
    {
        NO_ACTION,
        DRAG
    }
}
