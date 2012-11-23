using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CowMouse.UserInterface
{
    public class SideMenu : DrawableGameComponent
    {
        protected new CowMouseGame Game { get; set; }

        private int menuWidth = 60;
        private int menuHeight;

        private int buttonOffsetX = 2;
        private int buttonOffsetY = 2;
        private int buttonHeight = 40;

        private int currentButtonTop;

        private SpriteBatch batch;

        private static Texture2D HUDTexture;
        private const string HUDTexturePath = @"Images\HUD\HUD";

        private List<SideMenuButton> buttons;

        public void ToggleVisible()
        {
            this.Visible = !this.Visible;
        }

        public SideMenu(CowMouseGame game)
            : base(game)
        {
            this.Game = game;
            this.Visible = false;

            batch = new SpriteBatch(game.GraphicsDevice);

            setupButtons();
        }

        private void setupButtons()
        {
            buttons = new List<SideMenuButton>();
            currentButtonTop = buttonOffsetY;

            addButton(new Action(turnButtonsRed));
            addButton(new Action(turnButtonsWhite));
            addButton(new Action(doNothing));
            addButton(new Action(doNothing));
            addButton(new Action(doNothing));
            addButton(new Action(doNothing));
        }

        private void addButton(Action action)
        {
            int buttonWidth = menuWidth - (2 * buttonOffsetX + 1);

            SideMenuButton button = new SideMenuButton(buttonWidth, buttonHeight, buttonOffsetX, currentButtonTop, Game, action);

            buttons.Add(button);

            currentButtonTop += buttonHeight + buttonOffsetY;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            if (HUDTexture == null)
                HUDTexture = Game.Content.Load<Texture2D>(HUDTexturePath);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            menuHeight = Game.graphics.PreferredBackBufferHeight;

            batch.Begin();

            //draw the backdrop
            Rectangle sourceRect = new Rectangle(0, 0, menuWidth + 1, HUDTexture.Height);
            Rectangle targetRect = new Rectangle(0, 0, menuWidth, menuHeight);
            batch.Draw(HUDTexture, targetRect, sourceRect, Color.White);

            batch.End();

            batch.Begin();

            foreach (SideMenuButton button in buttons)
            {
                button.Draw(batch);
            }

            batch.End();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Visible)
                processMouseActions();
        }

        private bool leftMouseButtonPressed, leftMouseButtonWasPressed;
        private bool rightMouseButtonPressed, rightMouseButtonWasPressed;

        private void processMouseActions()
        {
            MouseState ms = Mouse.GetState();

            //these coordinates are measured from the upper left of the window
            int x = ms.X;
            int y = ms.Y;

            //keep track of current and past button states
            leftMouseButtonWasPressed = leftMouseButtonPressed;
            rightMouseButtonWasPressed = rightMouseButtonPressed;

            leftMouseButtonPressed = (ms.LeftButton == ButtonState.Pressed);
            rightMouseButtonPressed = (ms.RightButton == ButtonState.Pressed);

            //if we JUST pressed the left mouse button
            if (leftMouseButtonPressed && !leftMouseButtonWasPressed)
            {
                //then maybe click a button?
                foreach (SideMenuButton button in buttons)
                {
                    if (button.ContainsPoint(x, y))
                        button.GetClicked();
                }
            }
        }

        #region Button Actions
        /// <summary>
        /// You can probably guess what this does
        /// </summary>
        private void doNothing()
        {
        }

        private void turnButtonsRed()
        {
            foreach (SideMenuButton button in buttons)
                button.Tint = Color.Red;
        }

        private void turnButtonsWhite()
        {
            foreach (SideMenuButton button in buttons)
                button.Tint = Color.WhiteSmoke;
        }
        #endregion
    }

    class SideMenuButton
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

        public Texture2D Blank {get;set;}
        public Color Tint { get; set; }

        public Action Action { get; set; }

        public SideMenuButton(int width, int height, int leftX, int topY, CowMouseGame game, Action action)
        {
            this.Width = width;
            this.Height = height;
            this.Left = leftX;
            this.Top = topY;
            this.Action = action;

            LoadImage(game);
        }

        private void LoadImage(CowMouseGame game)
        {
            this.Tint = Color.WhiteSmoke;

            int length = Width * Height;

            Blank = new Texture2D(game.GraphicsDevice, Width, Height);

            Color[] colors = new Color[length];
            for (int i = 0; i < length; i++)
                colors[i] = Color.White;

            Blank.SetData<Color>(colors);
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(Blank, OuterRectangle, Color.Black);
            batch.Draw(Blank, InnerRectangle, this.Tint);
        }

        public void GetClicked()
        {
            this.Action.Invoke();
        }
    }
}
