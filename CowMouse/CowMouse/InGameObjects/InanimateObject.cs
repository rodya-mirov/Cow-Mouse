using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse.InGameObjects
{
    public abstract class InanimateObject : InGameObject
    {
        /// <summary>
        /// Returns true if this object can currently be picked up.
        /// </summary>
        public abstract bool CanBePickedUp { get; }

        /// <summary>
        /// Returns true if this object is currently being carried.
        /// </summary>
        public abstract bool IsBeingCarried { get; }

        /// <summary>
        /// Returns the "person" (for lack of a better word) which
        /// is current carrying this object.  Output is undefined
        /// when IsBeingCarried is set to false.
        /// </summary>
        protected abstract InGameObject CarryingPerson { get; }

        /// <summary>
        /// Get picked up.  Should assume CanBePickedUp is set to true,
        /// otherwise the behavior is not well-defined.
        /// 
        /// Also, should end with (among whatever else needs to happen)
        /// IsBeingCarried being true and CarryingPerson set to picker,
        /// unless something nonstandard is happening (traps or whatever?)
        /// </summary>
        /// <param name="picker"></param>
        public abstract void GetPickedUp(InGameObject picker);

        /// <summary>
        /// Get put down.  Should assume IsBeingCarried is set to true,
        /// otherwise the behavior is not well-defined.
        /// 
        /// Also, should end with (among whatever else needs to happen)
        /// IsBeingCarried being false, unless something nonstandard
        /// is happening (cursed items?  idk)
        /// </summary>
        public abstract void GetPutDown();
    }
}
