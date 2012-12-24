using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using CowMouse.InGameObjects;
using CowMouse.NPCs;

namespace CowMouse.Buildings
{
    public class Bedroom : Building
    {
        #region Tags
        public override bool Passable { get { return true; } }
        public override bool IsBedroom { get { return true; } }
        #endregion

        public Bedroom(int xMin, int xMax, int yMin, int yMax, WorldManager manager)
            : base(xMin, xMax, yMin, yMax, manager)
        {
            addFloors();
        }

        #region Building Materials
        protected override int NumberOfMaterialsPerSquare
        {
            get { return 0; }
        }

        protected override bool DoesResourceFitNeed(InWorldObject resource, int materialIndex)
        {
            return false;
        }
        #endregion

        protected override void setSquareToBuilt(int worldX, int worldY)
        {
            throw new NotImplementedException();
        }

        private void addFloors()
        {
            throw new NotImplementedException();
        }
    }
}
