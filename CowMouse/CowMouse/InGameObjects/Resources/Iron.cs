using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CowMouse.InGameObjects.Resources
{
    public class Iron : BasicResource
    {
        #region Tags
        public override bool IsIron { get { return true; } }
        #endregion

        public Iron(int squareX, int squareY, WorldManager manager)
            : base(squareX, squareY, manager)
        {
        }

        protected override int sourceIndex { get { return 1; } }
    }
}
