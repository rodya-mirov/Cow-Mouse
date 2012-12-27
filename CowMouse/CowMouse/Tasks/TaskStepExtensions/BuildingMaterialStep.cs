using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CowMouse.InGameObjects;
using TileEngine.Utilities.Pathfinding;
using CowMouse.Buildings;
using Microsoft.Xna.Framework;

namespace CowMouse.Tasks.TaskStepExtensions
{
    public class BuildingMaterialStep : TaskStep
    {
        public Carryable ToDropOff { get; protected set; }

        public BuildingMaterialStep(Path path, Carryable toDropOff, Building stockpile, FullTask parentList)
            : base(path, parentList, TaskType.PUT_DOWN)
        {
            this.ToDropOff = toDropOff;
            this.WhereToPlace = stockpile;

            this.endPoint = path.End;

            if (Path.Start != StartPoint)
                throw new ArgumentException("Path doesn't start where we picked up the item!");
        }

        public override Point StartPoint { get { return ToDropOff.SquareCoordinate; } }

        private Point endPoint;
        public override Point EndPoint { get { return endPoint; } }

        public override void CleanUp()
        {
            if (WhereToPlace.IsSquareMarkedForMaterialsBy(EndPoint.X, EndPoint.Y, ParentList))
                WhereToPlace.UnMarkSquareForMaterials(EndPoint.X, EndPoint.Y, ParentList);
        }
    }
}
