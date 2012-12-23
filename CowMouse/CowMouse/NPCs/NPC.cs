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

        private static Random ran = new Random();
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
            if (IsThinking)
            {
                //just hang out and think :)
                return;
            }

            VerifyOrClearPath();

            if (HasMorePath())
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
                {
                    LoadNextPartialTask();
                }
                else
                {
                    currentMainTask = null;
                }
            }
            else if (WorldManager.HasAvailableTasks())
            {
                List<FullTask> tasksToTry = WorldManager.TakeTopPriorityTasks();
                thinkingThread = new Thread(() => LoadTask(tasksToTry));
                StartThinkingThread();
            }
        }

        #region Thinking Thread
        private Thread thinkingThread = null;
        private bool IsThinking { get { return thinkingThread != null; } }

        private void StartThinkingThread()
        {
            thinkingThread.Start();
        }

        private void LoadTask(List<FullTask> tasksToTry)
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
            if (!HasMainTask || !HasPartialTask)
                throw new NotImplementedException("I honestly don't know what to do here, but it shouldn't happen yet?");

            if (IsCarryingItem)
                DropCarriedItem();

            currentPartialTask = null;
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

            CarriedItem = item;
            item.GetPickedUp(this);

            //now, just check if all the marking stuff worked properly
            if (!(item.IsMarkedForCollection && item.IntendedCollector == currentMainTask))
                throw new InvalidOperationException("THIS IS STOLEN ITEM!");

            if (!(item.InWorldSquareBoundingBox.Intersects(this.InWorldSquareBoundingBox)))
                throw new InvalidOperationException("It's so far away!  I can't reach it!");
        }

        private void DropCarriedItem()
        {
            if (!IsCarryingItem)
                throw new InvalidOperationException("How do I drop *nothing*?");

            CarriedItem.GetPutDown();
            CarriedItem.IsInStockpile = false;

            CarriedItem = null;
        }

        private void PutCarriedItemInStockpile(Building stockpile)
        {
            if (!IsCarryingItem)
                throw new InvalidOperationException("How do I put *nothing* in this stockpile?");

            stockpile.OccupySquare(SquareCoordinate, currentMainTask, CarriedItem);

            CarriedItem.GetPutDown();
            CarriedItem.IsInStockpile = true;

            CarriedItem = null;
        }
        #endregion
    }
}
