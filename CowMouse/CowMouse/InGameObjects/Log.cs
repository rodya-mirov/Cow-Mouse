using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CowMouse.Buildings;
using CowMouse.Tasks;

namespace CowMouse.InGameObjects
{
    public class Log : BasicResource
    {

        #region Tags
        public override bool IsWood
        {
            get { return true; }
        }
        #endregion

        /// <summary>
        /// Constructs a log at the specified square coordinates
        /// </summary>
        /// <param name="game"></param>
        /// <param name="squareX"></param>
        /// <param name="squareY"></param>
        /// <param name="usingTileCoordinates">True if x,y are referring to SQUARES, or False if they are in-world "pixels"</param>
        /// <param name="map"></param>
        public Log(int squareX, int squareY, WorldManager manager)
            : base(squareX, squareY, manager)
        {
        }

        protected override int sourceIndex { get { return 0; } }
    }
}
