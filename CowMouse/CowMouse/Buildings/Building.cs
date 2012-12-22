using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using CowMouse.Tasks;
using CowMouse.InGameObjects;

namespace CowMouse.Buildings
{
    /// <summary>
    /// The logical part of a building (which is really just a designated area
    /// for some purpose; call it a zone).  Has containment properties and not
    /// much else.  Will frequently modify the underlying terrain when it's created,
    /// but this is not required.
    /// 
    /// Must be updated!
    /// </summary>
    public abstract class Building
    {
        public int XMin { get; protected set; }
        public int YMin { get; protected set; }

        public int XMax { get; protected set; }
        public int YMax { get; protected set; }

        public WorldManager WorldManager { get; protected set; }

        #region Dimensions
        /// <summary>
        /// How many columns are in this building.  Note this
        /// is XMax-XMin+1, since XMax is a valid index.
        /// </summary>
        public int Width { get { return XMax - XMin + 1; } }

        /// <summary>
        /// How many rows are in this building.  Note this
        /// is YMax-YMin+1, since YMax is a valid index.
        /// </summary>
        public int Height { get { return YMax - YMin + 1; } }

        /// <summary>
        /// How many squares are in this building.
        /// </summary>
        public int Area { get { return Width * Height; } }
        #endregion

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
        public virtual bool IsStockpile { get { return false; } }
        public virtual bool IsBedroom { get { return false; } }
        #endregion

        public Building(int xMin, int xMax, int yMin, int yMax, WorldManager manager)
        {
            this.XMin = xMin;
            this.XMax = xMax;

            this.YMin = yMin;
            this.YMax = yMax;

            this.WorldManager = manager;

            InitializeSquareStates(CellState.STORAGE_AVAILABLE);
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
        /// Update method is called every turn; the base Update
        /// does nothing, but can be extended.
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            //does nothing by default
        }

        protected CellState[,] squareStates;
        protected Dictionary<CellState, int> stateCounts;

        protected FullTask[,] markers;
        protected InWorldObject[,] occupants;

        /// <summary>
        /// Clears out the square states and all associated objects.
        /// Sets them all to the specified state.
        /// </summary>
        protected void InitializeSquareStates(CellState defaultState)
        {
            int width = XMax - XMin + 1;
            int height = YMax - YMin + 1;

            squareStates = new CellState[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    squareStates[x, y] = defaultState;
                }
            }

            stateCounts = new Dictionary<CellState, int>();

            foreach (CellState state in Enum.GetValues((typeof(CellState))))
            {
                stateCounts[state] = 0;
            }

            stateCounts[CellState.STORAGE_AVAILABLE] = Area;

            markers = new FullTask[Width, Height];
            occupants = new InWorldObject[Width, Height];
        }

        /// <summary>
        /// Sets the designated square's occupancy status to state.
        /// This method maintains all the counts appropriately.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="state"></param>
        protected void SetSquareState(int worldX, int worldY, CellState state)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            stateCounts[squareStates[x, y]] -= 1;
            squareStates[x, y] = state;
            stateCounts[squareStates[x, y]] += 1;
        }

        //The following region contains a lot (probably too many) very simple
        //and very similar method for fiddling with using this building as
        //a storage place.

        #region Storage
        /// <summary>
        /// Determines whether or not there is a square which is 
        /// available for storage.
        /// </summary>
        /// <returns></returns>
        public bool HasFreeSquare()
        {
            return stateCounts[CellState.STORAGE_AVAILABLE] > 0;
        }

        /// <summary>
        /// Gets the next free (un-marked, un-occupied) square.
        /// Throws an Exception if there are none (check first!)
        /// </summary>
        /// <returns></returns>
        public Point GetNextFreeSquare()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (squareStates[x, y] == CellState.STORAGE_AVAILABLE)
                    {
                        return new Point(x + XMin, y + YMin);
                    }
                }
            }

            throw new InvalidOperationException("There are no free squares!  Check first!");
        }

        /// <summary>
        /// Determines if the specified square is free.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public bool IsSquareFreeForStorage(int worldX, int worldY)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            return squareStates[x, y] == CellState.STORAGE_AVAILABLE;
        }

        /// <summary>
        /// Determines if the specified square is free.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public bool IsSquareFreeForStorage(Point worldPoint)
        {
            return IsSquareFreeForStorage(worldPoint.X, worldPoint.Y);
        }

        /// <summary>
        /// Determines if the specified square is marked.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public bool IsSquareMarked(int worldX, int worldY)
        {
            return squareStates[worldX - XMin, worldY - YMin] == CellState.STORAGE_MARKED;
        }

        /// <summary>
        /// Determines if the specified square is marked.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public bool IsSquareMarked(Point worldPoint)
        {
            return IsSquareMarked(worldPoint.X, worldPoint.Y);
        }

        /// <summary>
        /// Determines whether the specified square is marked,
        /// and if so, whether it's marked by the supplied object.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        public bool IsSquareMarkedBy(int worldX, int worldY, FullTask marker)
        {
            return IsSquareMarked(worldX, worldY) &&
                this.markers[worldX - XMin, worldY - YMin] == marker;
        }

        /// <summary>
        /// Determines whether the specified square is marked,
        /// and if so, whether it's marked by the supplied object.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        public bool IsSquareMarkedBy(Point worldPoint, FullTask marker)
        {
            return this.IsSquareMarkedBy(worldPoint.X, worldPoint.Y, marker);
        }

        /// <summary>
        /// Determines if the specified square is occupied.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public bool IsSquareOccupied(int worldX, int worldY)
        {
            return squareStates[worldX - XMin, worldY - YMin] == CellState.STORAGE_OCCUPIED;
        }

        /// <summary>
        /// Determines if the specified square is occupied.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public bool IsSquareOccupied(Point worldPoint)
        {
            return IsSquareOccupied(worldPoint.X, worldPoint.Y);
        }

        /// <summary>
        /// Determines whether the specified square is occupied,
        /// and if so, whether it's occupied by the supplied object.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="occupant"></param>
        /// <returns></returns>
        public bool IsSquareOccupiedBy(int worldX, int worldY, InWorldObject occupant)
        {
            return IsSquareOccupied(worldX, worldY) &&
                this.occupants[worldX - XMin, worldY - YMin] == occupant;
        }

        /// <summary>
        /// Determines whether the specified square is occupied,
        /// and if so, whether it's occupied by the supplied object.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <param name="occupant"></param>
        /// <returns></returns>
        public bool IsSquareOccupiedBy(Point worldPoint, InWorldObject occupant)
        {
            return IsSquareOccupiedBy(worldPoint.X, worldPoint.Y, occupant);
        }

        /// <summary>
        /// Returns the marker of the specified square.
        /// Throws a fit if the square is not marked.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public FullTask MarkerAt(int worldX, int worldY)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (squareStates[x, y] != CellState.STORAGE_MARKED)
                throw new InvalidOperationException("Square is not marked!");

            return markers[x, y];
        }

        /// <summary>
        /// Returns the marker of the specified square.
        /// Throws a fit if the square is not marked.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public FullTask MarkerAt(Point worldPoint)
        {
            return MarkerAt(worldPoint.X, worldPoint.Y);
        }

        /// <summary>
        /// Returns the occupant of the specified square.
        /// Throws a fit if the square is not occupied.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public InWorldObject Occupant(int worldX, int worldY)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (squareStates[x, y] != CellState.STORAGE_OCCUPIED)
                throw new InvalidOperationException("Square is not occupied!");

            return occupants[x, y];
        }

        /// <summary>
        /// Returns the occupant of the specified square.
        /// Throws a fit if the square is not occupied.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public InWorldObject Occupant(Point worldPoint)
        {
            return Occupant(worldPoint.X, worldPoint.Y);
        }

        /// <summary>
        /// Marks a specific square as "taken" by the person of interest.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        public void MarkSquare(int worldX, int worldY, FullTask marker)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (squareStates[x, y] != CellState.STORAGE_AVAILABLE)
                throw new InvalidOperationException("Can't mark a non-free square!");

            SetSquareState(worldX, worldY, CellState.STORAGE_MARKED);

            markers[x, y] = marker;
        }

        /// <summary>
        /// Marks a specific square as "taken" by the person of interest.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <param name="marker"></param>
        public void MarkSquare(Point worldPoint, FullTask marker)
        {
            this.MarkSquare(worldPoint.X, worldPoint.Y, marker);
        }

        /// <summary>
        /// Unmarks the specified square.  Throws the usual invalidoperation fits
        /// when the arguments are wrong, in addition to a new fit if the supplied
        /// object isn't the one who marked it in the first place.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        public void UnMarkSquare(int worldX, int worldY, FullTask marker)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (markers[x, y] != marker)
                throw new InvalidOperationException("The supplied object is not the one who marked the square!");
            if (squareStates[x, y] != CellState.STORAGE_MARKED)
                throw new InvalidOperationException("Can't unmark an unmarked square!");

            SetSquareState(worldX, worldY, CellState.STORAGE_AVAILABLE);
            markers[x, y] = null;
        }

        /// <summary>
        /// Unmarks the specified square.  Throws the usual invalidoperation fits
        /// when the arguments are wrong, in addition to a new fit if the supplied
        /// object isn't the one who marked it in the first place.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <param name="marker"></param>
        public void UnMarkSquare(Point worldPoint, FullTask marker)
        {
            this.UnMarkSquare(worldPoint.X, worldPoint.Y, marker);
        }

        /// <summary>
        /// Occupies the specified square.  Throws a fit when it's not marked,
        /// or if the specified marker is not the marker we recognized.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        /// <param name="occupant"></param>
        public void OccupySquare(int worldX, int worldY, FullTask marker, InWorldObject occupant)
        {
            if (!IsSquareMarkedBy(worldX, worldY, marker))
                throw new InvalidOperationException("Gotta mark it before you can use it!");

            int x = worldX - XMin;
            int y = worldY - YMin;

            SetSquareState(worldX, worldY, CellState.STORAGE_OCCUPIED);
            occupants[x, y] = occupant;
        }

        /// <summary>
        /// Occupies the specified square.  Throws a fit when it's not marked,
        /// or if the specified marker is not the marker we recognized.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <param name="marker"></param>
        /// <param name="occupant"></param>
        public void OccupySquare(Point worldPoint, FullTask marker, InWorldObject occupant)
        {
            this.OccupySquare(worldPoint.X, worldPoint.Y, marker, occupant);
        }

        /// <summary>
        /// Sets the specified square as unoccupied.  Throws a fit unless the
        /// square is currently occupied and the specified argument is the one
        /// sitting there.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="occupant"></param>
        public void UnOccupySquare(int worldX, int worldY, InWorldObject occupant)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (squareStates[x, y] != CellState.STORAGE_OCCUPIED)
                throw new InvalidOperationException("Can't un-occupy an unoccupied square!");

            SetSquareState(worldX, worldY, CellState.STORAGE_AVAILABLE);
            occupants[x, y] = null;
        }

        /// <summary>
        /// Sets the specified square as unoccupied.  Throws a fit unless the
        /// square is currently occupied and the specified argument is the one
        /// sitting there.
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <param name="occupant"></param>
        public void UnOccupySquare(Point worldPoint, InWorldObject occupant)
        {
            this.UnOccupySquare(worldPoint.X, worldPoint.Y, occupant);
        }
        #endregion
    }

    /// <summary>
    /// Describes the current state of a square of the building
    /// </summary>
    public enum CellState
    {
        UNBUILT, //has not been built, can't do much until it is
        
        STORAGE_AVAILABLE, //available for storage with no complications
        STORAGE_MARKED, //available for storage, but someone has called dibs
        STORAGE_OCCUPIED //currently used for storage, is occupied
    }
}
