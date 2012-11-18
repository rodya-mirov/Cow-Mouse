using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework;
using CowMouse.InGameObjects;
using CowMouse.Utilities.Pathfinding;
using CowMouse.Buildings;

namespace CowMouse.NPCs
{
    class LogHunter : TownsMan
    {
        /// <summary>
        /// The maximum cost of any path to be hunted for.  Corresponds roughly to how
        /// long the longest path is, before the path algorithm just throws up its hands
        /// and leaves you to your own devices.
        /// </summary>
        protected const int DefaultSearchDepth = 300;

        public LogHunter(CowMouseGame game, int xCoordinate, int yCoordinate, bool usingTileCoordinates, TileMap map)
            : base(game, xCoordinate, yCoordinate, usingTileCoordinates, map)
        {
            mentalState = AIState.Start;
        }

        public bool IsCarryingItem { get { return hauledItem != null; } }
        protected Carryable hauledItem;

        protected AIState mentalState;

        public override void Update()
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
                endOfPath();
                
            }
        }

        #region AI Switch
        /// <summary>
        /// Basically just a switch statement for what happens when we run out of path.
        /// </summary>
        private void endOfPath()
        {
            switch (mentalState)
            {
                case AIState.Start: endPathStart();
                    break;

                case AIState.Looking_For_Resource:
                    endPathLookingForResource();
                    break;

                case AIState.Bringing_Resource_To_Stockpile:
                    endPathBringingResourceToStockpile();
                    break;

                case AIState.No_Task:
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Start condition; just switch to "looking for resource."
        /// </summary>
        private void endPathStart()
        {
            changeMentalStateTo(AIState.Looking_For_Resource);
        }

        /// <summary>
        /// End of path for looking for resource; if there was something
        /// here, pick it up and start hunting for a stockpile.  Otherwise
        /// throw an error in confusion.
        /// </summary>
        private void endPathLookingForResource()
        {
            if (IsCarryingItem)
                throw new InvalidOperationException("Shouldn't be looking for resource when we're holding something?");

            //we finished our path, so we should be standing on something that we
            //marked for collection earlier, so find it
            Point myPoint = SquareCoordinate();
            foreach (Carryable car in Game.WorldManager.Carryables)
            {
                if (isThisMyMarkedItem(myPoint, car))
                {
                    pickUp(car);
                    break;
                }
            }

            //Now either we found something or not
            if (IsCarryingItem)
            {
                changeMentalStateTo(AIState.Bringing_Resource_To_Stockpile);
            }
            else
            {
                //if not, this is weird
                throw new InvalidOperationException("There really should have been a resource here...");
            }
        }

        /// <summary>
        /// If we're on a stockpile, drop what we're holding and move on.
        /// If not, try again to find a path to a stockpile
        /// </summary>
        private void endPathBringingResourceToStockpile()
        {
            Point myPoint = SquareCoordinate();

            bool isOnStockPile = false;
            foreach (Building b in Game.WorldManager.Stockpiles)
            {
                if (b.ContainsCell(myPoint.X, myPoint.Y))
                {
                    isOnStockPile = true;
                    break;
                }
            }

            if (!isOnStockPile)
            {
                Path path = PathHunter.GetPath(myPoint, Game.WorldManager.Stockpiles, DefaultSearchDepth, Game.WorldManager);
                if (path != null)
                {
                    loadPathIntoQueue(path);
                }
            }
            else
            {
                putDownItem();
                changeMentalStateTo(AIState.Looking_For_Resource);
            }
        }

        /// <summary>
        /// This is a 2-level switch statement which is invoked when we change state from one into another.
        /// </summary>
        /// <param name="newState"></param>
        private void changeMentalStateTo(AIState newState)
        {
            AIState oldState = mentalState;
            mentalState = newState;

            switch (newState)
            {
                case AIState.Looking_For_Resource:
                    switch (oldState)
                    {
                        case AIState.Start:
                        case AIState.Bringing_Resource_To_Stockpile:
                            startToLookingForResource();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case AIState.Bringing_Resource_To_Stockpile:
                    switch (oldState)
                    {
                        case AIState.Looking_For_Resource:
                            lookingForResourcesToBringingResourceToStockpile();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case AIState.No_Task:
                    //do nothing
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Once we're holding something, try to find a stockpile.
        /// If we found a path, follow it; if not, just do nothing.
        /// </summary>
        private void lookingForResourcesToBringingResourceToStockpile()
        {
            if (!IsCarryingItem)
                throw new InvalidOperationException("Shouldn't be looking for a stockpile if we have nothing to put there!");

            //now it's time to find a stockpile
            Point myPoint = SquareCoordinate();
            Path path = PathHunter.GetPath(myPoint, Game.WorldManager.Stockpiles, DefaultSearchDepth, Game.WorldManager);

            if (path != null) //if we found a path, follow it
            {
                loadPathIntoQueue(path);
            }
            else //if we didn't find a path, it means there are no stockpiles nearby, so just try again later
            {
                //do nothing
            }
        }

        /// <summary>
        /// Find a path to a resource.
        /// If we found one, mark the resource we found, and move toward it.
        /// If we didn't find one, we must be done, so relax (change to No_Task).
        /// </summary>
        private void startToLookingForResource()
        {
            //two ways this can go down; either we find something or not.
            Path path = PathHunter.GetPath(SquareCoordinate(), resourcePositions(), DefaultSearchDepth, Game.WorldManager);

            //if we found nothing, we must be done; relax.
            if (path == null)
            {
                changeMentalStateTo(AIState.No_Task);
            }
            else //otherwise, go down that path
            {
                loadPathIntoQueue(path);

                Carryable resource = null;

                //also, find the resource that we latched onto earlier
                foreach (Carryable car in Game.WorldManager.Carryables)
                {
                    if (isValidForHauling(car) && car.SquareCoordinate() == path.End)
                    {
                        resource = car;
                        break;
                    }
                }

                if (resource == null)
                {
                    throw new InvalidOperationException("Why didn't we find any...?");
                }
                else
                {
                    resource.MarkForCollection(this);
                }
            }
        }

        /// <summary>
        /// Pick up the specified item, and all that entails.
        /// </summary>
        /// <param name="resource"></param>
        private void pickUp(Carryable resource)
        {
            resource.GetPickedUp(this);
            this.hauledItem = resource;
        }

        /// <summary>
        /// Put down any hauled item, and all that entails.
        /// Throws a fit if we're not actually hauling anything.
        /// </summary>
        private void putDownItem()
        {
            hauledItem.GetPutDown();
            hauledItem = null;
        }
        #endregion

        /// <summary>
        /// Just enumerates the valid resource positions; that is, the
        /// things which can be picked up, are not in stockpiles, and which
        /// are not marked for pickup yet.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Point> resourcePositions()
        {
            foreach (Carryable car in Game.WorldManager.Carryables)
            {
                if (isValidForHauling(car))
                    yield return car.SquareCoordinate();
            }
        }

        /// <summary>
        /// Determines if this resource is one we should be looking
        /// for when we're looking for something to haul.
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private static bool isValidForHauling(Carryable car)
        {
            return car.IsResource &&
                car.CanBePickedUp &&
                !car.IsInStockpile &&
                !car.IsMarkedForCollection;
        }

        /// <summary>
        /// Determines whether this is the Carryable we marked for collection, and whether we're standing on it.
        /// </summary>
        /// <param name="myPoint"></param>
        /// <param name="car"></param>
        /// <returns></returns>
        private bool isThisMyMarkedItem(Point myPoint, Carryable car)
        {
            return car.IsMarkedForCollection &&
                car.IntendedCollector == this &&
                car.SquareCoordinate() == myPoint;
        }

        /// <summary>
        /// Given a path, load it into the destinations queue
        /// </summary>
        /// <param name="newPath"></param>
        private void loadPathIntoQueue(Path newPath)
        {
            foreach (Point point in newPath.PointsTraveled())
                QueuedDestinations.Enqueue(point);
        }
    }

    #region States
    public enum AIState
    {
        Start,
        Looking_For_Resource,
        Bringing_Resource_To_Stockpile,
        No_Task
    }
    #endregion
}
