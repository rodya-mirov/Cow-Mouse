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
                //just relax bro
            }
            else if (ShouldFollowPath())
            {
                FollowPath();
            }
            else if (HasPartialTask)
            {
                CompletePartialTask();
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
                FullTask taskToTry = WorldManager.TakeAvailableTask();
                thinkingThread = new Thread(() => LoadTask(taskToTry));
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

        private void LoadTask(FullTask taskToTry)
        {
            Point myPoint = this.SquareCoordinate;
            HashSet<Point> goalPoints = new HashSet<Point>();
            goalPoints.Add(taskToTry.StartPoint);

            Path pathToTask = PathHunter.GetPath(myPoint, goalPoints, DEFAULT_SEARCH_DEPTH, WorldManager, lastUpdateTime);

            if (pathToTask == null)
            {
                WorldManager.ReturnTaskUnfinished(taskToTry);
            }
            else
            {
                SetCurrentTask(taskToTry, pathToTask);
            }

            thinkingThread = null;
        }
        #endregion

        #region Task Management
        private FullTask currentMainTask = null;
        private bool HasMainTask { get { return currentMainTask != null; } }

        private PartialTask currentPartialTask = null;
        private bool HasPartialTask { get { return currentPartialTask != null; } }

        private void SetCurrentTask(FullTask mainTask, Path pathToStartPoint)
        {
            if (HasMainTask)
                throw new NotImplementedException("There is no default behavior for replacing a task!");

            LoadPathIntoQueuedDestinations(pathToStartPoint);
            currentMainTask = mainTask;
        }

        private void CompletePartialTask()
        {
            switch (currentPartialTask.Type)
            {
                case TaskType.PICK_UP:
                    PickUp(currentPartialTask.ToPickUp);
                    break;

                case TaskType.PUT_DOWN:
                    PutDownCarriedItem(currentPartialTask.WhereToPlace);
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

        private void PutDownCarriedItem(OccupiableZone zone)
        {
            if (!IsCarryingItem)
                throw new InvalidOperationException("How do I put *nothing* in this stockpile?");

            zone.OccupySquare(SquareCoordinate, currentMainTask, CarriedItem);

            CarriedItem.GetPutDown();
            CarriedItem.IsInStockpile = true;

            CarriedItem = null;
        }
        #endregion
    }
}
