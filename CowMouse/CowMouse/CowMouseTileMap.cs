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
        }

        //the tiles I've made for grass are 20, 21, 22, 23
        private const int numTiles = 4;
        private const int startTile = 20;

        public override CowMouseMapCell MakeMapCell(int x, int y)
        {
            Random ran = new Random(makeSeed(x, y));
            int baseTile = ran.Next(numTiles) + startTile;
            return new CowMouseMapCell(baseTile, x, y, true);
        }

        private int makeSeed(int x, int y)
        {
            return x + y + x * y;
        }
    }
}
