using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using TileEngine.Utilities;

namespace CowMouse
{
    public class CowMouseMapCell : MapCell, Translatable<CowMouseMapCell>
    {
        public CowMouseMapCell(int baseTile, int x, int y, bool passable)
            : base(baseTile, x, y)
        {
            this.Passable = passable;
        }

        protected CowMouseMapCell(int x, int y, bool passable)
            : base(x, y)
        {
            this.Passable = passable;
        }

        public new CowMouseMapCell CopyAt(int newX, int newY)
        {
            MapCell baseCopy = base.CopyAt(newX, newY);

            CowMouseMapCell output = new CowMouseMapCell(newX, newY, this.Passable);
            output.Tiles = baseCopy.TilesCopy();

            return output;
        }

        public bool Passable { get; protected set; }
    }
}
