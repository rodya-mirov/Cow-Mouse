using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TileEngine.Utilities.Pathfinding;
using CowMouse.InGameObjects;

namespace CowMouse.Tasks.TaskStepExtensions
{
    public class PickupStep : TaskStep
    {
        public PickupStep(Path path, FullTask parentList, Carryable item)
            : base(path, parentList, TaskType.PICK_UP)
        {
            this.ToPickUp = item;

            if (path != null)
            {
                this.startPoint = path.Start;

                if (path.End != EndPoint)
                    throw new ArgumentException("The path doesn't end at the item we're picking up!");
            }
            else
            {
                this.startPoint = item.SquareCoordinate;
            }
        }

        private Point startPoint;
        public override Point StartPoint { get { return startPoint; } }

        public override Point EndPoint { get { return ToPickUp.SquareCoordinate; } }

        public override void CleanUp()
        {
            if (ToPickUp.IsMarkedForCollection && ToPickUp.IntendedCollector == ParentList)
                ToPickUp.UnMarkForCollection(ParentList);
        }
    }
}
