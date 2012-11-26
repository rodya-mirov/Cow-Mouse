﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse.Buildings
{
    /// <summary>
    /// This is pretty much a damn stockpile.  You can pile stock on it.
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
            addFloors();
        }

        private void addFloors()
        {
            TileMap map = WorldManager.MyMap;
            MapCell floorCell = new MapCell(10, 0, 0);

            for (int x = XMin; x <= XMax; x++)
            {
                for (int y = YMin; y <= YMax; y++)
                {
                    map.AddConstructedCell(floorCell, x, y);
                }
            }
        }
    }
}
