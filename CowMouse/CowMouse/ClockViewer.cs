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

        private SpriteFont font;
        public SpriteFont Font
        {
            get { return font; }
            set
            {
                font = value;
                FixTextSize();
            }
        }

        private Vector2 TextSize { get; set; }
        private void FixTextSize()
        {
            String testString = TimeKeeper.TestString;
            TextSize = Font.MeasureString(testString);
        }

        private Vector2 drawPosition
        {
            get
            {
                float x, y;

                x = Game.Window.ClientBounds.Width - TextSize.X - 30;
                y = Game.Window.ClientBounds.Height - TextSize.Y - 20;

                return new Vector2((int)x, (int)y);
            }
        }

        private SpriteBatch batch { get; set; }

        public ClockViewer(TimeKeeper keeper, CowMouseGame game)
            : base(game)
        {
            this.Clock = keeper;
            this.Visible = true;

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
