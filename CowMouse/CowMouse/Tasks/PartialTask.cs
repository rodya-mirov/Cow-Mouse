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

namespace CowMouse.Tasks
{
    /// <summary>
    /// This class encompasses a single stage of a task.
    /// 
    /// To complete the task, follow the path (if it exists!)
    /// then do the action type.  Any required fields for the action
    /// should be non-null; others are probably (but not guaranteed
    /// to be) non-null.
    /// </summary>
    public class PartialTask
    {
        private PartialTask(Path path, FullTask parentList, TaskType type)
        {
            this.ParentList = parentList;
            this.Type = type;

            this.Path = path;

            this.StartPoint = path.Start;
            this.EndPoint = path.End;
        }

        private PartialTask(Point onlyPoint, FullTask parentList, TaskType type)
        {
            this.ParentList = parentList;
            this.Type = type;

            this.Path = null;

            this.StartPoint = onlyPoint;
            this.EndPoint = onlyPoint;
        }

        public FullTask ParentList { get; private set; }

        public virtual Point StartPoint { get; private set; }
        public virtual Point EndPoint { get; private set; }

        /// <summary>
        /// If not null, follow the path first.
        /// If null, no movement is required for
        /// this step.
        /// </summary>
        public Path Path { get; private set; }

        public TaskType Type { get; private set; }

        public Carryable ToPickUp { get; private set; }
        public OccupiableZone WhereToPlace { get; private set; }

        public void CleanUp()
        {
            throw new NotImplementedException();
        }

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
            FullTask outputTaskList = new FullTask(priority);

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
                PartialTask pickupTask = new PartialTask(toPickUp.Item1, outputTaskList, TaskType.PICK_UP);
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
                PartialTask putdownTask = new PartialTask(foundPath, outputTaskList, TaskType.PUT_DOWN);
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
        PUT_DOWN //put down a carried item in the intended place
    }
}
