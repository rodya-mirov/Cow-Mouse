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
    public class WorldManager : TileMapManager<InWorldObject, CowMouseMapCell, CowMouseTileMap>
    {
        private const string TileSheetPath = @"Images\Tilesets\TileSheet";

        private CowMouseMapCell defaultHighlightCell { get; set; }
        private CowMouseMapCell defaultInvalidCell { get; set; }

        private SortedSet<Building> buildings;
        private Queue<Building> buildingQueue;

        private Queue<Person> npcs;
        private Queue<Carryable> carryables;
        private Queue<Torch> torches;

        private Queue<DebugPixel> pixels;
        private bool visualDebugMode = false;

        public new CowMouseGame game { get; set; }

        #region Camera Following NPC
        /// <summary>
        /// Whether or not the camera is following anything at the moment.
        /// </summary>
        public bool FollowMode { get; private set; }

        /// <summary>
        /// If FollowMode is on, this is the target of that following.
        /// </summary>
        public Person FollowTarget { get; private set; }

        /// <summary>
        /// Sets FollowMode to true, and targets the next thing in the queue.
        /// Does nothing if there are no NPCs.
        /// </summary>
        public void FollowNextNPC()
        {
            if (npcs.Count > 0)
            {
                FollowMode = true;

                //this makes the first element last
                npcs.Enqueue(npcs.Dequeue());

                FollowTarget = npcs.Peek();
            }
            else
            {
                FollowMode = false;
            }
        }

        /// <summary>
        /// Sets FollowMode to true, and targets the previous thing in the queue.
        /// Does nothing if there are no NPCs.
        /// </summary>
        public void FollowPreviousNPC()
        {
            if (npcs.Count > 0)
            {
                FollowMode = true;

                //basically, this moves the last element be first
                int n = npcs.Count - 1;
                for (int i = 0; i < n; i++)
                    npcs.Enqueue(npcs.Dequeue());

                FollowTarget = npcs.Peek();
            }
            else
            {
                FollowMode = false;
            }
        }

        /// <summary>
        /// Sets FollowMode to false and clears FollowTarget.
        /// </summary>
        public void Unfollow()
        {
            FollowMode = false;
            FollowTarget = null;
        }
        #endregion

        public WorldManager(CowMouseGame game)
            : base(game, TileSheetPath)
        {
            this.game = game;

            buildings = new SortedSet<Building>();
            buildingQueue = new Queue<Building>();

            makeStartingInWorldObjects();
        }

        #region Starting Object Creation
        private void makeStartingInWorldObjects()
        {
            Console.WriteLine("Setting up world ...");

            carryables = new Queue<Carryable>();
            int radius = 30;
            makeRandomLogs(radius, -radius, radius, -radius, radius);

            Console.WriteLine("Logged");

            npcs = new Queue<Person>();
            pixels = new Queue<DebugPixel>();
            makeRandomNPCs(10);

            Console.WriteLine("NPCed");

            torches = new Queue<Torch>();
            torches.Enqueue(new Torch(0, 0, this));
            torches.Enqueue(new Torch(5, 0, this));
            torches.Enqueue(new Torch(0, 5, this));
            torches.Enqueue(new Torch(5, 5, this));

            if (visualDebugMode)
            {
                foreach (NPC npc in npcs)
                {
                    foreach (DebugPixel p in npc.BoundingPixels(game))
                        pixels.Enqueue(p);
                }

                Console.WriteLine("Pixeled!");
            }
        }

        private void makeRandomNPCs(int numPeople)
        {
            Random r = new Random();

            for (int i = 0; i < numPeople; i++)
            {
                NPC npc = new NPC(
                    r.Next(numPeople * 2 + 1) - numPeople,
                    r.Next(numPeople * 2 + 1) - numPeople,
                    true,
                    this
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
                carryables.Enqueue(new Log(x, y - x, true, this));
            }
        }
        #endregion

        #region Enumerators and Accessors
        /// <summary>
        /// The list of ingameobjects for the purpose of updating, etc.
        /// </summary>
        protected override IEnumerable<InWorldObject> InGameObjects()
        {
            foreach (Carryable obj in carryables)
                yield return obj;

            foreach (NPC obj in npcs)
                yield return obj;

            foreach (Torch torch in torches)
                yield return torch;

            foreach (DebugPixel p in pixels)
                yield return p;
        }

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
        /// Enumerates all the internal points of all the stockpiles
        /// in the world.
        /// </summary>
        public IEnumerable<Stockpile> Stockpiles
        {
            get
            {
                foreach (Building b in buildings)
                {
                    if (b.IsStockpile)
                    {
                        yield return b as Stockpile;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates all the internal points of all the bedrooms
        /// in the world.
        /// </summary>
        public IEnumerable<Bedroom> Bedrooms
        {
            get
            {
                foreach (Building b in buildings)
                {
                    if (b.IsBedroom)
                    {
                        yield return (Bedroom)b;
                    }
                }
            }
        }
        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            Person.LoadContent(this.game);
            Log.LoadContent(this.game);
            Torch.LoadContent(this.game);

            LoadTints();
        }

        #region Drawing Stuff
        private static Color[] tints;
        private static void LoadTints()
        {
            int n = 8;

            tints = new Color[n];
            int amount = 255;

            int acc = -5;
            int vel = -5;

            for (int i = 0; i < n; i++)
            {
                tints[i] = new Color(amount, amount, amount);
                amount += vel;
                vel += acc;

                if (amount < 0)
                    amount = 0;
            }
        }

        //divide minimum distance by this to get the tint index
        private const int distanceScalingFactor = 2;

        public override Color CellTint(int x, int y)
        {
            int minDist = int.MaxValue;

            foreach (Torch t in torches)
            {
                Point p = t.SquareCoordinate;
                int dist = Math.Abs(p.X - x) + Math.Abs(p.Y - y);
                if (dist < minDist)
                    minDist = dist;
            }

            minDist /= distanceScalingFactor;

            if (minDist >= tints.Length)
                minDist = tints.Length - 1;

            return tints[minDist];
        }
        #endregion

        public override void Initialize()
        {
            base.Initialize();

            defaultHighlightCell = new CowMouseMapCell(2, 0, 0, true);
            defaultInvalidCell = new CowMouseMapCell(3, 0, 0, true);
        }

        protected override CowMouseTileMap makeMap()
        {
            return new CowMouseTileMap();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (FollowMode)
                Camera.CenterOnPoint(FollowTarget.xPositionDraw, FollowTarget.yPositionDraw);

            foreach (Building building in buildings)
                building.Update(gameTime);

            if (buildingQueue.Count > 0)
            {
                SortedSet<Building> newBuildings = new SortedSet<Building>();

                bool newWalls = false;

                foreach (Building b in buildings)
                {
                    newBuildings.Add(b);
                    if (!b.Passable)
                        newWalls = true;
                }

                foreach (Building b in buildingQueue)
                    newBuildings.Add(b);

                buildingQueue.Clear();
                this.buildings = newBuildings;

                if (newWalls)
                    this.PassabilityMadeHarder(gameTime);
            }

            if (visualDebugMode)
            {
                MyMap.ClearVisualOverrides();
                foreach (NPC npc in npcs)
                {
                    npc.OverrideTouchedSquares(defaultHighlightCell, this);
                }
            }
        }

        /// <summary>
        /// Adds the specified building to the list of buildings.
        /// </summary>
        /// <param name="building"></param>
        public void addBuilding(Building building)
        {
            buildingQueue.Enqueue(building);
        }

        /// <summary>
        /// In the specified rectangle, set the tile overrides to the
        /// default "valid" override cell or the default "invalid"
        /// override cell, depending on the "valid" parameter.
        /// 
        /// This also clears all other override cells.
        /// </summary>
        /// <param name="xmin"></param>
        /// <param name="ymin"></param>
        /// <param name="xmax"></param>
        /// <param name="ymax"></param>
        /// <param name="valid"></param>
        public void SetVisualOverrides(int xmin, int ymin, int xmax, int ymax, bool valid)
        {
            MyMap.ClearVisualOverrides();

            CowMouseMapCell overrideCell;
            if (valid)
                overrideCell = defaultHighlightCell;
            else
                overrideCell = defaultInvalidCell;

            for (int x = xmin; x <= xmax; x++)
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    MyMap.SetVisualOverride(overrideCell, x, y);
                }
            }
        }

        /// <summary>
        /// Determines whether the box has positive area.
        /// Also checks if this box overlaps any of the existing buildings.
        /// If this is marked as "blockable," it will also check if there is
        /// anything (non-building) which is blocking the current square.
        /// </summary>
        /// <param name="xmin"></param>
        /// <param name="xmax"></param>
        /// <param name="ymin"></param>
        /// <param name="ymax"></param>
        /// <param name="isBlockedByObjects">Whether or not this should be blocked by objects.</param>
        /// <returns></returns>
        public bool IsValidSelection(int xmin, int xmax, int ymin, int ymax, bool isBlockedByObjects)
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

            //If it's a non-passable object, also make sure it doesn't overlap any dudes
            if (isBlockedByObjects)
            {
                foreach (InWorldObject obj in InGameObjects())
                {
                    if (obj.SquareBoundingBoxTouches(xmin, ymin, xmax, ymax))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if a specified pixel bounding box will collide with
        /// any obstacles; returns true if it does, or false if not
        /// </summary>
        /// <param name="pixelBoundingBox"></param>
        /// <returns></returns>
        public bool DoesBoundingBoxTouchObstacles(InWorldObject obj)
        {
            foreach (Point p in obj.TouchedSquareCoordinates())
            {
                if (!this.MyMap.GetRealMapCell(p.X, p.Y).Passable)
                    return true;
            }

            return false;
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
                return false;

            if (!this.MyMap.GetRealMapCell(endX, endY).Passable)
                return false;

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
}
