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
            int[] baseTileCodes = new int[] { 20, 21, 22, 23 };

            baseTiles = new CowMouseMapCell[baseTileCodes.Length];

            for (int i = 0; i < baseTileCodes.Length; i++)
            {
                baseTiles[i] = new CowMouseMapCell(baseTileCodes[i], true);
            }
        }

        protected override bool UseCaching { get { return true; } }

        private static CowMouseMapCell[] baseTiles;

        public override CowMouseMapCell MakeMapCell(int x, int y)
        {
            Random ran = new Random(makeSeed(x, y));
            return baseTiles[ran.Next(baseTiles.Length)];
        }

        private int makeSeed(int x, int y)
        {
            return x + y + x * y;
        }
    }
}
