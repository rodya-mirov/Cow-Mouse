using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework;

namespace CowMouse.InGameObjects
{
    /// <summary>
    /// This is just an extension of InGameObject which has a large set of tags.
    /// </summary>
    public abstract class InWorldObject : InGameObject
    {
        #region Tags
        public virtual bool IsNPC { get { return false; } }
        #endregion

        #region Debugging Pixels
        /// <summary>
        /// These pixels are for debugging purposes; they allow one to see the
        /// bounding box of an object.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public IEnumerable<DebugPixel> BoundingPixels(CowMouseGame game)
        {
            Rectangle box = this.InWorldPixelBoundingBox;

            //top side is BLUE
            //bottom side is YELLOW
            for (int x = box.Left + 1; x < box.Right - 1; x++)
            {
                yield return new DebugPixel(
                    x - xPositionWorld,
                    box.Top - yPositionWorld,
                    game,
                    Color.Blue,
                    this
                    );

                yield return new DebugPixel(
                    x - xPositionWorld,
                    box.Bottom - 1 - yPositionWorld,
                    game,
                    Color.Yellow,
                    this
                    );
            }

            //left side is WHITE
            //right side is BLACK
            for (int y = box.Top + 1; y < box.Bottom - 1; y++)
            {
                yield return new DebugPixel(
                    box.Left - xPositionWorld,
                    y - yPositionWorld,
                    game,
                    Color.White,
                    this
                    );

                yield return new DebugPixel(
                    box.Right - 1 - xPositionWorld,
                    y - yPositionWorld,
                    game,
                    Color.Black,
                    this
                    );
            }
        }

        /// <summary>
        /// Floods all touched squares with the specified override.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="manager"></param>
        public void OverrideTouchedSquares(MapCell overrideCell, TileMapManager<InWorldObject> manager)
        {
            foreach (Point p in this.SquareCoordinatesTouched())
            {
                manager.MyMap.SetOverride(overrideCell, p.X, p.Y);
            }
        }
        #endregion
    }
}
