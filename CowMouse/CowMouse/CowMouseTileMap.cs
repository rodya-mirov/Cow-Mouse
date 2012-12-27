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

            grassTiles = new CowMouseMapCell[baseTileCodes.Length];

            for (int i = 0; i < baseTileCodes.Length; i++)
            {
                grassTiles[i] = new CowMouseMapCell(baseTileCodes[i], true);
            }
        }

        protected override bool UseCaching { get { return true; } }

        private static CowMouseMapCell[] grassTiles;

        public override CowMouseMapCell MakeMapCell(int x, int y)
        {
            Random ran = new Random(makeSeed(x, y));
            return grassTiles[ran.Next(grassTiles.Length)];
        }

        private int makeSeed(int x, int y)
        {
            return x + y + x * y;
        }
    }
}
