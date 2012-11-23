﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CowMouse.Buildings
{
    /// <summary>
    /// The logical part of a building (which is really just a designated area
    /// for some purpose; call it a zone).  Has containment properties and not
    /// much else.  Will frequently modify the underlying terrain when it's created,
    /// but this is not required.
    /// 
    /// Is not drawn or updated?
    /// </summary>
    public abstract class Building : IComparable<Building>
    {
        public int XMin { get; private set; }
        public int YMin { get; private set; }

        public int XMax { get; private set; }
        public int YMax { get; private set; }

        /// <summary>
        /// Determines the distance from this building to the
        /// specified point p.  Uses the bounding box.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual int DistanceToPoint(Point p)
        {
            int xdist;
            if (p.X < XMin)
                xdist = XMin - p.X;
            else if (p.X > XMax)
                xdist = p.X - XMax;
            else
                xdist = 0;

            int ydist;
            if (p.Y < YMin)
                ydist = YMin - p.Y;
            else if (YMax < p.Y)
                ydist = p.Y - YMax;
            else
                ydist = 0;

            return xdist + ydist;
        }

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
        public virtual bool ContainsCell(int x, int y)
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

        /// <summary>
        /// Returns an enumeration of the points inside this
        /// building.  Default behavior enumerates the bounding
        /// box; this can be extended.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Point> InternalPoints
        {
            get
            {
                for (int x = XMin; x <= XMax; x++)
                {
                    for (int y = YMin; y <= YMax; y++)
                    {
                        yield return new Point(x, y);
                    }
                }
            }
        }
    }
}
