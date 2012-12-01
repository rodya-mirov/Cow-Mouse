using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse
{
    public class CowMouseTileMap : TileMap<CowMouseMapCell>
    {
        public override CowMouseMapCell MakeMapCell(int x, int y)
        {
            return new CowMouseMapCell(0, x, y);
        }
    }
}
