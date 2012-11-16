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
        private Queue<InanimateObject> inanimateObjects;

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
            inanimateObjects = new Queue<InanimateObject>();
            makeRandomLogs();

            logHunters = new Queue<LogHunter>();
            logHunters.Enqueue(new LogHunter((CowMouseGame)game, 0, 0, true, this.MyMap));
        }

        private void makeRandomLogs()
        {
            Random r = new Random();

            int numLogs = 20;

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

        protected override IEnumerable<InGameObject> InGameObjects
        {
            get
            {
                foreach (InanimateObject obj in inanimateObjects)
                    yield return obj;

                foreach (LogHunter obj in logHunters)
                    yield return obj;
            }
        }

        public IEnumerable<InanimateObject> Carryables
        {
            get
            {
                foreach (InanimateObject obj in inanimateObjects)
                {
                    if (obj.CanBePickedUp)
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
    }
}
