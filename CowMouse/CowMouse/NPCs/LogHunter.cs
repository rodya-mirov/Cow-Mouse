using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework;
using CowMouse.InGameObjects;
using CowMouse.Buildings;
using System.Threading;
using TileEngine.Utilities.Pathfinding;

namespace CowMouse.NPCs
{
    /// <summary>
    /// Townsperson.
    /// 
    /// AI paradigm:
    /// 
    /// There are three phases of the AI structure (any of which can be
    /// skipped by the obvious "not doing anything at this stage" code).
    /// 
    /// 1) Thinking
    ///     invoked by: changeMentalStateTo(AIState)
    ///     overview: this determines what the person will be doing for
    ///           a while.  Typically this includes some pathing, which
    ///           will involve the thinkingThread (though technically
    ///           this could be used for anything, I guess)
    ///           
    /// 2) Moving
    ///     invoked by: (nothing; handled by default during the update() method)
    ///     overview: if there is a path, it will automatically move along
    ///           it without outside interference.
    ///           
    /// 3) Doing
    ///     invoked by: endOfPath()
    ///     overview: after we've done our moving, we should be in a
    ///           position to start doing things.  So, do them (typically
    ///           simple), then invoke the next cycle by setting the
    ///           new mental state (if this isn't done, endOfPath will repeatedly
    ///           be called)
    /// 
    /// Flow:
    ///     update():
    ///        if thinking, return
    ///        if there is a path, follow it, return
    ///        else invoke endOfPath
    ///        
    ///     endofPath():
    ///        do any simple actions available
    ///        determine any mental state changes necessary
    ///        if changeMentalStateTo is not called, will invoke endOfPath every update
    /// </summary>
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
            thinkingThread = null;
        }

        public bool IsCarryingItem { get { return hauledItem != null; } }
        protected Carryable hauledItem;

        protected bool IsThinking
        {
            get { return thinkingThread != null; }
        }

        protected AIState mentalState;
        protected Thread thinkingThread;

        public override void Update()
        {
            if (IsThinking)
            {
                //do nothing while we're thinking :P
            }
            else if (HasDestination) //go somewhere if possible
            {
                MoveTowardDestination();
            }
            else if (QueuedDestinations.Count > 0) //if we're there, but it was just a corner, queue up the next destination
            {
                SetDestination(QueuedDestinations.Dequeue());
            }
            else //otherwise we're REALLY there, so process that
            {
                endOfPath();
            }
        }

        #region AI Switch
        /// <summary>
        /// This is one of two workhorses for the AI, although it's
        /// mainly a switchboard.  Fundamentally, this deals with the END
        /// of an action- doing things, rather than deciding things.
        /// </summary>
        private void endOfPath()
        {
            switch (mentalState)
            {
                case AIState.Start:
                    endPathStart();
                    break;

                case AIState.Looking_For_Resource:
                    endPathLookingForResource();
                    break;

                case AIState.Bringing_Resource_To_Stockpile:
                    endPathBringingResourceToStockpile();
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
        /// End of path for looking for resource.
        /// 
        /// There are two ways this could happen.
        /// 1) We ended a path looking for a resource
        ///         In this case, we should be standing on
        ///         a resource, so pick it up and carry it
        ///         to a stockpile.
        /// 2) We were in the looking for resource state,
        ///    but didn't find anything.
        ///         In this case, try again on the pathing
        ///         thing.
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
            if (IsCarryingItem) //if so, bring it away
            {
                changeMentalStateTo(AIState.Bringing_Resource_To_Stockpile);
            }
            else //if not, try again on this one
            {
                changeMentalStateTo(AIState.Looking_For_Resource);
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
            foreach (Point p in Game.WorldManager.StockpilePositions)
            {
                if (p == myPoint)
                {
                    isOnStockPile = true;
                    break;
                }
            }

            if (!isOnStockPile)
            {
                changeMentalStateTo(AIState.Bringing_Resource_To_Stockpile);
            }
            else
            {
                putDownItem();
                changeMentalStateTo(AIState.Looking_For_Resource);
            }
        }

        /// <summary>
        /// This method is one of the two workhorses for the AI (mainly as a switchboard, admittedly).
        /// This is the BEGINNING of an action, so frequently involves the thinking thread and setting up
        /// a path.
        /// </summary>
        /// <param name="newState"></param>
        private void changeMentalStateTo(AIState newState)
        {
            AIState oldState = mentalState;
            mentalState = newState;

            switch (newState)
            {
                case AIState.Looking_For_Resource:
                    startLookingForResource();
                    break;

                case AIState.Bringing_Resource_To_Stockpile:
                    bringToStockpile();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Once we're holding something, try to find a stockpile.
        /// If we found a path, follow it; if not, just do nothing.
        /// </summary>
        private void bringToStockpile()
        {
            if (!IsCarryingItem)
                throw new InvalidOperationException("Shouldn't be looking for a stockpile if we have nothing to put there!");

            //this particular bit of weirdness is to check if it's worth looking for a path,
            //because starting and stopping multiple threads every frame can be a problem
            bool stockpilesExist = false;
            foreach (Point p in Game.WorldManager.StockpilePositions)
            {
                stockpilesExist = true;
                break;
            }

            if (stockpilesExist)
            {
                thinkingThread = new Thread(new ThreadStart(bringToStockpile_ThreadHelper));
                thinkingThread.Start();
            }
        }

        private void bringToStockpile_ThreadHelper()
        {
            //now it's time to find a stockpile
            Path path = PathHunter.GetPath(SquareCoordinate(), Game.WorldManager.StockpilePositions, DefaultSearchDepth, Game.WorldManager);

            if (path != null) //if we found a path, follow it
            {
                loadPathIntoQueue(path);
            }
            else //if we didn't find a path, it means there are no stockpiles nearby, so just try again next frame
            {
                //do nothing
            }

            thinkingThread = null;
        }

        /// <summary>
        /// Find a path to a resource.
        /// If we found one, mark the resource we found, and move toward it.
        /// If we didn't find one, we must be done, so relax (change to No_Task).
        /// </summary>
        private void startLookingForResource()
        {
            //first, we need to mark every possible hauling candidate
            bool validHaulsExist = false;
            foreach (Carryable car in Game.WorldManager.Carryables)
            {
                if (isValidForHauling(car))
                {
                    validHaulsExist = true;
                    car.MarkForCollection(this);
                }
            }

            if (validHaulsExist)
            {
                thinkingThread = new Thread(new ThreadStart(this.startLookingForResource_ThreadHelper));
                thinkingThread.Start();
            }
        }

        private void startLookingForResource_ThreadHelper()
        {
            //two ways this can go down; either we find something or not.
            Path path = PathHunter.GetPath(SquareCoordinate(), positionsMarkedForCollection(), DefaultSearchDepth, Game.WorldManager);

            //if we found nothing, try again next frame
            if (path == null)
            {
                //relax
            }
            else //otherwise, go down that path
            {
                loadPathIntoQueue(path);

                Carryable resource = null;

                //also, find the resource that we latched onto earlier
                foreach (Carryable car in Game.WorldManager.Carryables)
                {
                    if (car.IsMarkedForCollection &&
                        car.IntendedCollector == this &&
                        car.SquareCoordinate() == path.End)
                    {
                        resource = car;
                        break;
                    }
                }

                //we really should have found a resource, since we pathed to one
                if (resource == null)
                    throw new InvalidOperationException("Why didn't we find any...?");

                //and unmark everything else (we really did mark a lot didn't we?)
                foreach (Carryable car in Game.WorldManager.Carryables)
                {
                    if (car != resource && car.IsMarkedForCollection && car.IntendedCollector == this)
                    {
                        car.UnMarkForCollection(this);
                    }
                }
            }

            thinkingThread = null;
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
        /// Just enumerates the positions of all the resources this object
        /// has already called dibs on.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Point> positionsMarkedForCollection()
        {
            foreach (Carryable car in Game.WorldManager.Carryables)
            {
                if (car.IsMarkedForCollection && car.IntendedCollector == this)
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
        Bringing_Resource_To_Stockpile
    }
    #endregion
}
