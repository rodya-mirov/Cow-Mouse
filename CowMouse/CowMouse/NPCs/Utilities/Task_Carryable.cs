using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using CowMouse.InGameObjects;
using TileEngine.Utilities.Pathfinding;

namespace CowMouse.NPCs.Utilities
{
    public class Task_Carryable : Task
    {
        protected Queue<Carryable> MarkedPositions;
        public Carryable GoalCarryable { get; protected set; }
        protected NPC NPC;

        public override GoalType Type
        {
            get { return GoalType.ZONE; }
        }

        /// <summary>
        /// Constructs a Goal (for the purpose of being thought about!).  This
        /// automatically marks the associated positions, and there is no need
        /// to mark them otherwise (in fact this will result in an error!).
        /// </summary>
        /// <param name="potentials"></param>
        /// <param name="npc"></param>
        public Task_Carryable(ICollection<Carryable> potentials, NPC npc)
        {
            this.NPC = npc;

            this.State = GoalState.THINKING;
            this.MarkedPositions = new Queue<Carryable>();
            this.Route = null;
            this.GoalCarryable = null;

            foreach (Carryable car in potentials)
            {
                car.MarkForCollection(npc);
                MarkedPositions.Enqueue(car);
            }
        }

        /// <summary>
        /// Constructs and returns a collection of points which are the locations
        /// we have marked.  This is in preparation for thinking, and should not
        /// be called after thinking is completed.
        /// </summary>
        /// <returns></returns>
        public override HashSet<Point> MarkedLocations()
        {
            if (this.State != GoalState.THINKING)
                throw new InvalidOperationException("Wrong state.");

            HashSet<Point> output = new HashSet<Point>();

            foreach (Carryable car in MarkedPositions)
            {
                output.Add(car.SquareCoordinate);
            }

            return output;
        }

        /// <summary>
        /// Moves this Goal on to the next phase.  Cleans up all but the goal Zone
        /// and sets up the path.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="car"></param>
        public void FinishThinking(Path path)
        {
            this.Route = path;

            bool hasFoundGoal = false;

            foreach (Carryable marked in MarkedPositions)
            {
                if (!hasFoundGoal && marked.SquareCoordinate == path.End)
                {
                    hasFoundGoal = true;
                    this.GoalCarryable = marked;
                }
                else
                {
                    marked.UnMarkForCollection(this.NPC);
                }
            }

            if (!hasFoundGoal)
                throw new InvalidOperationException("Why didn't we find one?");

            MarkedPositions = null;

            this.State = GoalState.DOING;
        }

        /// <summary>
        /// This just indicates that the goal square is no longer marked
        /// (typically, it has been occupied).  Does nothing to the involved
        /// squares, just used for safe cleanup purposes.
        /// </summary>
        public override void DeclareFinished()
        {
            if (this.State != GoalState.DOING)
                throw new InvalidOperationException("Did the wrong things!");

            this.State = GoalState.COMPLETE;
        }

        /// <summary>
        /// "Completes" the goal, and cleans up all the remaining marks.  Also works
        /// for canceling a goal at any time.
        /// </summary>
        public override void CleanUp()
        {
            switch (this.State)
            {
                case GoalState.THINKING:
                    foreach (Carryable car in MarkedPositions)
                        car.UnMarkForCollection(this.NPC);

                    MarkedPositions = null;
                    break;

                case GoalState.DOING:
                    GoalCarryable.UnMarkForCollection(this.NPC);
                    GoalCarryable = null;
                    break;

                case GoalState.COMPLETE:
                    break;

                default:
                    throw new NotImplementedException();
            }

            this.State = GoalState.COMPLETE;
        }
    }
}
