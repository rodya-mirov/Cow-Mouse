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
        }

        private static CowMouseMapCell floorCell = new CowMouseMapCell(10, true);

        protected override void setSquareToBuilt(int worldX, int worldY)
        {
            this.SetSquareState(worldX, worldY, CellState.STORAGE_AVAILABLE);
            WorldManager.MyMap.AddConstructedCell(floorCell, worldX, worldY);
        }
    }
}
