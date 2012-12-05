using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using TileEngine.Utilies;

namespace CowMouse
{
    public class CowMouseTileMap : TileMap<CowMouseMapCell>
    {
        public static void LoadContent(CowMouseGame game)
        {
            baseTiles = new int[width, width];

            Random ran = new Random();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    baseTiles[x, y] = ran.Next(numTiles) + startTile;
                }
            }
        }

        //4 by 4
        private static int[,] baseTiles;
        private const int width = 5;

        //the tiles I've made for grass are 20, 21, 22, 23
        private const int numTiles = 4;
        private const int startTile = 20;

        public override CowMouseMapCell MakeMapCell(int x, int y)
        {
            return new CowMouseMapCell(baseTiles[Numerical.Mod(x, width), Numerical.Mod(y, width)], x, y, true);
        }
    }
}
