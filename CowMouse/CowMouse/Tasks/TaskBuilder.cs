using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using CowMouse.Tasks.FullTaskExtensions;
using CowMouse.InGameObjects;
using CowMouse.Buildings;
using TileEngine.Utilities.Pathfinding;
using CowMouse.Tasks.TaskStepExtensions;

namespace CowMouse.Tasks
{
    public static class TaskBuilder
    {

        /// <summary>
        /// Returns a new TaskList corresponding to hauling some resource
        /// to some stockpile, if possible.  Returns null otherwise.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="searchDepth"></param>
        /// <param name="currentTime"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static FullTask MakeHaulingTask(WorldManager manager, int searchDepth, GameTime currentTime, int priority)
        {
            HaulingTask outputTaskList = new HaulingTask(priority);

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
                outputTaskList.AddNewTask(putdownTask);

                //freeze and send out!
                outputTaskList.Freeze();
                return outputTaskList;
            }
        }

        public static FullTask MakeBuildingTask(WorldManager worldManager, int searchDepth, GameTime gameTime, int priority)
        {
            BuildingTask output = new BuildingTask(priority);

            bool foundSuccess = false;
            Building building = null;
            Point buildPoint = Point.Zero;

            foreach (Building candidate in worldManager.Buildings)
            {
                if (candidate.HasUnbuiltSquare())
                {
                    foundSuccess = true;
                    building = candidate;
                    buildPoint = candidate.GetNextUnbuiltSquare();
                    break;
                }
            }

            if (!foundSuccess)
                return null;
            
            building.MarkSquareForBuilding(buildPoint.X, buildPoint.Y, output);
            TaskStep step = new BuildStep(buildPoint, building, output);
            output.AddNewTask(step);

            output.Freeze();

            return output;
        }
    }
}
