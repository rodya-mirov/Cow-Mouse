using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse.Buildings
{
    /// <summary>
    /// An auto-build building which is pretty, but will probably go away soon.
    /// It's quite rigid and doesn't fit well into my image for the game.
    /// </summary>
    public class House : Building
    {
        private TileMap Map { get; set; }
        private bool isHorizontal { get; set; }

        public House(int xMin, int xMax, int yMin, int yMax, TileMap map)
            : base(xMin, xMax, yMin, yMax)
        {
            this.Map = map;

            if (XMax - XMin > 1)
                isHorizontal = true;
            else
                isHorizontal = false;
        }

        private bool builtFloors = false;
        private bool builtWalls = false;
        private bool builtRoof = false;

        private const int ticksUntilWalls = 100;
        private const int ticksUntilRoof = 100;
        private int ticks = 0;

        public override void Update()
        {
            if (!builtFloors)
            {
                dealWithFloors();
            }
            else if (!builtWalls)
            {
                dealWithWalls();
            }
            else if (!builtRoof)
            {
                dealWithRoof();
            }
        }

        private void dealWithFloors()
        {
            buildFloors();
            builtFloors = true;
        }

        private void buildFloors()
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

        private void dealWithWalls()
        {
            if (ticks < ticksUntilWalls)
            {
                ticks++;
            }
            else
            {
                buildWalls();

                builtWalls = true;
                ticks = 0;
            }
        }

        private void buildWalls()
        {
            //build back wall, which is tile 11
            for (int x = XMin; x <= XMax; x++)
            {
                Map.GetMapCell(x, YMax).AddTile(11, 0);
            }

            //build left wall, which is tile 12
            for (int y = YMin; y <= YMax; y++)
            {
                Map.GetMapCell(XMin, y).AddTile(12, 0);
            }

            //build right wall, which is tile 13
            for (int y = YMin; y <= YMax; y++)
            {
                Map.GetMapCell(XMax, y).AddTile(13, 0);
            }

            //build front wall, which is tile 14
            for (int x = XMin; x <= XMax; x++)
            {
                Map.GetMapCell(x, YMin).AddTile(14, 0);
            }
        }

        private void dealWithRoof()
        {
            if (ticks < ticksUntilRoof)
            {
                ticks++;
            }
            else
            {
                buildRoof();

                builtRoof = true;
                ticks = 0;
            }
        }

        private void buildRoof()
        {
            if (isHorizontal)
            {
                //add the back-blank roof, which is 28
                //add the front-blank roof, which is 17
                for (int x = XMin; x < XMax; x++)
                {
                    Map.GetMapCell(x, YMax).AddTile(28, 1);
                    Map.GetMapCell(x, YMin).AddTile(17, 1);
                }

                //add the back-right corner, which is 27
                Map.GetMapCell(XMax, YMax).AddTile(27, 1);

                //add the front-right corner, which is 15
                Map.GetMapCell(XMax, YMin).AddTile(15, 1);
            }
            else //is vertical :P
            {
                //add the left-blank roof, which is 26
                //add the right-blank roof, which is 18
                for (int y = YMin + 1; y <= YMax; y++)
                {
                    Map.GetMapCell(XMin, y).AddTile(26, 1);
                    Map.GetMapCell(XMax, y).AddTile(18, 1);
                }

                //add the back-left corner, which is 25
                Map.GetMapCell(XMin, YMin).AddTile(25, 1);

                //add the back-right corner, which is 16
                Map.GetMapCell(XMax, YMin).AddTile(16, 1);
            }
        }

        #region Tags
        public override bool Passable
        {
            get { return true; }
        }

        public override bool IsStockpile
        {
            get { return false; }
        }
        #endregion
    }
}
