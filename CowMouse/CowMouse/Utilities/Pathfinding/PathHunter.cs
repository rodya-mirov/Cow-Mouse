using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CowMouse.Utilities.DataStructures;
using Microsoft.Xna.Framework;
using CowMouse.Buildings;

namespace CowMouse.Utilities.Pathfinding
{
    public class PathHunter
    {
        /// <summary>
        /// Just making it so you can't instantiate these things I guess
        /// </summary>
        protected PathHunter()
        {
        }

        /// <summary>
        /// This constructs a path from the specified start point to
        /// some point in the set of destinations.
        /// 
        /// Will only construct paths up to a certain maximum COST;
        /// as of now, going from one square to another costs 1 unit.
        /// So the maximum cost is the maximum path length.
        /// 
        /// Returns null if no path was found.
        /// 
        /// Running time (if no path is found, the worst case) is O(k+n^2), where
        ///        n=maxCost
        ///        k=destinations.Count
        /// 
        /// Assumes no negative cost paths, as in Djikstra's algorithm
        /// </summary>
        /// <param name="startPoint">The point we start from.</param>
        /// <param name="destinations">List of possible destinations we could be happy with.</param>
        /// <param name="maxCost">Maximum cost of any path.</param>
        /// <param name="manager">The WorldManager that we use to do our pathing.</param>
        /// <returns></returns>
        public static Path GetPath(Point startPoint, IEnumerable<Point> destinations, int maxCost, WorldManager manager)
        {
            //first, set up the set of destinations
            HashSet<Point> goalPoints = new HashSet<Point>();
            foreach (Point p in destinations)
                goalPoints.Add(p);

            //check for trivialities- we can't find a path to nowhere
            if (destinations.Count() == 0)
                return null;

            // ... and we don't need to search if we're already there
            if (goalPoints.Contains(startPoint))
                return new Path(startPoint);

            //now set up a list of points we've seen before, so as to not
            //check the same position a billion times
            Dictionary<Point, int> bestDistancesFound = new Dictionary<Point, int>();
            bestDistancesFound[startPoint] = 0;

            PathHeap heap = new PathHeap();
            heap.Add(new Path(startPoint));

            while (heap.Count > 0)
            {
                Path bestPath = heap.Pop();

                //if we didn't cap out our path length yet,
                //check out the adjacent points to form longer paths
                if (bestPath.Cost < maxCost)
                {
                    int newCost = bestPath.Cost + 1;

                    //for each possible extension ...
                    foreach (Point adj in manager.GetAdjacentPoints(bestPath.End.X, bestPath.End.Y))
                    {
                        //if we hit a destination, great, stop
                        if (goalPoints.Contains(adj))
                            return new Path(bestPath, adj, 1);

                        //don't bother adding this possible extension back on unless
                        //it's either a completely new point or a strict improvement over another path
                        if (bestDistancesFound.Keys.Contains(adj) && bestDistancesFound[adj] <= newCost)
                            continue;

                        //otherwise, this is a perfectly serviceable path extension and we should look into it
                        bestDistancesFound[adj] = newCost;

                        heap.Add(new Path(bestPath, adj, newCost));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Like the other GetPath, except we're hunting for buildings instead of specific points.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="destinationBuildings"></param>
        /// <param name="maxCost"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        public static Path GetPath(Point startPoint, IEnumerable<Building> destinationBuildings, int maxCost, WorldManager manager)
        {
            //first, set up the set of destinations
            List<Building> goalBuildings = new List<Building>();
            foreach (Building building in destinationBuildings)
                goalBuildings.Add(building);

            //check for trivialities- we can't find a path to nowhere
            if (goalBuildings.Count() == 0)
                return null;

            // ... and we don't need to search if we're already there
            foreach (Building building in goalBuildings)
            {
                if (building.ContainsCell(startPoint.X, startPoint.Y))
                    return new Path(startPoint);
            }

            //now set up a list of points we've seen before, so as to not
            //check the same position a billion times
            Dictionary<Point, int> bestDistancesFound = new Dictionary<Point, int>();
            bestDistancesFound[startPoint] = 0;

            PathHeap heap = new PathHeap();
            heap.Add(new Path(startPoint));

            while (heap.Count > 0)
            {
                Path bestPath = heap.Pop();

                //if we didn't cap out our path length yet,
                //check out the adjacent points to form longer paths
                if (bestPath.Cost < maxCost)
                {
                    int newCost = bestPath.Cost + 1;

                    //for each possible extension ...
                    foreach (Point adj in manager.GetAdjacentPoints(bestPath.End.X, bestPath.End.Y))
                    {
                        //if we hit a destination, great, stop
                        foreach (Building building in goalBuildings)
                        {
                            if (building.ContainsCell(adj.X, adj.Y))
                                return new Path(bestPath, adj, 1);
                        }

                        //don't bother adding this possible extension back on unless
                        //it's either a completely new point or a strict improvement over another path
                        if (bestDistancesFound.Keys.Contains(adj) && bestDistancesFound[adj] <= newCost)
                            continue;

                        //otherwise, this is a perfectly serviceable path extension and we should look into it
                        bestDistancesFound[adj] = newCost;

                        heap.Add(new Path(bestPath, adj, newCost));
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Just a Heap of Path objects, where better means exclusively lower cost.
    /// </summary>
    class PathHeap : Heap<Path>
    {
        public PathHeap()
            : base()
        {
        }

        public override bool isBetter(Path a, Path b)
        {
            return a.Cost < b.Cost;
        }
    }
}
