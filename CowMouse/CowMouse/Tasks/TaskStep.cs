using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine.Utilities.DataStructures;
using Microsoft.Xna.Framework;
using TileEngine.Utilities.Pathfinding;
using CowMouse.InGameObjects;
using CowMouse.NPCs;
using CowMouse.Buildings;
using CowMouse.Tasks.TaskStepExtensions;
using CowMouse.Tasks.FullTaskExtensions;

namespace CowMouse.Tasks
{
    /// <summary>
    /// This class encompasses a single stage of a task.
    /// 
    /// To complete the task, follow the path (if it exists!)
    /// then do the action type.  Any required fields for the action
    /// should be non-null; others are probably (but not guaranteed
    /// to be) non-null.
    /// 
    /// Note that TaskStep is abstract, but the extension classes
    /// are pretty simple, and anything USING a TaskStep should be
    /// able to just use TaskStep, and not worry about what kind
    /// of Step it is (aside from the TaskType property, of course).
    /// </summary>
    public abstract class TaskStep
    {
        protected TaskStep(Path path, FullTask parentList, TaskType type)
        {
            this.ParentList = parentList;
            this.Type = type;
            this.Path = path;
        }

        public FullTask ParentList { get; private set; }

        public abstract Point StartPoint { get; }
        public abstract Point EndPoint { get; }

        /// <summary>
        /// If not null, follow the path first.
        /// If null, no movement is required for
        /// this step.
        /// </summary>
        public Path Path { get; private set; }

        /// <summary>
        /// The type of task we're doing.  This
        /// should inform the action necessary.
        /// </summary>
        public TaskType Type { get; protected set; }

        public Carryable ToPickUp { get; protected set; }
        public Building WhereToPlace { get; protected set; }
        public Building ToBuild { get; protected set; }

        /// <summary>
        /// Does any cleanup left to do.  Should be able to be safely
        /// called more than once!
        /// </summary>
        public abstract void CleanUp();
    }

    /// <summary>
    /// The type of action we need to take at the end of the path!
    /// </summary>
    public enum TaskType
    {
        PICK_UP, //pick up the relevant item
        PUT_DOWN, //put down a carried item in the intended place

        BUILD, //do some work!

        SLEEP //go to sleep!
    }
}
