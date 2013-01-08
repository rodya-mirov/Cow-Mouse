using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CowMouse.Buildings;
using Microsoft.Xna.Framework;

namespace CowMouse.Tasks.TaskStepExtensions
{
    public class SleepStep : TaskStep
    {
        public Bedroom markedBedroom { get; private set; }
        public Point markedPoint { get; private set; }

        public SleepStep(Point markedPoint, Bedroom markedBedroom, FullTask parentList)
            : base(null, parentList, TaskType.SLEEP)
        {
            this.markedBedroom = markedBedroom;
            this.markedPoint = markedPoint;
            this.WhereToPlace = markedBedroom;
        }

        public override Point StartPoint
        {
            get { return markedPoint; }
        }

        public override Point EndPoint
        {
            get { return markedPoint; }
        }

        public override void CleanUp()
        {
            if (markedBedroom.IsSquareMarkedByAndFor(markedPoint.X, markedPoint.Y, ParentList, BuildingInteractionType.USE))
                markedBedroom.UnMarkSquare(markedPoint.X, markedPoint.Y, ParentList, BuildingInteractionType.USE);
        }
    }
}
