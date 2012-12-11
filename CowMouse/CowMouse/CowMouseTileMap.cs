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
            baseTiles = new int[] { 20, 21, 22, 23 };
        }

        protected override bool UseCaching { get { return false; } }

        private static int[] baseTiles;

        public override CowMouseMapCell MakeMapCell(int x, int y)
        {
            Random ran = new Random(makeSeed(x, y));
            int baseTile = baseTiles[ran.Next(baseTiles.Length)];
            return new CowMouseMapCell(baseTile, x, y, true);
        }

        private int makeSeed(int x, int y)
        {
            return x + y + x * y;
        }
    }
}
