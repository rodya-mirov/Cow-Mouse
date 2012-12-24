using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse.InGameObjects
{
    public abstract class Carryable : InWorldObject
    {
        public Carryable(WorldManager manager)
            : base(manager)
        {
        }

        /// <summary>
        /// Returns true if this object can currently be picked up.
        /// </summary>
        public bool CanBePickedUp
        {
            get
            {
                switch (currentState)
                {
                    case CarryableState.CARRIED:
                    case CarryableState.LOCKED_AS_MATERIAL:
                        return false;

                    case CarryableState.IN_STOCKPILE:
                    case CarryableState.LOOSE:
                    case CarryableState.MARKED_FOR_COLLECTION:
                        return true;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Returns true if this object is currently being carried.
        /// </summary>
        public bool IsBeingCarried { get { return currentState == CarryableState.CARRIED; } }

        /// <summary>
        /// Returns the "person" (for lack of a better word) which
        /// is current carrying this object.  Output is undefined
        /// when IsBeingCarried is set to false.
        /// </summary>
        protected abstract InWorldObject CarryingPerson { get; }

        /// <summary>
        /// Get picked up.  Should assume CanBePickedUp is set to true,
        /// otherwise the behavior is not well-defined.
        /// 
        /// Also, should end with (among whatever else needs to happen)
        /// IsBeingCarried being true and CarryingPerson set to picker,
        /// unless something nonstandard is happening (traps or whatever?)
        /// </summary>
        /// <param name="picker"></param>
        public abstract void GetPickedUp(InWorldObject picker);

        /// <summary>
        /// Get put down in an un-special place.  Assumes the
        /// current state is Carried, and should end (barring
        /// weirdness) in current state being Loose.
        /// </summary>
        public abstract void Drop();

        /// <summary>
        /// Get put down in a stockpile.  Assumes the current
        /// state is Carried, and should end (barring weirdness)
        /// in current state being InStockpile.
        /// </summary>
        public abstract void GetPutInStockpile();

        /// <summary>
        /// Get used as material.  Assumes the current state is
        /// Carried, and should end (barring weirdness) in
        /// current state being Locked_As_Material
        /// </summary>
        public abstract void GetUsedAsMaterial();

        /// <summary>
        /// Represents whether someone has "called" this object.
        /// Used to prevent two collectors chasing the same object.
        /// </summary>
        public bool IsMarkedForCollection { get { return currentState == CarryableState.MARKED_FOR_COLLECTION; } }

        /// <summary>
        /// Marks the object for collection with the specified collector.
        /// Typically this should end with IsMarkedForCollection to be
        /// true.  It's fine to assume IsMarkedForCollection is false when
        /// the method starts.
        /// </summary>
        /// <param name="collector"></param>
        public abstract void MarkForCollection(Tasks.FullTask collectingTask);

        /// <summary>
        /// Like MarkForCollection, but backwards.
        /// </summary>
        /// <param name="collector"></param>
        public abstract void UnMarkForCollection(Tasks.FullTask collectingTask);

        /// <summary>
        /// Returns the current object which is intending to collect
        /// this object.  Undefined behavior when IsMarkedForCollection
        /// is false.
        /// </summary>
        public abstract Tasks.FullTask IntendedCollector { get; }

        /// <summary>
        /// Whether or not this resource is a good idea to mark for
        /// hauling.
        /// </summary>
        public bool ShouldBeMarkedForHauling
        {
            get
            {
                return IsResource && (currentState == CarryableState.LOOSE);
            }
        }

        #region Tags
        /// <summary>
        /// Represents whether or not this is a resource that should be
        /// collected and moved to a stockpile.
        /// </summary>
        public abstract bool IsResource { get; }

        /// <summary>
        /// Whether or not this object represents a wood resource.
        /// </summary>
        public virtual bool IsWood { get { return false; } }

        /// <summary>
        /// Indicates whether or not this is in a stockpile.
        /// </summary>
        public bool IsInStockpile { get { return currentState == CarryableState.IN_STOCKPILE; } }

        protected CarryableState currentState;
        #endregion

        public bool IsAvailableForUse
        {
            get
            {
                switch (currentState)
                {
                    case CarryableState.IN_STOCKPILE:
                    case CarryableState.LOOSE:
                        return true;

                    case CarryableState.MARKED_FOR_COLLECTION:
                    case CarryableState.LOCKED_AS_MATERIAL:
                    case CarryableState.CARRIED:
                        return false;

                    default: throw new NotImplementedException();
                }
            }
        }
    }

    public enum CarryableState
    {
        CARRIED,

        IN_STOCKPILE,
        LOCKED_AS_MATERIAL,

        LOOSE,
        MARKED_FOR_COLLECTION
    }
}
