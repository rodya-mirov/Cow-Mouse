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
using CowMouse.NPCs.Utilities;

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
    public class NPC : Person
    {
        #region Tags
        public override bool IsNPC { get { return true; } }
        #endregion

        private static Random ran;

        /// <summary>
        /// The tint to paint the sprite.  Used primarily to show the mood
        /// or other state of the NPC.
        /// </summary>
        public override Color Tint
        {
            get
            {
                if (IsInhabited)
                    return Color.Yellow;
                else
                {
                    if (isSleeping())
                        return new Color(180, 180, 255);

                    if (isTired())
                        return Color.LightPink;

                    if (isExhausted())
                        return Color.Red;

                    return Color.White;
                }
            }
        }

        public NPC(int xCoordinate, int yCoordinate, bool usingTileCoordinates, WorldManager manager)
            : base(xCoordinate, yCoordinate, usingTileCoordinates, manager)
        {
            thinkingThread = null;

            if (ran == null)
                ran = new Random();

            currentEnergy = ran.Next(sleepUntil);

            setUpAI();
        }

        #region Inhabitation
        public override void Inhabit()
        {
            getInterrupted();
            base.Inhabit();
        }

        public override void Release()
        {
            getInterrupted();
            base.Release();
            changeMentalStateTo(AIState.Undecided);
        }
        #endregion

        #region Hauling
        /// <summary>
        /// The item, if any, which is being hauled to a stockpile.
        /// </summary>
        protected Carryable heldItem;

        /// <summary>
        /// Whether or not this NPC is currently carrying an item (for
        /// the purpose of hauling it to a stockpile)
        /// </summary>
        public bool IsCarryingItem { get { return heldItem != null; } }

        /// <summary>
        /// Pick up the specified item, and all that entails.
        /// </summary>
        /// <param name="resource"></param>
        private void pickUpItem(Carryable resource)
        {
            resource.GetPickedUp(this);
            this.heldItem = resource;
        }

        /// <summary>
        /// Put down any hauled item, and all that entails.
        /// Throws a fit if we're not actually hauling anything.
        /// </summary>
        private void putDownItem(bool isInStockpile)
        {
            heldItem.GetPutDown();
            heldItem.IsInStockpile = isInStockpile;
            heldItem = null;
        }

        /// <summary>
        /// Just enumerates the positions of all the resources this object
        /// has already called dibs on.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Point> positionsMarkedForCollection()
        {
            foreach (Carryable car in WorldManager.Carryables)
            {
                if (car.IsMarkedForCollection && car.IntendedCollector == this)
                    yield return car.SquareCoordinate;
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
                car.SquareCoordinate == myPoint;
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
            if (isSleeping())
            {
                //if sleeping, gain energy
                currentEnergy += 2;
            }
            else
            {
                //if not sleeping, get more tired, to a minimum of zero
                if (currentEnergy > 0)
                    currentEnergy -= 1;
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

        private bool isSleeping()
        {
            switch (mentalState)
            {
                case AIState.PassedOut:
                case AIState.Sleeping_Unconscious:
                    return true;

                default:
                    return false;
            }
        }
        #endregion

        /// <summary>
        /// The general update method when this NPC is being
        /// controlled by its AI.
        /// </summary>
        protected override void aiUpdate()
        {
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
            else if (!VerifyPath())
            {
                getInterrupted();
                changeMentalStateTo(AIState.Undecided);
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

        #region Paths
        /// <summary>
        /// Checks if we can move from our current position to the
        /// next position on the queue, and from there to the next,
        /// and so on until the end.
        /// 
        /// Note the empty path yields true.
        /// </summary>
        /// <returns></returns>
        private bool VerifyPath()
        {
            Point current = this.SquareCoordinate;
            foreach (Point next in QueuedDestinations)
            {
                if (current != next &&
                    !WorldManager.CanMoveFromSquareToSquare(current.X, current.Y, next.X, next.Y))
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
        private void setRoute(Path newPath)
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
            get
            {
                return thinkingThread != null;
            }
        }

        /// <summary>
        /// The actions invoked by a change of state.  The argument is
        /// the previous state!
        /// </summary>
        protected Dictionary<AIState, Action<AIState>> stateChangeActions;

        /// <summary>
        /// The actions invoked by reaching the end of the current route.
        /// </summary>
        protected Dictionary<AIState, Action> pathEndActions;

        /// <summary>
        /// The actions invoked when something interrupts the current task.
        /// </summary>
        protected Dictionary<AIState, Action> interruptedActions;

        /// <summary>
        /// Load up the AI
        /// </summary>
        protected void setUpAI()
        {
            setUpStateChangeActions();
            setUpPathEndActions();
            setUpInterruptedActions();
        }

        /// <summary>
        /// Rig up the AI's state change actions
        /// </summary>
        protected void setUpStateChangeActions()
        {
            stateChangeActions = new Dictionary<AIState, Action<AIState>>();

            stateChangeActions[AIState.Undecided] = startStateNoTask;

            stateChangeActions[AIState.Sleeping_Thinking] = startPhase_Sleeping;
            stateChangeActions[AIState.Sleeping_GoingToBedroom] = startSubPhase_Sleeping_GoingToBedroom;
            stateChangeActions[AIState.Sleeping_Unconscious] = startSubPhase_Sleeping_Unconscious;

            stateChangeActions[AIState.PassedOut] = startPhase_PassedOut;

            stateChangeActions[AIState.Hauling_Thinking] = startPhase_Hauling;
            stateChangeActions[AIState.Hauling_FindingResource] = startSubPhase_Hauling_FindingResource;
            stateChangeActions[AIState.Hauling_BringingResource] = startSubPhase_Hauling_BringingResource;
        }

        /// <summary>
        /// Rigs up the AI's end of path actions.
        /// </summary>
        protected void setUpPathEndActions()
        {
            pathEndActions = new Dictionary<AIState, Action>();

            pathEndActions[AIState.Undecided] = endPathNoTask;

            pathEndActions[AIState.Sleeping_GoingToBedroom] = endOfPath_Sleeping_GoingToBedroom;
            pathEndActions[AIState.Sleeping_Unconscious] = endOfPath_Sleeping_Unconscious;

            pathEndActions[AIState.PassedOut] = endOfPath_PassedOut;

            pathEndActions[AIState.Hauling_FindingResource] = endOfPath_Hauling_FindingResource;
            pathEndActions[AIState.Hauling_BringingResource] = endOfPath_Hauling_BringingResource;

            pathEndActions[AIState.Hauling_Thinking] = endOfPath_Thinking;
            pathEndActions[AIState.Sleeping_Thinking] = endOfPath_Thinking;
        }

        /// <summary>
        /// Sets up the dictionary of interruption actions.
        /// </summary>
        protected void setUpInterruptedActions()
        {
            interruptedActions = new Dictionary<AIState, Action>();

            interruptedActions[AIState.Undecided] = interruptedNoTask;

            interruptedActions[AIState.Hauling_FindingResource] = interrupted_Hauling_FindingResource;
            interruptedActions[AIState.Hauling_BringingResource] = interrupted_Hauling_BringingResource;

            interruptedActions[AIState.Sleeping_GoingToBedroom] = interrupted_Sleeping_GoingToBedroom;
            interruptedActions[AIState.Sleeping_Unconscious] = interrupted_Sleeping_Unconscious;

            interruptedActions[AIState.PassedOut] = interrupted_PassedOut;

            interruptedActions[AIState.Sleeping_Thinking] = interruptedThinking;
            interruptedActions[AIState.Hauling_Thinking] = interruptedThinking;
        }

        /// <summary>
        /// The maximum cost of any path to be hunted for.  Corresponds roughly to how
        /// long the longest path is, before the path algorithm just throws up its hands
        /// and leaves you to your own devices.
        /// </summary>
        protected const int DefaultSearchDepth = 300;

        protected AIState mentalState = AIState.Undecided;
        protected Thread thinkingThread = null;

        protected Queue<Task> StoredGoals = new Queue<Task>();
        protected Task CurrentGoal = null;

        /// <summary>
        /// This is the beginning of a new action, and there's
        /// very little cleanup/setup to do.  It just changes
        /// the mental state, then calls the stateChangeAction
        /// corresponding to the new state.
        /// </summary>
        /// <param name="newState"></param>
        private void changeMentalStateTo(AIState newState)
        {
            AIState oldState = mentalState;
            mentalState = newState;

            stateChangeActions[mentalState].Invoke(oldState);
        }

        /// <summary>
        /// This is called typically at the end of an action
        /// and just invokes the pathEndAction associated to
        /// the current mentalState.
        /// </summary>
        private void endOfPath()
        {
            pathEndActions[mentalState].Invoke();
        }

        /// <summary>
        /// Handles task interruptions; calls any state-specific
        /// code from interruptedActions, and also calls
        /// cancelAndCleanUp (afterward).
        /// </summary>
        private void getInterrupted()
        {
            if (IsThinking)
                throw new NotImplementedException("I honestly haven't figured out what to do here...");

            interruptedActions[mentalState].Invoke();
            CancelAndCleanUp();
        }

        /// <summary>
        /// Starts the thinking thread.
        /// </summary>
        private void startThinkingThread()
        {
            //it is conceivable that there will eventually be more to this.
            thinkingThread.Start();
        }

        /// <summary>
        /// Typically invoked when a pathing attempt failed before it started (because
        /// there were no feasible destinations, usually).  Just sets the mentalState
        /// back to Undecided.
        /// </summary>
        private void endOfPath_Thinking()
        {
            changeMentalStateTo(AIState.Undecided);
        }

        private void interruptedThinking()
        {
            //do nothing
        }

        /// <summary>
        /// Cancels current goals and cleans up all stored actions and task-oriented
        /// foolishness, getting the NPC into a reasonably blank state.
        /// </summary>
        protected void CancelAndCleanUp()
        {
            if (CurrentGoal != null)
                CurrentGoal.CleanUp();

            while (StoredGoals.Count > 0)
                StoredGoals.Dequeue().CleanUp();

            CurrentGoal = null;

            QueuedDestinations.Clear();
            SetDestination(this.SquareCoordinate);

            changeMentalStateTo(AIState.Undecided);
        }

        #region Undecided AI
        /// <summary>
        /// Switching to undecided does nothing until the next turn.
        /// </summary>
        /// <param name="previousState"></param>
        private void startStateNoTask(AIState previousState)
        {
            //does nothing; this is handled in the endPathNoTask thing.
        }

        /// <summary>
        /// Start condition; we have no task.
        /// 
        /// This is the part where we decide what to do next.
        /// </summary>
        private void endPathNoTask()
        {
            //if we're tired and we didn't just give up on finding a bed...
            if (isTired())
            {
                changeMentalStateTo(AIState.Sleeping_Thinking);
            }
            else
            {
                changeMentalStateTo(AIState.Hauling_Thinking);
            }
        }

        private void interruptedNoTask()
        {
            //nothing :)
        }
        #endregion

        #region Hauling AI
        /// <summary>
        /// Hauling is a two-fold process; finding an item, then bringing it
        /// to a stockpile.  This requires two paths, both of which are found
        /// here.
        /// </summary>
        private void startPhase_Hauling(AIState previousState)
        {
            CancelAndCleanUp();

            //first, we need to mark every possible hauling candidate
            bool validResourcesExist = false;
            HashSet<Carryable> validResources = new HashSet<Carryable>();

            foreach (Carryable car in WorldManager.Carryables)
            {
                if (isValidForHauling(car))
                {
                    validResourcesExist = true;
                    validResources.Add(car);
                }
            }

            //next check for stockpiles
            bool validStockpilesExist = false;
            HashSet<Stockpile> validStockpiles = new HashSet<Stockpile>();

            foreach (Stockpile pile in WorldManager.Stockpiles)
            {
                if (pile.HasFreeSquare())
                {
                    validStockpilesExist = true;
                    validStockpiles.Add(pile);
                }
            }

            //if there aren't any of some type, just quit now
            //there won't be any cleanup that needs to be done, yet
            if (!validResourcesExist || !validStockpilesExist)
                return;

            //but now there is :)
            Task resourceGoal = new Task_Carryable(validResources, this);
            Task stockpileGoal = new Task_Zone<Stockpile>(validStockpiles, this);

            StoredGoals.Enqueue(resourceGoal);
            StoredGoals.Enqueue(stockpileGoal);

            //otherwise, we can get on with our lives
            thinkingThread = new Thread(() => startLookingForResource_ThreadHelper(resourceGoal.MarkedLocations(), stockpileGoal.MarkedLocations()));
            startThinkingThread();
        }

        /// <summary>
        /// Given the valid sets of data, we can now find two paths.
        /// If successful, will load them both onto the savedPaths queue, unmark leftover marked positions,
        /// and move on to the next part of the phase.
        /// 
        /// If not, will still clean up the marked positions, and then wait to be done.
        /// </summary>
        /// <param name="resourceLocations"></param>
        /// <param name="stockpileLocations"></param>
        private void startLookingForResource_ThreadHelper(HashSet<Point> resourceLocations, HashSet<Point> stockpileLocations)
        {
            Path pathToResources = null;
            Path pathToStockpile = null;

            pathToResources = PathHunter.GetPath(SquareCoordinate, resourceLocations, DefaultSearchDepth, WorldManager, this.lastUpdateTime);

            if (pathToResources == null)
            {
                CancelAndCleanUp();
                return;
            }

            //grab the first goal, which is the "to resource" goal, and cycle the queue
            Task_Carryable resourceGoal = StoredGoals.Dequeue() as Task_Carryable;
            StoredGoals.Enqueue(resourceGoal);

            //unmark the excess and rig this goal up to go to the correct resource
            resourceGoal.FinishThinking(pathToResources);

            //now find the next path
            pathToStockpile = PathHunter.GetPath(pathToResources.End, stockpileLocations, DefaultSearchDepth, WorldManager, this.lastUpdateTime);

            if (pathToStockpile == null)
            {
                CancelAndCleanUp();
                return;
            }

            //now grab the second goal, which is the "to stockpile" goal, and cycle the queue
            Task_Zone<Stockpile> stockpileGoal = StoredGoals.Dequeue() as Task_Zone<Stockpile>;
            StoredGoals.Enqueue(stockpileGoal);

            //unmark the excess and rig the goal up!
            stockpileGoal.FinishThinking(pathToStockpile);

            //start heading off in the right direction!
            changeMentalStateTo(AIState.Hauling_FindingResource);

            thinkingThread = null;
        }

        /// <summary>
        /// Starts down along the road to finding the goal resource.
        /// Not actually much to be done; pathing was handled in the
        /// thinking phase.
        /// </summary>
        private void startSubPhase_Hauling_FindingResource(AIState previousState)
        {
            CurrentGoal = StoredGoals.Dequeue();
            setRoute(CurrentGoal.Route);
        }

        /// <summary>
        /// Start along the road to the goal stockpile.
        /// Not a lot to do here; pathing was handled in
        /// the Thinking phase.
        /// </summary>
        private void startSubPhase_Hauling_BringingResource(AIState previousState)
        {
            CurrentGoal = StoredGoals.Dequeue();
            setRoute(CurrentGoal.Route);
        }

        /// <summary>
        /// We ended path looking for a resource, so we
        /// should now be standing on one.  Pick it up
        /// and change state to BringingResource.
        /// </summary>
        private void endOfPath_Hauling_FindingResource()
        {
            if (IsCarryingItem)
                throw new InvalidOperationException("Shouldn't be looking for resource when we're holding something?");

            Point myPoint = SquareCoordinate;
            Task_Carryable resourceGoal = CurrentGoal as Task_Carryable;

            if (resourceGoal.GoalCarryable.SquareCoordinate != myPoint)
            {
                throw new NotImplementedException();
            }
            else
            {
                pickUpItem(resourceGoal.GoalCarryable);
                CurrentGoal.DeclareFinished();
                changeMentalStateTo(AIState.Hauling_BringingResource);
            }
        }

        /// <summary>
        /// Arrives at the stockpile, puts the item, and changes state
        /// to Undecided.  Throws a fit if any of this fails.
        /// </summary>
        private void endOfPath_Hauling_BringingResource()
        {
            if (!IsCarryingItem)
                throw new InvalidOperationException("YOU DARE APPROACH THIS PLACE EMPTYHANDED?!");

            Task_Zone<Stockpile> stockpileGoal = CurrentGoal as Task_Zone<Stockpile>;

            Point myPoint = SquareCoordinate;

            if (stockpileGoal.End.Item1 != myPoint)
            {
                throw new NotImplementedException();
            }
            else
            {
                putDownItem(true);
                stockpileGoal.End.Item2.OccupySquare(myPoint.X, myPoint.Y, this, heldItem);

                CurrentGoal.DeclareFinished();

                changeMentalStateTo(AIState.Undecided);
            }
        }

        private void interrupted_Hauling_FindingResource()
        {
            //nothing!
        }

        private void interrupted_Hauling_BringingResource()
        {
            putDownItem(false);
        }
        #endregion

        #region Sleeping AI
        /// <summary>
        /// This starts the (planned) sleeping process; it finds a path to a bedroom,
        /// claims it, and starts moving toward it.
        /// </summary>
        private void startPhase_Sleeping(AIState previousState)
        {
            CancelAndCleanUp();

            bool bedroomsExist = false;
            HashSet<Bedroom> validBedrooms = new HashSet<Bedroom>();

            foreach (Bedroom bedroom in WorldManager.Bedrooms)
            {
                if (bedroom.HasFreeSquare())
                {
                    bedroomsExist = true;
                    validBedrooms.Add(bedroom);
                }
            }

            //If there are no bedroompositions, quit.
            //With no success, there's no cleanup either, which is nice.
            if (!bedroomsExist)
                return;

            Task_Zone<Bedroom> bedroomGoal = new Task_Zone<Bedroom>(validBedrooms, this);
            StoredGoals.Enqueue(bedroomGoal);

            HashSet<Point> bedroomLocations = bedroomGoal.MarkedLocations();

            thinkingThread = new Thread(() => startPhase_Sleeping_ThreadHelper(bedroomLocations));
            startThinkingThread();
        }

        /// <summary>
        /// The deferred part of startPhase_Sleeping.  It finds a path.
        /// 
        /// If successful, loads it into the PathQueue and changes state to
        /// Sleeping_GoingToBedroom.  It also handles the cleanup (unmarking
        /// excess bedrooms) and the handling of currentBedrooms.
        /// 
        /// If unsuccessful, changes state back to Undecided.
        /// 
        /// Then sets thinking to false.
        /// </summary>
        /// <param name="destinations"></param>
        private void startPhase_Sleeping_ThreadHelper(HashSet<Point> destinations)
        {
            Path bedroomPath = PathHunter.GetPath(SquareCoordinate, destinations, DefaultSearchDepth, WorldManager, lastUpdateTime);

            if (bedroomPath == null)
            {
                CancelAndCleanUp();
                changeMentalStateTo(AIState.Undecided);
            }

            //Rig up the goals!
            Task_Zone<Bedroom> bedroomGoal = (StoredGoals.Dequeue() as Task_Zone<Bedroom>);
            bedroomGoal.FinishThinking(bedroomPath);
            StoredGoals.Enqueue(bedroomGoal);

            //and move to the next stage
            changeMentalStateTo(AIState.Sleeping_GoingToBedroom);

            //and we're done!  hurrah
            thinkingThread = null;
        }

        /// <summary>
        /// We've started going to the bedroom, so load up our
        /// first stored path and start ourselves along it.
        /// </summary>
        private void startSubPhase_Sleeping_GoingToBedroom(AIState previousState)
        {
            CurrentGoal = StoredGoals.Dequeue();
            setRoute(CurrentGoal.Route);
        }

        /// <summary>
        /// Nothing to do
        /// </summary>
        private void startSubPhase_Sleeping_Unconscious(AIState previousState)
        {
            //does nothing :)
        }

        /// <summary>
        /// Deal with the end of path, when we're looking for bed.
        /// If we successfully found our bedroom, success!  Occupy
        /// it and go to sleep.
        /// 
        /// If not, change to undecided, and if our bedroom was marked
        /// (and we just didn't make it there for some reason) unmark
        /// it.
        /// </summary>
        private void endOfPath_Sleeping_GoingToBedroom()
        {
            Task_Zone<Bedroom> bedroomGoal = CurrentGoal as Task_Zone<Bedroom>;

            bool isInBedroom = (SquareCoordinate == bedroomGoal.End.Item1);

            if (!isInBedroom)
            {
                throw new InvalidOperationException();
            }
            else
            {
                //if we did find a bedroom, occupy it ...
                bedroomGoal.End.Item2.OccupySquare(bedroomGoal.End.Item1.X, bedroomGoal.End.Item1.Y, this, this);
                bedroomGoal.DeclareFinished();

                //... and fall asleep
                changeMentalStateTo(AIState.Sleeping_Unconscious);
            }
        }

        /// <summary>
        /// At the end of the turn, determine whether you want to wake up.
        /// </summary>
        private void endOfPath_Sleeping_Unconscious()
        {
            if (shouldWakeUp())
            {
                Task_Zone<Bedroom> bedroomGoal = CurrentGoal as Task_Zone<Bedroom>;
                bedroomGoal.End.Item2.UnOccupySquare(bedroomGoal.End.Item1.X, bedroomGoal.End.Item1.Y, this);

                changeMentalStateTo(AIState.Undecided);
            }

            //else nothing!  just relax :)
        }

        private void interrupted_Sleeping_GoingToBedroom()
        {
            //nothing
        }

        private void interrupted_Sleeping_Unconscious()
        {
            Task_Zone<Bedroom> bedroomGoal = (Task_Zone<Bedroom>)this.CurrentGoal;

            bedroomGoal.End.Item2.UnOccupySquare(bedroomGoal.End.Item1.X, bedroomGoal.End.Item1.Y, this);
        }
        #endregion

        #region Interruptions
        /// <summary>
        /// When you just can't go anymore, sleep where you are.
        /// </summary>
        private void passOut()
        {
            getInterrupted();
            StoredGoals.Clear();

            //and sleep!
            changeMentalStateTo(AIState.PassedOut);
        }

        private void startPhase_PassedOut(AIState oldState)
        {
            //does nothing :)
        }

        private void endOfPath_PassedOut()
        {
            if (shouldWakeUp())
            {
                changeMentalStateTo(AIState.Undecided);
            }
        }

        private void interrupted_PassedOut()
        {
            //nothing to clean up
        }
        #endregion

        #endregion

        #region AI States
        public enum AIState
        {
            Undecided,

            //Hauling subdivision
            Hauling_Thinking,
            Hauling_FindingResource,
            Hauling_BringingResource,

            //Sleeping subdivision
            Sleeping_Thinking,
            Sleeping_GoingToBedroom,
            Sleeping_Unconscious, //just used for planned sleeping

            //Not quite the same as sleeping :)
            PassedOut
        }
        #endregion
    }
}
