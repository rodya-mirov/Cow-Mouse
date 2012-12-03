using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using CowMouse.Buildings;
using TileEngine.Utilities.Pathfinding;

namespace CowMouse.NPCs.Utilities
{
    public class Task_Zone<T> : Task
        where T : OccupiableZone
    {
        protected Queue<Tuple<Point, T>> MarkedPositions;
        public Tuple<Point, T> End { get; protected set; }
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
        /// <param name="start"></param>
        /// <param name="potentials"></param>
        /// <param name="npc"></param>
        public Task_Zone(ICollection<T> potentials, NPC npc)
        {
            this.NPC = npc;

            this.State = GoalState.THINKING;
            this.MarkedPositions = new Queue<Tuple<Point, T>>();
            this.Route = null;
            this.End = null;

            foreach (T zone in potentials)
            {
                Point p = zone.GetNextFreeSquare();
                zone.MarkSquare(p.X, p.Y, npc);
                MarkedPositions.Enqueue(new Tuple<Point, T>(p, zone));
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

            foreach (Tuple<Point, T> pz in MarkedPositions)
            {
                output.Add(pz.Item1);
            }

            return output;
        }

        /// <summary>
        /// Moves this Goal on to the next phase.  Cleans up all but the goal Zone
        /// and sets up the path.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="zone"></param>
        public void FinishThinking(Path path)
        {
            this.Route = path;
            Point endPoint = path.End;
            bool hasSetGoal = false;

            foreach (Tuple<Point, T> pointedZone in MarkedPositions)
            {
                if (!hasSetGoal && pointedZone.Item1 == endPoint)
                {
                    hasSetGoal = true;
                    this.End = pointedZone;
                }
                else
                {
                    pointedZone.Item2.UnMarkSquare(pointedZone.Item1.X, pointedZone.Item1.Y, this.NPC);
                }
            }

            MarkedPositions = null;

            this.Route = path;

            if (!hasSetGoal)
                throw new InvalidOperationException("Why didn't we find one?");

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
        /// Cleans up all the remaining marks.  Also works
        /// for canceling a goal at any time.
        /// 
        /// This also clears out all the data from the goal,
        /// to help you not write bad code that uses this
        /// after it's cleaned up :)
        /// </summary>
        public override void CleanUp()
        {
            switch (this.State)
            {
                case GoalState.THINKING:
                    foreach (Tuple<Point, T> pointedZone in MarkedPositions)
                        pointedZone.Item2.UnMarkSquare(pointedZone.Item1.X, pointedZone.Item1.Y, NPC);

                    MarkedPositions = null;
                    break;

                case GoalState.DOING:
                    End.Item2.UnMarkSquare(End.Item1.X, End.Item1.Y, this.NPC);
                    End = null;
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
