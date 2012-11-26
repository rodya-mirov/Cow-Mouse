using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse.InGameObjects
{
    /// <summary>
    /// This is just an extension of InGameObject which has a large set of tags.
    /// </summary>
    public abstract class InWorldObject : InGameObject
    {
        #region Tags
        public virtual bool IsNPC { get { return false; } }
        #endregion
    }
}
