using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CowMouse.Tasks.TaskStepExtensions;

namespace CowMouse.Tasks.FullTaskExtensions
{
    public class HaulingTask : FullTask
    {
        public HaulingTask(int Priority)
            : base(Priority)
        {
        }

        /// <summary>
        /// Verifies that there are exactly two tasks
        /// (a pickup step and a putdown step), and
        /// that they line up properly (the item that
        /// is picked up is the same item to be put down).
        /// </summary>
        /// <returns></returns>
        protected override bool VerifyWellFormed()
        {
            if (this.Tasks.Count != 2)
                return false;

            PickupStep pickup = this.Tasks[0] as PickupStep;
            StockpileStep stockpile = this.Tasks[1] as StockpileStep;

            if (pickup == null || stockpile == null)
                return false;

            if (pickup.ToPickUp != stockpile.ToDropOff)
                return false;

            return true;
        }
    }
}
