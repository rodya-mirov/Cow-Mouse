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
    ///           this could be used for anything)
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
    ///        if we're between squares, finish that up
    ///        if there's an interruption (e.g. exhaustion), cope, return
    ///        if there is a path, follow it, return
    ///        else invoke endOfPath
    ///        
    ///     endofPath():
    ///        do any simple actions available
    ///        determine any mental state changes necessary
    ///        if changeMentalStateTo is not called, will invoke endOfPath every update
    /// </summary>
    class NPC : Person
    {
        public NPC(CowMouseGame game, int xCoordinate, int yCoordinate, bool usingTileCoordinates, TileMap map)
            : base(game, xCoordinate, yCoordinate, usingTileCoordinates, map)
        {
            mentalState = AIState.Undecided;
            thinkingThread = null;

            Random ran = new Random();
            currentEnergy = ran.Next(sleepUntil);
        }

        #region Hauling
        /// <summary>
        /// The item, if any, which is being hauled to a stockpile.
        /// </summary>
        protected Carryable hauledItem;

        /// <summary>
        /// Whether or not this NPC is currently carrying an item (for
        /// the purpose of hauling it to a stockpile)
        /// </summary>
        public bool IsCarryingItem { get { return hauledItem != null; } }

        /// <summary>
        /// Pick up the specified item, and all that entails.
        /// </summary>
        /// <param name="resource"></param>
        private void pickUpItem(Carryable resource)
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
        /// Just determines whether there is a stockpile we could haul
        /// something to.
        /// </summary>
        /// <returns></returns>
        private bool StockpilesExist()
        {
            foreach (Point p in Game.WorldManager.StockpilePositions)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether or not something exists that is worth hauling.
        /// </summary>
        /// <returns></returns>
        private bool ResourcesExist()
        {
            foreach (Carryable car in Game.WorldManager.Carryables)
            {
                if (isValidForHauling(car))
                    return true;
            }

            return false;
        }
        #endregion

        #region Sleeping
        private int currentEnergy;
        private int sleepUntil = 10000;
        private int tiredThreshold = 3500;

        /// <summary>
        /// Get slightly more tired.  Should be called
        /// once per update in normal circumstances.
        /// </summary>
        private void getMoreTired()
        {
            if (mentalState == AIState.Sleeping)
            {
                //if sleeping, gain energy
                currentEnergy += 2;

                //color them for sleeping
                Tint = new Color(180, 180, 255); //somewhat blue
            }
            else
            {
                //if not sleeping, get more tired, to a minimum of zero
                if (currentEnergy > 0)
                    currentEnergy -= 1;

                //adjust tint
                if (currentEnergy < tiredThreshold)
                    Tint = Color.LightPink;
                else if (currentEnergy <= 0)
                    Tint = Color.Red;
                else
                    Tint = Color.White;
            }
        }

        /// <summary>
        /// Determines whether the NPC is tired enough to find a bedroom
        /// </summary>
        /// <returns></returns>
        private bool isTired()
        {
            return currentEnergy < tiredThreshold;
        }

        /// <summary>
        /// Determines whether the NPC is tired enough to just sleep where
        /// he's standing.
        /// </summary>
        /// <returns></returns>
        private bool isExhausted()
        {
            return currentEnergy <= 0;
        }

        /// <summary>
        /// Determines whether the NPC is energetic enough to wake up.
        /// </summary>
        /// <returns></returns>
        private bool shouldWakeUp()
        {
            return currentEnergy >= sleepUntil;
        }
        #endregion

        private GameTime lastUpdateTime;

        public override void Update(GameTime time)
        {
            this.lastUpdateTime = time;

            getMoreTired();

            if (IsThinking)
            {
                //do nothing while we're thinking :P
            }
            else if (HasDestination) //go somewhere if possible, moving slowly along the path
            {
                MoveTowardDestination();
            }
            else if (isExhausted()) //if we just don't have the energy to move anymore, pass out
            {
                passOut();
            }
            else if (QueuedDestinations.Count > 0 && VerifyPath()) //if we're there, but it was just a corner, queue up the next destination
            {
                SetDestination(QueuedDestinations.Dequeue());
            }
            else //otherwise we're REALLY there, so process that
            {
                endOfPath();
            }
        }

        #region Paths
        /// <summary>
        /// Checks if we can move from our current position to the
        /// next position on the queue, and from there to the next,
        /// and so on until the end.
        /// </summary>
        /// <returns></returns>
        private bool VerifyPath()
        {
            Point current = this.SquareCoordinate();
            foreach (Point next in QueuedDestinations)
            {
                if (current != next &&
                    !Game.WorldManager.CanMoveFromSquareToSquare(current.X, current.Y, next.X, next.Y))
                {
                    return false;
                }

                current = next;
            }

            return true;
        }

        /// <summary>
        /// Given a path, load it into the destinations queue
        /// </summary>
        /// <param name="newPath"></param>
        private void loadPathIntoQueue(Path newPath)
        {
            QueuedDestinations.Clear();

            foreach (Point point in newPath.PointsTraveled())
                QueuedDestinations.Enqueue(point);
        }
        #endregion

        #region AI
        /// <summary>
        /// Whether or not the thinking thread is running;
        /// generally we shouldn't be doing anything while it
        /// is, since it might screw up the thought process
        /// and/or create race conditions.
        /// </summary>
        protected bool IsThinking
        {
            get { return thinkingThread != null; }
        }

        /// <summary>
        /// The maximum cost of any path to be hunted for.  Corresponds roughly to how
        /// long the longest path is, before the path algorithm just throws up its hands
        /// and leaves you to your own devices.
        /// </summary>
        protected const int DefaultSearchDepth = 300;

        protected AIState mentalState;
        protected Thread thinkingThread;

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
                case AIState.Undecided:
                    startStateNoTask(oldState);
                    break;

                case AIState.Finding_Resource_To_Haul:
                    startLookingForResource();
                    break;

                case AIState.Bringing_Resource_To_Stockpile:
                    startStateBringToStockpile();
                    break;

                case AIState.Looking_For_Bed:
                    startStateLookForBed();
                    break;

                case AIState.Sleeping:
                    startStateSleep();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// This is one of two workhorses for the AI, although it's
        /// mainly a switchboard.  Fundamentally, this deals with the END
        /// of an action- doing things, rather than deciding things.
        /// </summary>
        private void endOfPath()
        {
            switch (mentalState)
            {
                case AIState.Undecided:
                    endPathNoTask();
                    break;

                case AIState.Finding_Resource_To_Haul:
                    endPathLookingForResource();
                    break;

                case AIState.Bringing_Resource_To_Stockpile:
                    endPathBringingResourceToStockpile();
                    break;

                case AIState.Looking_For_Bed:
                    endPathLookingForBed();
                    break;

                case AIState.Sleeping:
                    endPathSleeping();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Starts the thinking thread.
        /// </summary>
        private void startThinkingThread()
        {
            //it is conceivable that there will eventually be more to this.
            thinkingThread.Start();
        }

        #region Undecided AI
        /// <summary>
        /// Start condition; we have no task.
        /// 
        /// This is the part where we decide what to do next.
        /// </summary>
        private void endPathNoTask()
        {
            changeMentalStateTo(AIState.Undecided);
        }

        private void startStateNoTask(AIState previousState)
        {
            //if we're tired and we didn't just give up on finding a bed...
            if (isTired() && previousState != AIState.Looking_For_Bed)
            {
                changeMentalStateTo(AIState.Looking_For_Bed);
            }
            else if (StockpilesExist() && ResourcesExist())
            {
                changeMentalStateTo(AIState.Finding_Resource_To_Haul);
            }
        }
        #endregion

        #region Hauling AI
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
                    pickUpItem(car);
                    break;
                }
            }

            //Now either we found something or not
            if (IsCarryingItem) //if so, bring it away
            {
                changeMentalStateTo(AIState.Bringing_Resource_To_Stockpile);
            }
            else //if not, maybe find another task?
            {
                changeMentalStateTo(AIState.Undecided);
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
            foreach (Building b in Game.WorldManager.Buildings)
            {
                if (b.IsStockpile && b.ContainsCell(myPoint.X, myPoint.Y))
                {
                    isOnStockPile = true;
                    break;
                }
            }

            if (isOnStockPile) //if we found a stockpile, put our stuff down and move on
            {
                putDownItem();
                changeMentalStateTo(AIState.Undecided);
            }
            else //if we didn't find the stockpile
            {
                if (isTired())
                {
                    //if we're tired, give up and find a new task
                    putDownItem();
                    changeMentalStateTo(AIState.Undecided);
                }
                else
                {
                    //if we're not tired, keep trying
                    changeMentalStateTo(AIState.Bringing_Resource_To_Stockpile);
                }
            }
        }

        /// <summary>
        /// Once we're holding something, try to find a stockpile.
        /// If we found a path, follow it; if not, just do nothing.
        /// </summary>
        private void startStateBringToStockpile()
        {
            if (!IsCarryingItem)
                throw new InvalidOperationException("Shouldn't be looking for a stockpile if we have nothing to put there!");
            
            if (StockpilesExist())
            {
                HashSet<Point> destinations = new HashSet<Point>(Game.WorldManager.StockpilePositions);

                if (destinations.Count > 0)
                {
                    thinkingThread = new Thread(() => bringToStockpile_ThreadHelper(destinations));
                    startThinkingThread();
                }
                else
                {
                    throw new InvalidOperationException("We *just* checked that was nonempty!");
                }
            }
        }

        private void bringToStockpile_ThreadHelper(HashSet<Point> destinations)
        {
            //now it's time to find a stockpile
            Path path = PathHunter.GetPath(SquareCoordinate(), destinations, DefaultSearchDepth, Game.WorldManager, lastUpdateTime);

            if (path != null)
                loadPathIntoQueue(path);

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
            HashSet<Point> destinations = new HashSet<Point>();
            foreach (Carryable car in Game.WorldManager.Carryables)
            {
                if (isValidForHauling(car))
                {
                    validHaulsExist = true;
                    car.MarkForCollection(this);
                    destinations.Add(car.SquareCoordinate());
                }
            }

            if (validHaulsExist)
            {
                thinkingThread = new Thread(() => startLookingForResource_ThreadHelper(destinations));
                startThinkingThread();
            }
        }

        private void startLookingForResource_ThreadHelper(HashSet<Point> destinations)
        {
            //two ways this can go down; either we find something or not.
            Path path = PathHunter.GetPath(SquareCoordinate(), destinations, DefaultSearchDepth, Game.WorldManager, this.lastUpdateTime);

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
        #endregion

        #region Sleeping AI
        /// <summary>
        /// Hunt for a bedroom, if one exists.
        /// </summary>
        private void startStateLookForBed()
        {
            bool bedroomsExist = false;
            foreach (Point p in Game.WorldManager.BedroomPositions)
            {
                bedroomsExist = true;
                break;
            }

            if (bedroomsExist)
            {
                HashSet<Point> destinations = new HashSet<Point>(Game.WorldManager.BedroomPositions);

                if (destinations.Count > 0)
                {
                    thinkingThread = new Thread(() => lookForBed_ThreadHelper(destinations));
                    startThinkingThread();
                }
                else
                {
                    throw new InvalidOperationException("We *just* checked that was nonempty!");
                }
            }
        }

        private void lookForBed_ThreadHelper(HashSet<Point> destinations)
        {
            Path path = PathHunter.GetPath(SquareCoordinate(), destinations, DefaultSearchDepth, Game.WorldManager, lastUpdateTime);

            if (path != null)
                loadPathIntoQueue(path);

            thinkingThread = null;
        }

        /// <summary>
        /// Deal with the end of path, when we're looking for bed.
        /// </summary>
        private void endPathLookingForBed()
        {
            Point myPoint = SquareCoordinate();
            bool isInBedroom = false;

            foreach (Building b in Game.WorldManager.Buildings)
            {
                if (b.IsBedroom && b.ContainsCell(myPoint.X, myPoint.Y))
                {
                    isInBedroom = true;
                    break;
                }
            }

            if (!isInBedroom)
            {
                //if we didn't find a bedroom, try something else
                changeMentalStateTo(AIState.Undecided);
            }
            else
            {
                //if we did find a bedroom, go to sleep
                changeMentalStateTo(AIState.Sleeping);
            }
        }

        /// <summary>
        /// Sleep until we're done sleeping!
        /// </summary>
        private void startStateSleep()
        {
            if (shouldWakeUp())
                changeMentalStateTo(AIState.Undecided);

            //else nothing!  just relax :)
        }

        /// <summary>
        /// When you're sleeping, there's nowhere to go :)
        /// </summary>
        private void endPathSleeping()
        {
            changeMentalStateTo(AIState.Sleeping);
        }

        /// <summary>
        /// When you just can't go anymore, sleep where you are.
        /// </summary>
        private void passOut()
        {
            //drop your possessions...
            if (IsCarryingItem)
                putDownItem();

            //...forget your cares...
            QueuedDestinations.Clear();

            //and sleep!
            changeMentalStateTo(AIState.Sleeping);
        }
        #endregion

        #endregion
    }

    #region States
    public enum AIState
    {
        Undecided,

        //these two are really the same task,
        //but split into two states
        Finding_Resource_To_Haul,       //enter state
        Bringing_Resource_To_Stockpile, //exit state

        //these two are really the same task,
        //but split into two states
        Looking_For_Bed,                //enter state
        Sleeping                        //exit state
    }
    #endregion
}
