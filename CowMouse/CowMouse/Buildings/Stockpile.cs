using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse.Buildings
{
    public class Stockpile : Building
    {
        TileMap Map;

        /// <summary>
        /// </summary>
        /// <param name="xmin"></param>
        /// <param name="xmax"></param>
        /// <param name="ymin"></param>
        /// <param name="ymax"></param>
        public Stockpile(int xmin, int xmax, int ymin, int ymax, TileMap map)
            : base(xmin, xmax, ymin, ymax)
        {
            this.Map = map;

            addFloors();
        }

        private void addFloors()
        {
            MapCell floorCell = new MapCell(10, 0, 0);

            for (int x = XMin; x <= XMax; x++)
            {
                for (int y = YMin; y <= YMax; y++)
                {
                    Map.AddConstructedCell(floorCell, x, y);
                }
            }
        }

        #region Tags
        /// <summary>
        /// Stockpiles can be freely walked across
        /// </summary>
        public override bool Passable
        {
            get { return true; }
        }

        public override bool IsStockpile
        {
            get { return true; }
        }
        #endregion

        /// <summary>
        /// Update method; does nothing.
        /// </summary>
        public override void Update()
        {
        }
    }
}
