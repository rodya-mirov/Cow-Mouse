using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CowMouse.Tasks
{
    public abstract class FullTask
    {
        /// <summary>
        /// Lower means better!
        /// </summary>
        public int Priority { get; private set; }

        public FullTask(int Priority)
        {
            this.Priority = Priority;

            this.Tasks = new List<TaskStep>();
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
        /// frozen, this throws a fit.  This also verifies
        /// that the task is well-formed.
        /// </summary>
        public void Freeze()
        {
            if (!VerifyWellFormed())
                throw new InvalidOperationException("This task is made improperly!");

            if (IsFrozen)
                throw new InvalidOperationException("Already done!");

            IsFrozen = true;
        }

        protected List<TaskStep> Tasks;
        protected int currentTaskIndex = -1;

        /// <summary>
        /// Whether or not there is another step after the current one.
        /// </summary>
        public bool HasMoreTasks { get { return currentTaskIndex < Tasks.Count - 1; } }

        /// <summary>
        /// This returns the next task, and internally, updates the pointer to current task
        /// </summary>
        /// <returns></returns>
        public TaskStep GetNextTask()
        {
            currentTaskIndex++;
            return Tasks[currentTaskIndex];
        }

        public Point StartPoint { get { return Tasks[0].StartPoint; } }

        /// <summary>
        /// This adds a new task to the end of the list.
        /// Does not work after the task has been frozen!
        /// </summary>
        /// <param name="task"></param>
        public void AddNewTask(TaskStep task)
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
            foreach (TaskStep task in this.Tasks)
                task.CleanUp();
        }

        /// <summary>
        /// This indicates that the person who was doing this task
        /// has given up on it.  Frequently this is because it is
        /// impossible.  The default behavior here is just to mark the
        /// task as "complete" (or just finished) by moving the source
        /// index, and doing cleanup on all tasks.
        /// </summary>
        public virtual void GiveUp()
        {
            CleanUpAllTasks();
            this.currentTaskIndex = this.Tasks.Count - 1;
        }

        /// <summary>
        /// Checks to make sure that the task is well-formed, in that
        /// it has all the right pieces in the right order, and so
        /// forth.  Means different things for different task types.
        /// </summary>
        /// <returns></returns>
        protected abstract bool VerifyWellFormed();
    }
}
