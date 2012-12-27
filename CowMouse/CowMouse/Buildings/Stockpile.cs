using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework;
using CowMouse.InGameObjects;
using CowMouse.NPCs;

namespace CowMouse.Buildings
{
    /// <summary>
    /// This is pretty much a stockpile.  You can pile stock on it.
    /// </summary>
    public class Stockpile : Building
    {
        #region Tags
        public override bool Passable { get { return true; } }
        public override bool IsStockpile { get { return true; } }
        #endregion

        public Stockpile(int xmin, int xmax, int ymin, int ymax, WorldManager manager)
            : base(xmin, xmax, ymin, ymax, manager)
        {
            for (int x = xmin; x <= xmax; x++)
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    WorldManager.MyMap.AddConstructedCell(unbuiltCell, x, y);
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

        private static CowMouseMapCell builtCell = new CowMouseMapCell(30, true);
        private static CowMouseMapCell unbuiltCell = new CowMouseMapCell(31, true);

        protected override void setSquareToBuilt(int worldX, int worldY)
        {
            WorldManager.MyMap.AddConstructedCell(builtCell, worldX, worldY);
            this.SetSquareState(worldX, worldY, CellState.STORAGE_AVAILABLE);
        }
    }
}
