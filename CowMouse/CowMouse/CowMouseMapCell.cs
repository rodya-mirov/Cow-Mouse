using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using TileEngine.Utilities;

namespace CowMouse
{
    public class CowMouseMapCell : MapCell, Copyable<CowMouseMapCell>
    {
        public CowMouseMapCell(int baseTile, bool passable)
            : base(baseTile)
        {
            this.Passable = passable;
        }

        protected CowMouseMapCell(bool passable)
            : base()
        {
            this.Passable = passable;
        }

        public new CowMouseMapCell Copy()
        {
            MapCell baseCopy = base.Copy();

            CowMouseMapCell output = new CowMouseMapCell(this.Passable);
            output.Tiles = baseCopy.TilesCopy();

            return output;
        }

        public bool Passable { get; protected set; }
    }
}
