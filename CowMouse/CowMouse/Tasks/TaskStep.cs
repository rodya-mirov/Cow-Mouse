using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine.Utilities.DataStructures;
using Microsoft.Xna.Framework;
using TileEngine.Utilities.Pathfinding;
using CowMouse.InGameObjects;
using CowMouse.NPCs;
using CowMouse.Buildings;
using CowMouse.Tasks.TaskStepExtensions;
using CowMouse.Tasks.FullTaskExtensions;

namespace CowMouse.Tasks
{
    /// <summary>
    /// This class encompasses a single stage of a task.
    /// 
    /// To complete the task, follow the path (if it exists!)
    /// then do the action type.  Any required fields for the action
    /// should be non-null; others are probably (but not guaranteed
    /// to be) non-null.
    /// 
    /// Note that TaskStep is abstract, but the extension classes
    /// are pretty simple, and anything USING a TaskStep should be
    /// able to just use TaskStep, and not worry about what kind
    /// of Step it is (aside from the TaskType property, of course).
    /// </summary>
    public abstract class TaskStep
    {
        protected TaskStep(Path path, FullTask parentList, TaskType type)
        {
            this.ParentList = parentList;
            this.Type = type;
            this.Path = path;
        }

        public FullTask ParentList { get; private set; }

        public abstract Point StartPoint { get; }
        public abstract Point EndPoint { get; }

        /// <summary>
        /// If not null, follow the path first.
        /// If null, no movement is required for
        /// this step.
        /// </summary>
        public Path Path { get; private set; }

        /// <summary>
        /// The type of task we're doing.  This
        /// should inform the action necessary.
        /// </summary>
        public TaskType Type { get; protected set; }

        public Carryable ToPickUp { get; protected set; }
        public Building WhereToPlace { get; protected set; }
        public Building ToBuild { get; protected set; }

        /// <summary>
        /// Does any cleanup left to do.  Should be able to be safely
        /// called more than once!
        /// </summary>
        public abstract void CleanUp();

        #region FullTask Makers
        /// <summary>
        /// Returns a new TaskList corresponding to hauling some resource
        /// to some stockpile, if possible.  Returns null otherwise.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="searchDepth"></param>
        /// <param name="currentTime"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static FullTask MakeHaulingGoal(WorldManager manager, int searchDepth, GameTime currentTime, int priority)
        {
            FullTask outputTaskList = new HaulingTask(priority);

            HashSet<Point> resourceLocations = new HashSet<Point>();
            HashSet<Tuple<Point, Carryable>> markedResources = new HashSet<Tuple<Point, Carryable>>();

            HashSet<Point> stockpileLocations = new HashSet<Point>();
            HashSet<Tuple<Point, Stockpile>> markedStockpiles = new HashSet<Tuple<Point, Stockpile>>();

            //mark all the resources
            foreach (Carryable car in manager.Carryables)
            {
                if (car.ShouldBeMarkedForHauling)
                {
                    resourceLocations.Add(car.SquareCoordinate);

                    car.MarkForCollection(outputTaskList);
                    markedResources.Add(new Tuple<Point, Carryable>(car.SquareCoordinate, car));
                }
            }

            //mark all the stockpiles
            foreach (Stockpile pile in manager.Stockpiles)
            {
                if (pile.HasFreeSquare())
                {
                    Point point = pile.GetNextFreeSquare();

                    stockpileLocations.Add(point);

                    pile.MarkSquare(point, outputTaskList);
                    markedStockpiles.Add(new Tuple<Point, Stockpile>(point, pile));
                }
            }

            //attempt to path
            Path foundPath = PathHunter.GetPath(resourceLocations, stockpileLocations, searchDepth, manager, currentTime);

            if (foundPath == null)
            {
                //if impossible, clear out the marked stuff...
                foreach (Tuple<Point, Carryable> tup in markedResources)
                    tup.Item2.UnMarkForCollection(outputTaskList);

                foreach (Tuple<Point, Stockpile> tup in markedStockpiles)
                    tup.Item2.UnMarkSquare(tup.Item1, outputTaskList);

                //and give up
                return null;
            }
            else
            {
                //first, find the resource
                Tuple<Point, Carryable> toPickUp = null;

                foreach (Tuple<Point, Carryable> tup in markedResources)
                {
                    if (toPickUp == null && tup.Item1 == foundPath.Start)
                        toPickUp = tup;
                    else
                        tup.Item2.UnMarkForCollection(outputTaskList);
                }

                if (toPickUp == null)
                    throw new InvalidOperationException();

                //now make that task up!
                TaskStep pickupTask = new PickupStep(null, outputTaskList, toPickUp.Item2);
                pickupTask.ToPickUp = toPickUp.Item2;
                outputTaskList.AddNewTask(pickupTask);

                //now, find the stockpile
                Tuple<Point, Stockpile> goalPile = null;

                foreach (Tuple<Point, Stockpile> tup in markedStockpiles)
                {
                    if (goalPile == null && tup.Item1 == foundPath.End)
                        goalPile = tup;
                    else
                        tup.Item2.UnMarkSquare(tup.Item1, outputTaskList);
                }

                if (goalPile == null)
                    throw new InvalidOperationException();

                //make THAT task
                TaskStep putdownTask = new StockpileStep(foundPath, toPickUp.Item2, goalPile.Item2, outputTaskList);
                putdownTask.WhereToPlace = goalPile.Item2;
                outputTaskList.AddNewTask(putdownTask);

                //freeze and send out!
                outputTaskList.Freeze();
                return outputTaskList;
            }
        }
        #endregion
    }

    /// <summary>
    /// The type of action we need to take at the end of the path!
    /// </summary>
    public enum TaskType
    {
        PICK_UP, //pick up the relevant item
        PUT_DOWN, //put down a carried item in the intended place

        BUILD //do some work!
    }
}
