using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CowMouse.UserInterface
{
    public class SideMenuButton
    {
        #region Bounding box
        public int Width { get; set; }
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }

        public int Right
        {
            get { return Left + Width; }
        }

        public int Bottom
        {
            get { return Top + Height; }
        }

        public Rectangle OuterRectangle
        {
            get { return new Rectangle(Left, Top, Width, Height); }
        }

        public Rectangle InnerRectangle
        {
            get { return new Rectangle(Left + 1, Top + 1, Width - 2, Height - 2); }
        }

        public bool ContainsPoint(int x, int y)
        {
            return InnerRectangle.Contains(x, y);
        }
        #endregion

        public Texture2D Blank { get; set; }
        public bool Selected { get; set; }

        public Action Action { get; set; }

        private String text;
        public String Text
        {
            get { return text; }
            set
            {
                text = value;
                if (value != null && Font != null)
                    fixTextPosition();
            }
        }

        private SpriteFont font;
        public SpriteFont Font
        {
            get { return font; }
            set
            {
                font = value;
                if (value != null && Text != null)
                    fixTextPosition();
            }
        }

        private Vector2 TextPosition { get; set; }

        private void fixTextPosition()
        {
            Vector2 textSize = Font.MeasureString(Text);

            float textX = Left + Width / 2 - textSize.X / 2;
            float textY = Top + Height / 2 - textSize.Y / 2;

            TextPosition = new Vector2((int)textX, (int)textY);
        }

        public SideMenuButton(int width, int height, int leftX, int topY, CowMouseGame game, Action action,
            String text, SpriteFont font)
        {
            this.Width = width;
            this.Height = height;
            this.Left = leftX;
            this.Top = topY;

            this.Action = action;
            this.Text = text;
            this.Font = font;

            LoadImage(game);
        }

        private void LoadImage(CowMouseGame game)
        {
            int length = Width * Height;

            Blank = new Texture2D(game.GraphicsDevice, Width, Height);

            Color[] colors = new Color[length];
            for (int i = 0; i < length; i++)
                colors[i] = Color.White;

            Blank.SetData<Color>(colors);
        }

        public void Draw(SpriteBatch batch)
        {
            Color tint;

            if (Selected)
                tint = Color.PaleTurquoise;
            else
                tint = Color.WhiteSmoke;

            batch.Draw(Blank, OuterRectangle, Color.Black);
            batch.Draw(Blank, InnerRectangle, tint);

            batch.DrawString(font, Text, TextPosition, Color.Black);
        }

        public void GetClicked()
        {
            this.Action.Invoke();
            this.Selected = true;
        }
    }
}
