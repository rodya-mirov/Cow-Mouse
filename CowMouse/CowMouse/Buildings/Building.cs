using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using CowMouse.Tasks;
using CowMouse.InGameObjects;
using CowMouse.NPCs;

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
        public WorldManager WorldManager { get; protected set; }

        #region Dimensions
        public int XMin { get; protected set; }
        public int YMin { get; protected set; }

        public int XMax { get; protected set; }
        public int YMax { get; protected set; }

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
        #endregion

        #region Tags
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

        #region Cell Containment
        /// <summary>
        /// Determines whether or not this Building contains a specific cell,
        /// based on its coordinates.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public virtual bool ContainsSquare(int worldX, int worldY)
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

        protected BuildingInteractionType[,] squareActionModes;
        protected BuildingAvailabilityType[,] squareAvailabilityModes;

        protected Dictionary<Tuple<BuildingInteractionType, BuildingAvailabilityType>, int> modeCounts;

        protected FullTask[,] markers;
        protected InWorldObject[,] occupants;
        protected Person[,] users;

        protected InWorldObject[, ,] materials;
        protected int[,] materialIndices;

        /// <summary>
        /// Clears out the square states and all associated objects.
        /// Sets them all to the specified state.
        /// </summary>
        protected void InitializeSquareStates()
        {
            BuildingInteractionType startMode;
            if (NumberOfMaterialsPerSquare == 0)
                startMode = BuildingInteractionType.BUILD;
            else if (NumberOfMaterialsPerSquare > 0)
                startMode = BuildingInteractionType.LOAD_BUILDING_MATERIALS;
            else
                throw new NotImplementedException();

            squareActionModes = new BuildingInteractionType[Width, Height];
            squareAvailabilityModes = new BuildingAvailabilityType[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    squareActionModes[x, y] = startMode;
                    squareAvailabilityModes[x, y] = BuildingAvailabilityType.AVAILABLE;
                }
            }

            modeCounts = new Dictionary<Tuple<BuildingInteractionType, BuildingAvailabilityType>, int>();

            foreach (BuildingAvailabilityType bat in Enum.GetValues((typeof(BuildingAvailabilityType))))
            {
                foreach (BuildingInteractionType bit in Enum.GetValues((typeof(BuildingInteractionType))))
                {
                    modeCounts[Tuple.Create(bit, bat)] = 0;
                }
            }

            modeCounts[Tuple.Create(startMode, BuildingAvailabilityType.AVAILABLE)] = Area;

            markers = new FullTask[Width, Height];
            occupants = new InWorldObject[Width, Height];
            users = new Person[Width, Height];

            materials = new InWorldObject[Width, Height, NumberOfMaterialsPerSquare];
            materialIndices = new int[Width, Height];
        }

        /// <summary>
        /// Returns true if the specified square is marked by "marker" and is intended for the interaction type "bit."
        /// Returns false otherwise.
        /// 
        /// Throws a fit if (worldX,worldY) are not in the building.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        /// <param name="bit"></param>
        /// <returns></returns>
        public bool IsSquareMarkedByAndFor(int worldX, int worldY, FullTask marker, BuildingInteractionType bit)
        {
            if (!this.ContainsSquare(worldX, worldY))
                throw new InvalidOperationException("Cell is not in the building.");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            if (squareAvailabilityModes[localX, localY] != BuildingAvailabilityType.MARKED)
                return false;

            return markers[localX, localY] == marker;
        }

        /// <summary>
        /// Marks the square for use by the specified marker.  Current interaction type must match the
        /// supplied parameter, and the square supplied must be available.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        /// <param name="bit"></param>
        public void MarkSquare(int worldX, int worldY, FullTask marker, BuildingInteractionType bit)
        {
            if (!this.ContainsSquare(worldX, worldY))
                throw new InvalidOperationException("Cell is not in building.");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            BuildingInteractionType currentBit = squareActionModes[localX, localY];
            BuildingAvailabilityType currentBat = squareAvailabilityModes[localX, localY];

            if (currentBit != bit || currentBat != BuildingAvailabilityType.AVAILABLE)
                throw new InvalidOperationException("Building is not available for use as " + bit.ToString());

            SetSquareState(worldX, worldY, bit, BuildingAvailabilityType.MARKED);
            markers[localX, localY] = marker;
        }

        /// <summary>
        /// Unmarks the square for use by the specified marker.  Current interaction type must match the
        /// supplied parameter, and the square must be marked for the specified task.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        /// <param name="bit"></param>
        public void UnMarkSquare(int worldX, int worldY, FullTask marker, BuildingInteractionType bit)
        {
            if (!this.IsSquareMarkedByAndFor(worldX, worldY, marker, bit))
                throw new InvalidOperationException("Can't unmark without marking!");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            SetSquareState(worldX, worldY, bit, BuildingAvailabilityType.AVAILABLE);
            markers[localX, localY] = null;
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
            if (!this.ContainsSquare(worldX, worldY))
                throw new ArgumentOutOfRangeException("Invalid cell.");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            if (squareActionModes[localX, localY] != BuildingInteractionType.LOAD_BUILDING_MATERIALS)
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
        /// <param name="bit"></param>
        /// <param name="bat"></param>
        protected void SetSquareState(int worldX, int worldY, BuildingInteractionType bit, BuildingAvailabilityType bat)
        {
            int localX = worldX - XMin;
            int localY = worldY - YMin;

            modeCounts[Tuple.Create(squareActionModes[localX, localY], squareAvailabilityModes[localX, localY])] -= 1;

            squareAvailabilityModes[localX, localY] = bat;
            squareActionModes[localX, localY] = bit;

            modeCounts[Tuple.Create(bit, bat)] += 1;
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
            if (!this.ContainsSquare(worldX, worldY))
                throw new ArgumentOutOfRangeException("This building does not contain the specified cell!");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            if (squareAvailabilityModes[localX, localY] != BuildingAvailabilityType.MARKED)
                throw new NotImplementedException("Can't send an object without marking the square first.");

            switch (squareActionModes[localX, localY])
            {
                case BuildingInteractionType.STORAGE:
                    if (!IsSquareMarkedByAndFor(worldX, worldY, marker, BuildingInteractionType.STORAGE))
                        throw new InvalidOperationException("Cell is not marked for storage by this marker, so cannot be used for storage by this marker!");

                    if (!IsSquareMarkedByAndFor(worldX, worldY, marker, BuildingInteractionType.STORAGE))
                        throw new InvalidOperationException("Can't occupy without marking!");

                    SetSquareState(worldX, worldY, BuildingInteractionType.STORAGE, BuildingAvailabilityType.IN_USE);
                    occupants[localX, localY] = worldObject;

                    worldObject.GetPutInStockpile();

                    break;

                case BuildingInteractionType.LOAD_BUILDING_MATERIALS:
                    if (!IsSquareMarkedByAndFor(worldX, worldY, marker, BuildingInteractionType.LOAD_BUILDING_MATERIALS))
                        throw new InvalidOperationException("Wrong marker or coordinate.");

                    materials[localX, localY, materialIndices[localX, localY]] = worldObject;
                    materialIndices[localX, localY]++;

                    worldObject.GetUsedAsMaterial();

                    if (materialIndices[localX, localY] == NumberOfMaterialsPerSquare)
                        this.SetSquareState(worldX, worldY, BuildingInteractionType.BUILD, BuildingAvailabilityType.AVAILABLE);
                    else
                        this.SetSquareState(worldX, worldY, BuildingInteractionType.LOAD_BUILDING_MATERIALS, BuildingAvailabilityType.AVAILABLE);

                    break;

                default:
                    throw new NotImplementedException("Unclear how to receive an object in state " + squareActionModes[localX, localY]);
            }
        }

        /// <summary>
        /// Sets the specified square to being used by the specified person.  Requires marking first.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="user"></param>
        /// <param name="marker"></param>
        public void UseByPerson(int worldX, int worldY, Person user, FullTask marker)
        {
            if (!IsSquareMarkedByAndFor(worldX, worldY, marker, BuildingInteractionType.USE))
                throw new InvalidOperationException("Can't use what you didn't mark!");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            SetSquareState(worldX, worldY, BuildingInteractionType.USE, BuildingAvailabilityType.IN_USE);
            users[localX, localY] = user;
        }

        /// <summary>
        /// Stops using the specified square.  Obvious preconditions.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="user"></param>
        public void StopUsingSquare(int worldX, int worldY, Person user)
        {
            if (!this.ContainsSquare(worldX, worldY))
                throw new InvalidOperationException("Square out of bounds!");

            int localX = worldX - XMin;
            int localY = worldY - YMin;

            BuildingAvailabilityType currentBat = squareAvailabilityModes[localX, localY];
            BuildingInteractionType currentBit = squareActionModes[localX, localY];

            if (currentBat != BuildingAvailabilityType.IN_USE || currentBit != BuildingInteractionType.USE || users[localX, localY] != user)
                throw new InvalidOperationException("Square was not being used by the specified user!");

            users[localX, localY] = null;
            SetSquareState(worldX, worldY, BuildingInteractionType.USE, BuildingAvailabilityType.AVAILABLE);
        }

        /// <summary>
        /// Whether or not there is a square in this building which is available for the supplied interaction type.
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public bool HasAvailableSquare(BuildingInteractionType bit)
        {
            return modeCounts[Tuple.Create(bit, BuildingAvailabilityType.AVAILABLE)] > 0;
        }

        /// <summary>
        /// Returns the world coordinates for the next unbuilt
        /// square which is available for the specified interaction.  Throws a fit
        /// if none exist.
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public Point GetNextAvailableSquare(BuildingInteractionType bit)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (squareActionModes[x, y] == bit && squareAvailabilityModes[x, y] == BuildingAvailabilityType.AVAILABLE)
                    {
                        return new Point(x + XMin, y + YMin);
                    }
                }
            }

            throw new InvalidOperationException("There are no unbuilt squares!  Check first!");
        }

        /// <summary>
        /// Enumerate all squares which are available for the specified interaction type.
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public IEnumerable<Point> AvailableSquares(BuildingInteractionType bit)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (squareActionModes[x, y] == bit && squareAvailabilityModes[x, y] == BuildingAvailabilityType.AVAILABLE)
                    {
                        yield return new Point(x + XMin, y + YMin);
                    }
                }
            }
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
            if (!this.ContainsSquare(worldX, worldY))
                throw new ArgumentOutOfRangeException("The specified cell is not in the building!");

            if (!this.IsSquareMarkedByAndFor(worldX, worldY, marker, BuildingInteractionType.BUILD))
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
    }

    public enum BuildingInteractionType
    {
        LOAD_BUILDING_MATERIALS,
        BUILD,

        STORAGE,
        USE,

        NONE
    }

    public enum BuildingAvailabilityType
    {
        AVAILABLE,
        MARKED,
        IN_USE
    }
}
