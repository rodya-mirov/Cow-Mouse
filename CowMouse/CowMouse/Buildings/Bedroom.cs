using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using CowMouse.InGameObjects;
using CowMouse.NPCs;

namespace CowMouse.Buildings
{
    public class Bedroom : Building
    {
        #region Tags
        public override bool IsBedroom { get { return true; } }
        #endregion

        public Bedroom(int xMin, int xMax, int yMin, int yMax, WorldManager manager)
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
            get { return 0; }
        }

        protected override bool DoesResourceFitNeed(InWorldObject resource, int materialIndex)
        {
            return false;
        }
        #endregion

        private static CowMouseMapCell unbuiltCell = new CowMouseMapCell(31, true);
        private static CowMouseMapCell builtCell = new CowMouseMapCell(1, true);

        protected override void setSquareToBuilt(int worldX, int worldY)
        {
            WorldManager.MyMap.AddConstructedCell(builtCell, worldX, worldY);
            this.SetSquareState(worldX, worldY, BuildingInteractionType.USE, BuildingAvailabilityType.AVAILABLE);
        }
    }
}
