using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CowMouse.Buildings
{
    /// <summary>
    /// Fundamentally a logical object.
    /// Represents a building, in terms of bounds.  Fairly abstract.
    /// </summary>
    public abstract class Building : IComparable<Building>
    {
        public int XMin { get; private set; }
        public int YMin { get; private set; }

        public int XMax { get; private set; }
        public int YMax { get; private set; }

        #region Tags
        public abstract bool Passable { get; }
        public abstract bool IsStockpile { get; }
        #endregion

        public Building(int xMin, int xMax, int yMin, int yMax)
        {
            this.XMin = xMin;
            this.XMax = xMax;

            this.YMin = yMin;
            this.YMax = yMax;
        }

        /// <summary>
        /// Determines whether or not this Building contains a specific cell,
        /// based on its coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool ContainsCell(int x, int y)
        {
            return XMin <= x && x <= XMax
                && YMin <= y && y <= YMax;
        }

        /// <summary>
        /// Determines whether or not this Building overlaps a specific other Building
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Overlaps(Building b)
        {
            if (this.XMax < b.XMin || b.XMax < this.XMin ||
                this.YMax < b.YMin || b.YMax < this.YMin)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Determines whether this Building overlaps a specified Rectangle,
        /// as defined by its corner coordinates
        /// </summary>
        /// <param name="xmin"></param>
        /// <param name="xmax"></param>
        /// <param name="ymin"></param>
        /// <param name="ymax"></param>
        /// <returns></returns>
        public bool OverlapsRectangle(int xmin, int xmax, int ymin, int ymax)
        {
            if (this.XMax < xmin || xmax < this.XMin ||
                this.YMax < ymin || ymax < this.YMin)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Defines a completely arbitrary (but consistent)
        /// sorting of buildings, sorting by (lexicographically)
        /// XMin, XMax, YMin, YMax
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public int CompareTo(Building b)
        {
            if (this.XMin != b.XMin)
                return this.XMin - b.XMin;
            else if (this.XMax != b.XMax)
                return this.XMax - b.XMax;
            else if (this.YMin != b.YMin)
                return this.YMin - b.YMin;
            else
                return this.YMax - b.YMax;
        }

        public abstract void Update();
    }
}
