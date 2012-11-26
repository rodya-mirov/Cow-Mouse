using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

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

        private void addFloors()
        {
            TileMap map = WorldManager.MyMap;
            MapCell cell = new MapCell(1, 0, 0);

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
