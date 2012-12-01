using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CowMouse.InGameObjects;
using Microsoft.Xna.Framework;

namespace CowMouse.Buildings
{
    public abstract class OccupiableZone : Building
    {
        public OccupiableZone(int xmin, int xmax, int ymin, int ymax, WorldManager manager)
            : base(xmin, xmax, ymin, ymax, manager)
        {
            InitializeSquareStates();
        }

        protected enum SquareState { FREE, MARKED, OCCUPIED }

        protected SquareState[,] squareStates;
        protected Dictionary<SquareState, int> stateCounts;

        protected InWorldObject[,] markers;
        protected InWorldObject[,] occupants;

        /// <summary>
        /// Clears out the square states and all associated objects.
        /// </summary>
        protected void InitializeSquareStates()
        {
            int width = XMax - XMin + 1;
            int height = YMax - YMin + 1;

            squareStates = new SquareState[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    squareStates[x, y] = SquareState.FREE;
                }
            }

            stateCounts = new Dictionary<SquareState, int>();

            foreach (SquareState state in Enum.GetValues((typeof(SquareState))))
            {
                stateCounts[state] = 0;
            }

            stateCounts[SquareState.FREE] = Area;

            markers = new InWorldObject[Width, Height];
            occupants = new InWorldObject[Width, Height];
        }

        /// <summary>
        /// Determines whether or not there is a square which is (totally) free.
        /// </summary>
        /// <returns></returns>
        public bool HasFreeSquare()
        {
            return stateCounts[SquareState.FREE] > 0;
        }

        /// <summary>
        /// Sets the designated square's occupancy status to val.
        /// This method maintains all the counts appropriately.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="val"></param>
        protected void SetSquareState(int worldX, int worldY, SquareState state)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            stateCounts[squareStates[x, y]] -= 1;
            squareStates[x, y] = state;
            stateCounts[squareStates[x, y]] += 1;
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
                    if (squareStates[x, y] == SquareState.FREE)
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
        public bool IsSquareFree(int worldX, int worldY)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            return squareStates[x, y] == SquareState.FREE;
        }

        /// <summary>
        /// Determines if the specified square is marked.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public bool IsSquareMarked(int worldX, int worldY)
        {
            return squareStates[worldX - XMin, worldY - YMin] == SquareState.MARKED;
        }

        /// <summary>
        /// Determines whether the specified square is marked,
        /// and if so, whether it's marked by the supplied object.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        public bool IsSquareMarkedBy(int worldX, int worldY, InWorldObject marker)
        {
            return IsSquareMarked(worldX, worldY) &&
                this.markers[worldX - XMin, worldY - YMin] == marker;
        }

        /// <summary>
        /// Determines if the specified square is occupied.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public bool IsSquareOccupied(int worldX, int worldY)
        {
            return squareStates[worldX - XMin, worldY - YMin] == SquareState.OCCUPIED;
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
        /// Returns the marker of the specified square.
        /// Throws a fit if the square is not marked.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <returns></returns>
        public InWorldObject Marker(int worldX, int worldY)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (squareStates[x, y] != SquareState.MARKED)
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
        public InWorldObject Occupant(int worldX, int worldY)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (squareStates[x, y] != SquareState.OCCUPIED)
                throw new InvalidOperationException("Square is not occupied!");

            return occupants[x, y];
        }

        /// <summary>
        /// Marks a specific square as "taken" by the person of interest.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="marker"></param>
        public void MarkSquare(int worldX, int worldY, InWorldObject marker)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (squareStates[x, y] != SquareState.FREE)
                throw new InvalidOperationException("Can't mark a non-free square!");

            SetSquareState(worldX, worldY, SquareState.MARKED);

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
        public void UnMarkSquare(int worldX, int worldY, InWorldObject marker)
        {
            int x = worldX - XMin;
            int y = worldY - YMin;

            if (markers[x, y] != marker)
                throw new InvalidOperationException("The supplied object is not the one who marked the square!");
            if (squareStates[x, y] != SquareState.MARKED)
                throw new InvalidOperationException("Can't unmark an unmarked square!");

            SetSquareState(worldX, worldY, SquareState.FREE);
            markers[x, y] = null;
        }

        /// <summary>
        /// Occupies the specified square.  Throws a fit when it's not marked,
        /// or if the specified marker is not the marker we recognized.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="occupant"></param>
        public void OccupySquare(int worldX, int worldY, InWorldObject marker, InWorldObject occupant)
        {
            if (!IsSquareMarkedBy(worldX, worldY, marker))
                throw new InvalidOperationException("Gotta mark it before you can use it!");

            int x = worldX - XMin;
            int y = worldY - YMin;

            SetSquareState(worldX, worldY, SquareState.OCCUPIED);
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

            if (squareStates[x, y] != SquareState.OCCUPIED)
                throw new InvalidOperationException("Can't un-occupy an unoccupied square!");

            SetSquareState(worldX, worldY, SquareState.FREE);
            occupants[x, y] = null;
        }

        /// <summary>
        /// Enumerates all objects which are contained in this stockpile.
        /// </summary>
        public IEnumerable<InWorldObject> Occupants
        {
            get
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (squareStates[x, y] == SquareState.OCCUPIED)
                            yield return occupants[x, y];
                    }
                }
            }
        }
    }
}
