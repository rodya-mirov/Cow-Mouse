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
        #region Tags
        public override bool Passable { get { return false; } }
        #endregion

        public Barrier(int xMin, int xMax, int yMin, int yMax, WorldManager manager)
            : base(xMin, xMax, yMin, yMax, manager)
        {
            addTiles();
        }

        protected override void setSquareToBuilt(int worldX, int worldY)
        {
            throw new NotImplementedException();
        }

        private void addTiles()
        {
            CowMouseTileMap map = WorldManager.MyMap;

            CowMouseMapCell floorCell = new CowMouseMapCell(10, false);
            floorCell.AddTile(11, 0);
            floorCell.AddTile(12, 0);
            floorCell.AddTile(13, 0);
            floorCell.AddTile(14, 0);
            floorCell.AddTile(3, 1);

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
