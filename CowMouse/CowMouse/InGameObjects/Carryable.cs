using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse.InGameObjects
{
    public abstract class Carryable : InWorldObject
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

        /// <summary>
        /// Represents whether someone has "called" this object.
        /// Used to prevent two collectors chasing the same object.
        /// </summary>
        public abstract bool IsMarkedForCollection { get; }

        /// <summary>
        /// Marks the object for collection with the specified collector.
        /// Typically this should end with IsMarkedForCollection to be
        /// true.  It's fine to assume IsMarkedForCollection is false when
        /// the method starts.
        /// </summary>
        /// <param name="collector"></param>
        public abstract void MarkForCollection(InGameObject collector);

        /// <summary>
        /// Like MarkForCollection, but backwards.
        /// </summary>
        /// <param name="collector"></param>
        public abstract void UnMarkForCollection(InGameObject collector);

        /// <summary>
        /// Returns the current object which is intending to collect
        /// this object.  Undefined behavior when IsMarkedForCollection
        /// is false.
        /// </summary>
        public abstract InGameObject IntendedCollector { get; }

        #region Tags
        /// <summary>
        /// Represents whether or not this is a resource that should be
        /// collected and moved to a stockpile.
        /// </summary>
        public abstract bool IsResource { get; }

        /// <summary>
        /// Indicates whether or not this is in a stockpile.
        /// </summary>
        public abstract bool IsInStockpile { get; }
        #endregion
    }
}
