using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine.Utilities.Pathfinding;
using Microsoft.Xna.Framework;

namespace CowMouse.NPCs.Utilities
{
    public abstract class Goal
    {
        public GoalState State { get; protected set; }
        public Path Route { get; protected set; }

        public abstract GoalType Type { get; }

        public abstract void CleanUp();
        public abstract void DeclareFinished();

        public abstract HashSet<Point> MarkedLocations();
    }

    public enum GoalState
    {
        THINKING,
        DOING,
        COMPLETE
    }

    public enum GoalType
    {
        CARRYABLE,
        ZONE
    }
}
