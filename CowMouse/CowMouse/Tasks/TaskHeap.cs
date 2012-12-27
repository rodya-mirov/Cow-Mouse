using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine.Utilities.DataStructures;

namespace CowMouse.Tasks
{
    /// <summary>
    /// Straightforward task heap; lower priority bubbles up to the top.
    /// </summary>
    public class TaskHeap : Heap<FullTask>
    {
        public override bool isBetter(FullTask a, FullTask b)
        {
            return a.Priority < b.Priority;
        }
    }
}
