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
        public override bool Passable { get { return true; } }
        public override bool IsBedroom { get { return true; } }
        #endregion

        public Bedroom(int xMin, int xMax, int yMin, int yMax, WorldManager manager)
            : base(xMin, xMax, yMin, yMax, manager)
        {
            addFloors();
        }

        protected override void setSquareToBuilt(int worldX, int worldY)
        {
            throw new NotImplementedException();
        }

        private void addFloors()
        {
            CowMouseTileMap map = WorldManager.MyMap;
            CowMouseMapCell cell = new CowMouseMapCell(1, true);

            for (int x = XMin; x <= XMax; x++)
            {
                for (int y = YMin; y <= YMax; y++)
                {
                    map.AddConstructedCell(cell, x, y);
                }
            }
        }
    }
}
