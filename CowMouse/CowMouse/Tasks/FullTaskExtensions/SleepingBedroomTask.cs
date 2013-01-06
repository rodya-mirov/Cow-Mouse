using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CowMouse.Tasks.TaskStepExtensions;

namespace CowMouse.Tasks.FullTaskExtensions
{
    public class SleepingBedroomTask : FullTask
    {
        public SleepingBedroomTask(int Priority)
            : base(Priority)
        {
        }

        protected override bool VerifyWellFormed()
        {
            if (Tasks.Count != 1)
                return false;

            SleepStep step = Tasks[0] as SleepStep;

            if (step == null)
                return false;

            return true;
        }
    }
}
