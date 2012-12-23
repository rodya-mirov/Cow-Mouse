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
using System.Threading;
using CowMouse.Tasks;

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

        private Queue<Building> buildings;
        private Queue<Building> buildingQueue;

        private Queue<Person> npcs;
        private Queue<Carryable> carryables;
        private Queue<Torch> torches;

        private Queue<DebugPixel> pixels;

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

        #region Time keeping
        public TimeKeeper Clock { get; private set; }
        #endregion

        public WorldManager(CowMouseGame game)
            : base(game, TileSheetPath)
        {
            this.game = game;
            Clock = new TimeKeeper(10);

            buildings = new Queue<Building>();
            buildingQueue = new Queue<Building>();

            makeStartingInWorldObjects();
        }

        public override void Initialize()
        {
            base.Initialize();

            defaultHighlightCell = new CowMouseMapCell(2, true);
            defaultInvalidCell = new CowMouseMapCell(3, true);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Person.LoadContent(this.game);
            Log.LoadContent(this.game);
            Torch.LoadContent(this.game);

            CowMouseTileMap.LoadContent(this.game);
        }

        #region Starting Object Creation
        private void makeStartingInWorldObjects()
        {
            Console.WriteLine("Setting up world ...");

            carryables = new Queue<Carryable>();
            npcs = new Queue<Person>();
            pixels = new Queue<DebugPixel>();
            torches = new Queue<Torch>();

            int radius = 30;
            makeRandomLogs(radius, -radius, radius, -radius, radius);

            Console.WriteLine("Logged");

            makeRandomNPCs(10);

            Console.WriteLine("NPCed");

            torches.Enqueue(new Torch(0, 0, this));
            torches.Enqueue(new Torch(5, 0, this));
            torches.Enqueue(new Torch(0, 5, this));
            torches.Enqueue(new Torch(5, 5, this));
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

        #region Drawing Stuff
        /// <summary>
        /// Determines the maximum tint of any (sufficiently well-lit) square.
        /// Depends on the time of day.
        /// </summary>
        /// <returns></returns>
        public Color MaxTint()
        {
            float max = .95f;
            float min = .5f;
            float range = max - min;

            float amount;

            switch (Clock.DayTime)
            {
                case TimeOfDay.AFTERNOON:
                case TimeOfDay.MORNING:
                    return new Color(max, max, max);

                case TimeOfDay.NIGHT_1:
                case TimeOfDay.NIGHT_2:
                    return new Color(min, min, min);

                case TimeOfDay.SUNRISE:
                    amount = range * Clock.PercentageThroughCurrentPhase() + min;
                    return new Color(amount, amount, amount);

                case TimeOfDay.SUNDOWN:
                    amount = max - range * Clock.PercentageThroughCurrentPhase();
                    return new Color(amount, amount, amount);

                default:
                    throw new NotImplementedException();
            }
        }

        private const float maxAmplitude = 1.5f;
        private const float minAmplitude = 0;
        private const float amplitudeRange = maxAmplitude - minAmplitude;

        /// <summary>
        /// This determines the active tint on the specified cell.
        /// For now, this is used for lighting.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Color CellTint(int x, int y)
        {
            float amplitude = 0;

            foreach (Torch t in torches)
            {
                Point p = t.SquareCoordinate;

                float dx = Math.Abs(p.X - x);
                float dy = Math.Abs(p.Y - y);

                amplitude += t.AmountOfLight / (dx * dx + dy * dy);
            }

            if (amplitude > maxAmplitude)
                amplitude = maxAmplitude;
            if (amplitude < minAmplitude)
                amplitude = minAmplitude;

            float scaling = 1 - (amplitude - minAmplitude) / amplitudeRange;

            Vector3 maxTintVector = MaxTint().ToVector3();
            Vector3 noTintVector = Color.White.ToVector3();

            return new Color(scaling * maxTintVector + (1 - scaling) * noTintVector);
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
        #endregion

        protected override CowMouseTileMap makeMap()
        {
            return new CowMouseTileMap();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Clock.Update(gameTime);

            if (FollowMode)
                Camera.CenterOnPoint(FollowTarget.xPositionDraw, FollowTarget.yPositionDraw);

            foreach (Building building in buildings)
                building.Update(gameTime);

            if (!IsThinking)
                AddTask();

            if (buildingQueue.Count > 0)
                AddQueuedBuildings();

            if (queuedTasks.Count > 0)
                AddQueuedTasks();
        }

        #region Adding Buildings
        /// <summary>
        /// Adds the specified building to the world.
        /// </summary>
        /// <param name="building"></param>
        public void addBuilding(Building building)
        {
            buildingQueue.Enqueue(building);
        }

        /// <summary>
        /// Call when the building queue is nonempty, at the proper time.
        /// This loads the new buildings into the world, and if that makes
        /// passability easier or harder, updates this fact.
        /// </summary>
        /// <param name="gameTime"></param>
        private void AddQueuedBuildings()
        {
            Queue<Building> newBuildings = new Queue<Building>();

            bool newWalls = false;

            foreach (Building b in buildings)
            {
                newBuildings.Enqueue(b);
                if (!b.Passable)
                    newWalls = true;
            }

            foreach (Building b in buildingQueue)
                newBuildings.Enqueue(b);

            buildingQueue.Clear();
            this.buildings = newBuildings;

            if (newWalls)
                this.PassabilityMadeHarder(this.currentTime);
        }
        #endregion

        #region Task manager
        private const int DEFAULT_SEARCH_DEPTH = 300;

        private const int HAULING_PRIORITY = 100;
        private const int BUILDING_PRIORITY = 80;

        private TaskHeap availableTasks = new TaskHeap();
        private Queue<FullTask> queuedTasks = new Queue<FullTask>();

        /// <summary>
        /// Determines whether there are any tasks that need to get done.
        /// </summary>
        /// <returns></returns>
        public bool HasAvailableTasks()
        {
            return availableTasks.Count > 0;
        }

        /// <summary>
        /// Returns the highest priority tasks and removes them from
        /// the available task heap.  So if the tasks stored have
        /// priorities 20, 20, 20, 35, 40, 40, 50, it will return
        /// a list of the first three.
        /// </summary>
        /// <returns></returns>
        public List<FullTask> TakeTopPriorityTasks()
        {
            List<FullTask> output = new List<FullTask>();

            FullTask first = availableTasks.Pop();

            output.Add(first);

            while (availableTasks.Count > 0 && availableTasks.Peek().Priority <= first.Priority)
            {
                output.Add(availableTasks.Pop());
            }

            return output;
        }

        /// <summary>
        /// Gives up on a task; returns it to the list of available
        /// tasks.
        /// </summary>
        /// <param name="task"></param>
        public void ReturnTaskUnfinished(FullTask task)
        {
            EnqueueTask(task);
        }

        /// <summary>
        /// Just adds the task to the list of tasks.
        /// </summary>
        /// <param name="task"></param>
        private void EnqueueTask(FullTask task)
        {
            lock (queuedTasks)
            {
                queuedTasks.Enqueue(task);
            }
        }

        private Thread thinkingThread = null;
        private bool IsThinking { get { return thinkingThread != null; } }

        /// <summary>
        /// Starts the thinking thread
        /// </summary>
        private void StartThinkingThread()
        {
            //there could conceivably be more to this at some point
            thinkingThread.Start();
        }

        /// <summary>
        /// Ends the thinking thread; actually this is
        /// the ending OF the thinking thread.  Stores any
        /// new tasks and loses the reference to the thinking
        /// thread.
        /// </summary>
        private void EndOfThinkingThread()
        {
            thinkingThread = null;
        }

        /// <summary>
        /// Adds in the queued tasks into the pile of
        /// available tasks (in a thread-safe way).
        /// </summary>
        private void AddQueuedTasks()
        {
            TaskHeap newTaskHeap = new TaskHeap();
            foreach (FullTask task in availableTasks)
                newTaskHeap.Add(task);

            lock (queuedTasks)
            {
                foreach (FullTask task in queuedTasks)
                {
                    newTaskHeap.Add(task);
                }

                queuedTasks.Clear();
            }

            availableTasks = newTaskHeap;
        }

        /// <summary>
        /// Loads up a task, if possible.  Does most of the work
        /// in the thinkingThread.
        /// </summary>
        private void AddTask()
        {
            if (HaulingTaskPossible())
            {
                thinkingThread = new Thread(() => makeHaulingTask_ThreadHelper());
                StartThinkingThread();
            }

            if (BuildingTaskPossible())
            {
                thinkingThread = new Thread(() => makeBuildingTask_ThreadHelper());
                StartThinkingThread();
            }
        }

        private void makeHaulingTask_ThreadHelper()
        {
            FullTask haulingTask = TaskBuilder.MakeHaulingTask(this, DEFAULT_SEARCH_DEPTH, this.currentTime, HAULING_PRIORITY);

            if (haulingTask != null)
                EnqueueTask(haulingTask);

            EndOfThinkingThread();
        }

        /// <summary>
        /// Determines whether we should attempt to add a Hauling
        /// task to the task heap.
        /// </summary>
        /// <returns></returns>
        private bool HaulingTaskPossible()
        {
            bool resourceExists = false;
            bool stockpileExists = false;

            foreach (Carryable car in this.Carryables)
            {
                if (car.ShouldBeMarkedForHauling)
                {
                    resourceExists = true;
                    break;
                }
            }

            foreach (Stockpile pile in this.Stockpiles)
            {
                if (pile.HasFreeSquare())
                {
                    stockpileExists = true;
                    break;
                }
            }

            return resourceExists && stockpileExists;
        }

        private void makeBuildingTask_ThreadHelper()
        {
            FullTask buildingTask = TaskBuilder.MakeBuildingTask(this, DEFAULT_SEARCH_DEPTH, this.currentTime, BUILDING_PRIORITY);

            if (buildingTask != null)
                EnqueueTask(buildingTask);

            EndOfThinkingThread();
        }

        private bool BuildingTaskPossible()
        {
            foreach (Building building in buildings)
            {
                if (building.HasUnbuiltSquare())
                    return true;
            }

            return false;
        }
        #endregion

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
            foreach (Point p in obj.SquareCoordinatesTouched())
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
