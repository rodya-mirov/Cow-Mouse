using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine.Utilities.Pathfinding;
using Microsoft.Xna.Framework;
using CowMouse.Buildings;

namespace CowMouse.Tasks.TaskStepExtensions
{
    public class BuildStep : TaskStep
    {
        public BuildStep(Path path, Building toBuild, FullTask parentList)
            : base(path, parentList, TaskType.BUILD)
        {
            this.ToBuild = toBuild;

            this.startPoint = path.Start;
            this.endPoint = path.End;

            checkIfBuildActionMakesSense();
        }

        public BuildStep(Point onlyPoint, Building toBuild, FullTask parentList)
            : base(null, parentList, TaskType.BUILD)
        {
            this.ToBuild = toBuild;

            this.startPoint = onlyPoint;
            this.endPoint = onlyPoint;

            checkIfBuildActionMakesSense();
        }

        /// <summary>
        /// Throws an ArgumentException if the endpoint is not a valid
        /// place to build the building :)
        /// </summary>
        private void checkIfBuildActionMakesSense()
        {
            if (!ToBuild.IsSquareMarkedByAndFor(EndPoint.X, EndPoint.Y, ParentList, BuildingInteractionType.BUILD))
                throw new ArgumentException("Cell isn't properly marked for building!");
        }

        public override void CleanUp()
        {
            if (ToBuild.IsSquareMarkedByAndFor(EndPoint.X, EndPoint.Y, ParentList, BuildingInteractionType.BUILD))
                ToBuild.UnMarkSquare(EndPoint.X, EndPoint.Y, ParentList, BuildingInteractionType.BUILD);
        }

        private Point startPoint;
        public override Point StartPoint { get { return startPoint; } }

        private Point endPoint;
        public override Point EndPoint { get { return endPoint; } }
    }
}
