using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading;
using CowMouse.Tasks;
using TileEngine.Utilities.Pathfinding;
using CowMouse.InGameObjects;
using CowMouse.Buildings;

namespace CowMouse.NPCs
{
    public class NPC : Person
    {
        #region Tags
        public override bool IsNPC { get { return true; } }
        #endregion

        private static Random rng = new Random();
        private const int DEFAULT_SEARCH_DEPTH = 300;

        public NPC(int xCoordinate, int yCoordinate, bool usingWorldCoordinates, WorldManager manager)
            : base(xCoordinate, yCoordinate, usingWorldCoordinates, manager)
        {
        }

        /// <summary>
        /// This is the update method which is actually called
        /// when the NPC is left to its own devices.  Comprises
        /// decision making and effective action.
        /// </summary>
        protected override void aiUpdate(GameTime gameTime)
        {
            UpdateEnergy();

            if (IsThinking)
            {
                //just hang out and think :)
                return;
            }
            VerifyOrClearPath();

            if (HasDestination)
            {
                FollowPath();
            }
            else if (IsSleeping)
            {
                Sleep();
            }
            else if (IsExhausted)
            {
                PassOutFromExhaustion();
            }
            else if (HasMorePath())
            {
                FollowPath();
            }
            else if (HasPartialTask)
            {
                if (this.SquareCoordinate == this.currentPartialTask.EndPoint)
                    CompletePartialTask();
                else
                    CancelCurrentTask();
            }
            else if (HasMainTask)
            {
                if (currentMainTask.HasMoreTasks)
                    LoadNextPartialTask();
                else
                    currentMainTask = null;
            }
            else if (IsTired && AvailableBedroomExists())
            {
                Queue<FullTask> tasks = new Queue<FullTask>();

                foreach (Bedroom bedroom in WorldManager.Bedrooms)
                {
                    FullTask singleTask = TaskBuilder.MakeSleepingBedroomTask(bedroom, 0);
                    if (singleTask != null)
                        tasks.Enqueue(singleTask);
                }

                thinkingThread = new Thread(() => LoadTask(tasks));
                StartThinkingThread();
            }
            else if (WorldManager.HasAvailableTasks())
            {
                List<FullTask> tasksToTry = WorldManager.TakeTopPriorityTasks();
                thinkingThread = new Thread(() => LoadTask(tasksToTry));
                StartThinkingThread();
            }
        }

        public override Color Tint
        {
            get
            {
                if (IsSleeping)
                    return Color.Blue;
                else
                    return Color.White;
            }
        }

        #region Energy and Sleeping
        protected int wakeUpThreshold = 3500 + rng.Next(500);
        protected int tirednessThreshold = 1500 + rng.Next(250); //constant
        protected int exhaustionThreshold = 0; //constant

        protected int tirednessPerFrame = -1; //constant
        protected int sleepingEnergyPerFrame = 2; //constant

        protected int currentEnergy = 500 + rng.Next(1000); //varies
        protected bool IsSleeping = false;

        protected void UpdateEnergy()
        {
            if (IsSleeping)
                currentEnergy += sleepingEnergyPerFrame;
            else
                currentEnergy += tirednessPerFrame;
        }

        protected bool IsTired
        {
            get { return currentEnergy <= tirednessThreshold; }
        }

        protected bool IsExhausted
        {
            get { return currentEnergy <= exhaustionThreshold; }
        }

        /// <summary>
        /// Cancels all current tasks and just passes out.
        /// </summary>
        protected void PassOutFromExhaustion()
        {
            CancelCurrentTask();
            IsSleeping = true;
        }

        protected Bedroom currentBedroom;

        /// <summary>
        /// Bedroom might be null.
        /// </summary>
        /// <param name="bedroom"></param>
        protected void GoToSleep(Bedroom bedroom)
        {
            currentBedroom = bedroom;

            if (bedroom != null)
                bedroom.UseByPerson(SquareCoordinate.X, SquareCoordinate.Y, this, currentMainTask);

            IsSleeping = true;
        }

        protected void WakeUp()
        {
            if (currentBedroom != null)
                currentBedroom.StopUsingSquare(SquareCoordinate.X, SquareCoordinate.Y, this);

            IsSleeping = false;
        }

        protected void Sleep()
        {
            if (currentEnergy >= wakeUpThreshold)
                WakeUp();
        }

        protected bool AvailableBedroomExists()
        {
            foreach (Bedroom building in WorldManager.Bedrooms)
            {
                if (building.HasAvailableSquare(BuildingInteractionType.USE))
                    return true;
            }

            return false;
        }
        #endregion

        #region Thinking Thread
        private Thread thinkingThread = null;
        private bool IsThinking { get { return thinkingThread != null; } }

        private void StartThinkingThread()
        {
            thinkingThread.Start();
        }

        private void LoadTask(IEnumerable<FullTask> tasksToTry)
        {
            Point myPoint = this.SquareCoordinate;

            HashSet<Point> goalPoints = new HashSet<Point>();

            foreach (FullTask task in tasksToTry)
                goalPoints.Add(task.StartPoint);

            Path pathToTask = PathHunter.GetPath(myPoint, goalPoints, DEFAULT_SEARCH_DEPTH, WorldManager, lastUpdateTime);

            if (pathToTask == null)
            {
                foreach(FullTask task in tasksToTry)
                    WorldManager.ReturnTaskUnfinished(task);
            }
            else
            {
                bool foundTask = false;

                foreach (FullTask task in tasksToTry)
                {
                    if (!foundTask && task.StartPoint == pathToTask.End)
                    {
                        foundTask = true;
                        SetCurrentTask(task, pathToTask);
                    }
                    else
                    {
                        WorldManager.ReturnTaskUnfinished(task);
                    }
                }
            }

            thinkingThread = null;
        }
        #endregion

        #region Task Management
        private FullTask currentMainTask = null;
        private bool HasMainTask { get { return currentMainTask != null; } }

        private TaskStep currentPartialTask = null;
        private bool HasPartialTask { get { return currentPartialTask != null; } }

        private void SetCurrentTask(FullTask mainTask, Path pathToStartPoint)
        {
            if (HasMainTask)
                throw new NotImplementedException("There is no default behavior for replacing a task!");

            LoadPathIntoQueuedDestinations(pathToStartPoint);
            currentMainTask = mainTask;
        }

        private void CancelCurrentTask()
        {
            if (IsCarryingItem)
                DropCarriedItem();

            ClearPath();
            currentPartialTask = null;

            if (currentMainTask != null)
                currentMainTask.GiveUp();

            currentMainTask = null;
        }

        private void CompletePartialTask()
        {
            switch (currentPartialTask.Type)
            {
                case TaskType.PICK_UP:
                    PickUp(currentPartialTask.ToPickUp);
                    break;

                case TaskType.PUT_DOWN:
                    PutCarriedItemInStockpile(currentPartialTask.WhereToPlace);
                    break;

                case TaskType.BUILD:
                    currentPartialTask.ToBuild.BuildSquare(SquareCoordinate.X, SquareCoordinate.Y, currentMainTask);
                    break;

                case TaskType.SLEEP:
                    Bedroom goalBedroom = currentPartialTask.WhereToPlace as Bedroom;

                    if (goalBedroom != null)
                        goalBedroom.UseByPerson(SquareCoordinate.X, SquareCoordinate.Y, this, currentMainTask);

                    GoToSleep(goalBedroom);
                    break;

                default:
                    throw new NotImplementedException();
            }

            currentPartialTask = null;
        }

        private void LoadNextPartialTask()
        {
            currentPartialTask = currentMainTask.GetNextTask();
            if (currentPartialTask.Path != null)
            {
                LoadPathIntoQueuedDestinations(currentPartialTask.Path);
            }
        }
        #endregion

        #region Carrying Items
        private Carryable CarriedItem = null;
        private bool IsCarryingItem { get { return CarriedItem != null; } }

        private void PickUp(Carryable item)
        {
            if (IsCarryingItem)
                throw new NotImplementedException("Not sure how to pick up two things bro");

            if (!(item.IsMarkedForCollection && item.IntendedCollector == currentMainTask))
                throw new InvalidOperationException("THIS IS STOLEN ITEM!");

            if (!(item.InWorldSquareBoundingBox.Intersects(this.InWorldSquareBoundingBox)))
                throw new InvalidOperationException("It's so far away!  I can't reach it!");

            CarriedItem = item;
            item.GetPickedUp(this);
        }

        private void DropCarriedItem()
        {
            if (!IsCarryingItem)
                throw new InvalidOperationException("How do I drop *nothing*?");

            CarriedItem.Drop();

            CarriedItem = null;
        }

        private void PutCarriedItemInStockpile(Building stockpile)
        {
            if (!IsCarryingItem)
                throw new InvalidOperationException("How do I put *nothing* in this stockpile?");

            stockpile.ReceiveObject(SquareCoordinate.X, SquareCoordinate.Y, CarriedItem, currentMainTask);

            CarriedItem = null;
        }
        #endregion
    }
}
