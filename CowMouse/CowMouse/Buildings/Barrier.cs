using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using CowMouse.InGameObjects;

namespace CowMouse.Buildings
{
    /// <summary>
    /// This is a stand-in for future non-passable structures.
    /// </summary>
    public class Barrier : Building
    {
        public Barrier(int xMin, int xMax, int yMin, int yMax, WorldManager manager)
            : base(xMin, xMax, yMin, yMax, manager)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    manager.MyMap.AddConstructedCell(unbuiltCell, x, y);
                }
            }
        }

        #region Building Materials
        protected override int NumberOfMaterialsPerSquare
        {
            get { return 2; }
        }

        protected override bool DoesResourceFitNeed(InWorldObject resource, int materialIndex)
        {
            Carryable car = resource as Carryable;

            if (car == null)
                return false;

            if (car.IsWood)
                return true;

            return false;
        }
        #endregion

        protected override void setSquareToBuilt(int worldX, int worldY)
        {
            WorldManager.MyMap.AddConstructedCell(wallCell, worldX, worldY);
            SetSquareState(worldX, worldY, BuildingInteractionType.NONE, BuildingAvailabilityType.AVAILABLE);
        }

        #region Premade cells
        private static CowMouseMapCell unbuiltCell = new CowMouseMapCell(31, true);

        private static CowMouseMapCell wallCell = makeWallCell();

        private static CowMouseMapCell makeWallCell()
        {
            CowMouseMapCell output = new CowMouseMapCell(10, false);
            output.AddTile(11, 0);
            output.AddTile(12, 0);
            output.AddTile(13, 0);
            output.AddTile(14, 0);
            output.AddTile(3, 1);

            return output;
        }
        #endregion
    }
}
