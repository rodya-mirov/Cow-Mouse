using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CowMouse
{
    public abstract class InGameObject
    {
        /// <summary>
        /// General update method.  Should be called once per timestep.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// General draw method.
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="drawOffset"></param>
        public abstract void Draw(SpriteBatch batch, Vector2 drawOffset);
    }
}
