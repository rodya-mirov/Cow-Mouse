using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;

namespace CowMouse
{
    public class CowMouseMapCell : MapCell
    {
        public CowMouseMapCell(int baseTile, int x, int y)
            : base(baseTile, x, y)
        {
        }

        protected CowMouseMapCell(int x, int y)
            : base(x, y)
        {
        }

        public override MapCell Copy(int newX, int newY)
        {
            MapCell baseCopy = base.Copy(newX, newY);

            CowMouseMapCell output = new CowMouseMapCell(newX, newY);
            output.Tiles = baseCopy.TilesCopy();

            return output;
        }
    }
}
