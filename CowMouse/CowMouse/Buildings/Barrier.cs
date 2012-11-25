using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse.Buildings
{
    /// <summary>
    /// This is a stand-in for future non-passable structures.
    /// </summary>
    public class Barrier : Building
    {
        TileMap Map;

        #region Tags
        public override bool Passable { get { return false; } }
        public override bool IsStockpile { get { return false; } }
        #endregion

        public Barrier(int xMin, int xMax, int yMin, int yMax, TileMap map)
            : base(xMin, xMax, yMin, yMax)
        {
            this.Map = map;
            addTiles();
        }

        private void addTiles()
        {
            MapCell floorCell = new MapCell(10, 0, 0);
            floorCell.AddTile(11, 0);
            floorCell.AddTile(12, 0);
            floorCell.AddTile(13, 0);
            floorCell.AddTile(14, 0);
            floorCell.AddTile(3, 1);

            for (int x = XMin; x <= XMax; x++)
            {
                for (int y = YMin; y <= YMax; y++)
                {
                    Map.AddConstructedCell(floorCell, x, y);
                }
            }
        }

        public override void Update()
        {
            //do nothing?
        }
    }
}
