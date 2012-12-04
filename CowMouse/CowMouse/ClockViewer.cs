using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CowMouse
{
    public class ClockViewer : DrawableGameComponent
    {
        private TimeKeeper Clock { get; set; }
        public SpriteFont Font { get; set; }

        private Vector2 drawPosition { get; set; }
        private SpriteBatch batch { get; set; }

        public ClockViewer(TimeKeeper keeper, CowMouseGame game)
            : base(game)
        {
            this.Clock = keeper;
            this.Visible = true;

            drawPosition = new Vector2(100, 100);
            batch = new SpriteBatch(game.GraphicsDevice);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Font != null)
            {
                batch.Begin();
                batch.DrawString(Font, Clock.ToString(), drawPosition, Color.White);
                batch.End();
            }
        }
    }
}
