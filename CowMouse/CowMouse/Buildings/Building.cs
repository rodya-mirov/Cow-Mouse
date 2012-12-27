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

            InitializeSquareStates();
        }

        /// <summary>
        /// How many frames it takes to build a single square in this building,
        /// ignoring hauling, etc. time.
        /// </summary>
        public virtual int SquareBuildTime { get { return 45; } }

        #region Cell Containment
        /// <summary>
        /// Determines whether or not this Building contains a specific cell,
        /// based on its coordinates.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public virtual bool ContainsCell(int worldX, int worldY)
        {
            return XMin <= worldX && worldX <= XMax
                && YMin <= worldY && worldY <= YMax;
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
        #endregion

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

        protected InWorldObject[,,] materials;
        protected int[,] materialIndices;

        /// <summary>
        /// Clears out the square states and all associated objects.
        /// Sets them all to the specified state.
        /// </summary>
        protected void InitializeSquareStates()
        {
            CellState startState = CellState.UNBUILT_READY_FOR_MATERIALS;
            if (NumberOfMaterialsPerSquare <= 0)
                startState = CellState.UNBUILT_READY_TO_BUILD;

            int width = XMax - XMin + 1;
            int height = YMax - YMin + 1;

            squareStates = new CellState[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    squareStates[x, y] = startState;
                }
            }

            stateCounts = new Dictionary<CellState, int>();

            foreach (CellState state in Enum.GetValues((typeof(CellState))))
            {
                stateCounts[state] = 0;
            }

            stateCounts[startState] = Area;

            markers = new FullTask[Width, Height];
            occupants = new InWorldObject[Width, Height];

            materials = new InWorldObject[Width, Height, NumberOfMaterialsPerSquare];
            materialIndices = new int[Width, Height];
        }

        #region Materials for Building
        /// <summary>
        /// The number of materials needed for each square of the building
        /// </summary>
        protected abstract int NumberOfMaterialsPerSquare { get; }

        /// <summary>
        /// Whether or not the specified resource fits the material need
        /// of a square which already has ``materialIndex"-many resources
        /// in it for building.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="materialIndex"></param>
        /// <returns></returns>
        protected abstract bool DoesResourceFitNeed(InWorldObject resource, int materialIndex);

        public bool DoesResourceFitNeed(int worldX, int worldY, InWorldObject resource)
        {
            if (!this.ContainsCell(worldX, worldY))
                throw new ArgumentOutOfRangeException("Invalid cell.");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            if (squareStates[localX, localY] != CellState.UNBUILT_READY_FOR_MATERIALS &&
                squareStates[localX, localY] != CellState.UNBUILT_MARKED_FOR_MATERIALS)
                throw new InvalidOperationException("Don't even need resources ...");

            return DoesResourceFitNeed(resource, materialIndices[localX, localY]);
        }
        #endregion

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

        /// <summary>
        /// Gets an object, and does with it as appropriate based on the state of the building
        /// and the specific square.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="worldObject"></param>
        public void ReceiveObject(int worldX, int worldY, Carryable worldObject, FullTask marker)
        {
            if (!this.ContainsCell(worldX, worldY))
                throw new ArgumentOutOfRangeException("This building does not contain the specified cell!");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            switch (squareStates[localX, localY])
            {
                case CellState.STORAGE_MARKED:
                    if (!IsSquareMarkedForStorageBy(worldX, worldY, marker))
                        throw new InvalidOperationException("Cell is not marked for storage by this marker, so cannot be used for storage by this marker!");

                    OccupySquareForStorage(worldX, worldY, marker, worldObject);
                    worldObject.GetPutInStockpile();

                    break;

                case CellState.UNBUILT_MARKED_FOR_MATERIALS:
                    if (!IsSquareMarkedForMaterialsBy(worldX, worldY, marker))
                        throw new InvalidOperationException("Wrong marker or coordinate.");

                    materials[localX, localY, materialIndices[localX, localY]] = worldObject;
                    materialIndices[localX, localY]++;

                    worldObject.GetUsedAsMaterial();

                    if (materialIndices[localX, localY] == NumberOfMaterialsPerSquare)
                        this.SetSquareState(worldX, worldY, CellState.UNBUILT_READY_TO_BUILD);
                    else
                        this.SetSquareState(worldX, worldY, CellState.UNBUILT_READY_FOR_MATERIALS);

                    break;
                default:
                    throw new InvalidOperationException("Can't receive object in the state " + squareStates[localX, localY].ToString());
            }
        }

        #region Building the Building
        /// <summary>
        /// Determines whether or not there is an unbuilt square.
        /// </summary>
        /// <returns></returns>
        public bool HasUnbuiltSquare()
        {
            return stateCounts[CellState.UNBUILT_READY_TO_BUILD] > 0;
        }

        /// <summary>
        /// Returns the world coordinates for the next unbuilt
        /// square.  Throws a fit if there are none.
        /// </summary>
        /// <returns></returns>
        public Point GetNextUnbuiltSquare()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (squareStates[x, y] == CellState.UNBUILT_READY_TO_BUILD)
                    {
                        return new Point(x + XMin, y + YMin);
                    }
                }
            }

            throw new InvalidOperationException("There are no unbuilt squares!  Check first!");
        }

        public void MarkSquareForBuilding(int worldX, int worldY, FullTask marker)
        {
            if (!this.ContainsCell(worldX, worldY))
                throw new ArgumentOutOfRangeException("Cell is not in the building.");

            CellState state = squareStates[worldX - XMin, worldY - YMin];

            if (state != CellState.UNBUILT_READY_TO_BUILD)
                throw new InvalidOperationException("This cell is not available for building.");

            this.SetSquareState(worldX, worldY, CellState.UNBUILT_MARKED_FOR_BUILDING);
            markers[worldX - XMin, worldY - YMin] = marker;
        }

        public void MarkSquareForMaterials(int worldX, int worldY, FullTask marker)
        {
            if (!this.ContainsCell(worldX, worldY))
                throw new ArgumentOutOfRangeException("Cell is not in the building.");

            CellState state = squareStates[worldX - XMin, worldY - YMin];

            if (state != CellState.UNBUILT_READY_FOR_MATERIALS)
                throw new InvalidOperationException("This cell is not available for building materials.");

            this.SetSquareState(worldX, worldY, CellState.UNBUILT_MARKED_FOR_MATERIALS);
            markers[worldX - XMin, worldY - YMin] = marker;
        }

        /// <summary>
        /// Returns true when the specified cell is in the building, marked for building,
        /// and the associated marker is the specified argument.  Returns false if any
        /// of these are not satisfied.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        public bool CellIsMarkedForBuildingBy(int worldX, int worldY, FullTask marker)
        {
            if (!this.ContainsCell(worldX, worldY))
                return false;

            if (squareStates[worldX - XMin, worldY - YMin] != CellState.UNBUILT_MARKED_FOR_BUILDING)
                return false;

            return markers[worldX - XMin, worldY - YMin] == marker;
        }

        public void UnMarkSquareForBuilding(int worldX, int worldY, FullTask marker)
        {
            if (!this.CellIsMarkedForBuildingBy(worldX, worldY, marker))
                throw new InvalidOperationException("Can't unmark it if you didn't mark it!");

            this.SetSquareState(worldX, worldY, CellState.UNBUILT_READY_TO_BUILD);
            this.markers[worldX - XMin, worldY - YMin] = null;
        }

        /// <summary>
        /// Attempts to build up the square in the building.
        /// Throws a fit if the square is not in the building, or
        /// if the square specified is not Unbuilt.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        public void BuildSquare(int worldX, int worldY, FullTask marker)
        {
            if (!this.ContainsCell(worldX, worldY))
                throw new ArgumentOutOfRangeException("The specified cell is not in the building!");

            if (!this.CellIsMarkedForBuildingBy(worldX, worldY, marker))
                throw new InvalidOperationException("Have to mark the square first!");

            setSquareToBuilt(worldX, worldY);
        }

        /// <summary>
        /// Sets a square to whatever the state should be after building.
        /// Checks, etc. have already been done, so don't worry about that.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        protected abstract void setSquareToBuilt(int worldX, int worldY);
        #endregion

        //The following region contains a lot (probably too many) very simple
        //and very similar method for fiddling with using this building as
        //a storage place.

        #region Storage and Occupation
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
        /// Determines if the specified square is marked.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public bool IsSquareMarkedForStorage(int worldX, int worldY)
        {
            return squareStates[worldX - XMin, worldY - YMin] == CellState.STORAGE_MARKED;
        }

        public bool IsSquareMarkedForMaterials(int worldX, int worldY)
        {
            return squareStates[worldX - XMin, worldY - YMin] == CellState.UNBUILT_MARKED_FOR_MATERIALS;
        }

        /// <summary>
        /// Determines whether the specified square is marked,
        /// and if so, whether it's marked by the supplied object.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        public bool IsSquareMarkedForStorageBy(int worldX, int worldY, FullTask marker)
        {
            return IsSquareMarkedForStorage(worldX, worldY) &&
                this.markers[worldX - XMin, worldY - YMin] == marker;
        }

        public bool IsSquareMarkedForMaterialsBy(int worldX, int worldY, FullTask marker)
        {
            return IsSquareMarkedForMaterials(worldX, worldY) &&
                this.markers[worldX - XMin, worldY - YMin] == marker;
        }

        /// <summary>
        /// Determines if the specified square is occupied.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public bool IsSquareOccupiedByStorage(int worldX, int worldY)
        {
            return squareStates[worldX - XMin, worldY - YMin] == CellState.STORAGE_OCCUPIED;
        }

        /// <summary>
        /// Determines whether the specified square is occupied,
        /// and if so, whether it's occupied by the supplied object.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="occupant"></param>
        /// <returns></returns>
        public bool IsSquareOccupiedForStorageBy(int worldX, int worldY, InWorldObject occupant)
        {
            return IsSquareOccupiedByStorage(worldX, worldY) &&
                this.occupants[worldX - XMin, worldY - YMin] == occupant;
        }

        /// <summary>
        /// Returns the marker of the specified square.
        /// Throws a fit if the square is not marked.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        protected FullTask MarkerAt(int worldX, int worldY)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (squareStates[x, y] != CellState.STORAGE_MARKED && squareStates[x, y] != CellState.UNBUILT_MARKED_FOR_BUILDING)
                throw new InvalidOperationException("Square is not marked!");

            return markers[x, y];
        }

        /// <summary>
        /// Returns the occupant of the specified square.
        /// Throws a fit if the square is not occupied.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public InWorldObject StorageOccupant(int worldX, int worldY)
        {
            if (!this.ContainsCell(worldX, worldY))
                throw new ArgumentOutOfRangeException("This does not contain the specified cell.");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            if (squareStates[localX, localY] != CellState.STORAGE_OCCUPIED)
                throw new InvalidOperationException("Square is not occupied!");

            return occupants[localX, localY];
        }

        /// <summary>
        /// Marks a specific square as "taken" by the person of interest.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        public void MarkSquareForStorage(int worldX, int worldY, FullTask marker)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (squareStates[x, y] != CellState.STORAGE_AVAILABLE)
                throw new InvalidOperationException("Can't mark a non-free square!");

            SetSquareState(worldX, worldY, CellState.STORAGE_MARKED);

            markers[x, y] = marker;
        }

        /// <summary>
        /// Unmarks the specified square.  Throws the usual invalidoperation fits
        /// when the arguments are wrong, in addition to a new fit if the supplied
        /// object isn't the one who marked it in the first place.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        public void UnMarkSquareForStorage(int worldX, int worldY, FullTask marker)
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

        public void UnMarkSquareForMaterials(int worldX, int worldY, FullTask marker)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (markers[x, y] != marker)
                throw new InvalidOperationException("The supplied object is not the one who marked the square!");
            if (squareStates[x, y] != CellState.UNBUILT_MARKED_FOR_MATERIALS)
                throw new InvalidOperationException("Can't unmark an unmarked square!");

            SetSquareState(worldX, worldY, CellState.UNBUILT_READY_FOR_MATERIALS);
            markers[x, y] = null;
        }

        /// <summary>
        /// Occupies the specified square.  Throws a fit when it's not marked,
        /// or if the specified marker is not the marker we recognized.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        /// <param name="occupant"></param>
        protected void OccupySquareForStorage(int worldX, int worldY, FullTask marker, InWorldObject occupant)
        {
            if (!IsSquareMarkedForStorageBy(worldX, worldY, marker))
                throw new InvalidOperationException("Gotta mark it before you can use it!");

            int x = worldX - XMin;
            int y = worldY - YMin;

            SetSquareState(worldX, worldY, CellState.STORAGE_OCCUPIED);
            occupants[x, y] = occupant;
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
        #endregion

        public IEnumerable<Point> SquaresThatNeedMaterials
        {
            get
            {
                for (int localX = 0; localX < Width; localX++)
                {
                    for (int localY = 0; localY < Height; localY++)
                    {
                        if (squareStates[localX, localY] == CellState.UNBUILT_READY_FOR_MATERIALS)
                            yield return new Point(localX + XMin, localY + YMin);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Describes the current state of a square of the building
    /// </summary>
    public enum CellState
    {
        UNBUILT_READY_FOR_MATERIALS, //unbuilt, needs materials
        UNBUILT_MARKED_FOR_MATERIALS, //unbuilt, but has been marked for material delivery

        UNBUILT_READY_TO_BUILD, //has not been built, but has all its materials
        UNBUILT_MARKED_FOR_BUILDING, //has not been built, but someone is getting stuff for it
        UNBUILT_BUILDING, //has not bee built, but someone is working on it

        STORAGE_AVAILABLE, //available for storage with no complications
        STORAGE_MARKED, //available for storage, but someone has called dibs
        STORAGE_OCCUPIED, //currently used for storage, is occupied

        INERT //doesn't really do anything :/
    }
}
