using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CowMouse.Tasks.TaskStepExtensions;

namespace CowMouse.Tasks.FullTaskExtensions
{
    public class BuildingTask : FullTask
    {
        public BuildingTask(int priority)
            : base(priority)
        {
        }

        protected override bool VerifyWellFormed()
        {
            if (this.Tasks.Count != 1)
                return false;

            BuildStep buildStep = Tasks[0] as BuildStep;

            if (buildStep == null)
                return false;

            return true;
        }
    }
}
