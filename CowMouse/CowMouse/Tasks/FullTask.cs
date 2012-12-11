using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CowMouse.Tasks
{
    public class FullTask
    {
        /// <summary>
        /// Lower means better!
        /// </summary>
        public int Priority { get; private set; }

        public FullTask(int Priority)
        {
            this.Priority = Priority;

            this.Tasks = new List<PartialTask>();
            this.currentTaskIndex = 0;
        }

        /// <summary>
        /// Whether or not the task list is frozen.
        /// This indicates that all the intended tasks
        /// have been added, and nothing more should
        /// be added or removed.
        /// </summary>
        public bool IsFrozen { get; private set; }

        /// <summary>
        /// This sets IsFrozen to true.  If it was already
        /// frozen, this throws a fit.
        /// </summary>
        public void Freeze()
        {
            if (IsFrozen)
                throw new InvalidOperationException("Already done!");

            IsFrozen = true;
        }

        protected List<PartialTask> Tasks;
        protected int currentTaskIndex;

        public bool HasMoreTasks { get { return currentTaskIndex < Tasks.Count; } }

        public PartialTask GetNextTask()
        {
            return Tasks[currentTaskIndex++];
        }

        public Point StartPoint { get { return Tasks[0].StartPoint; } }

        /// <summary>
        /// This adds a new task to the end of the list.
        /// Does not work after the task has been frozen!
        /// </summary>
        /// <param name="task"></param>
        public void AddNewTask(PartialTask task)
        {
            if (IsFrozen)
                throw new InvalidOperationException("Can't add a new task now!");

            Tasks.Add(task);
        }

        /// <summary>
        /// Does all required cleanup for all tasks, past and present,
        /// which are not done yet.
        /// </summary>
        public void CleanUpAllTasks()
        {
            foreach (PartialTask task in this.Tasks)
                task.CleanUp();
        }
    }
}
