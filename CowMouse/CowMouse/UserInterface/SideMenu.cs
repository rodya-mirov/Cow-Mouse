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

        private SpriteFont font;
        public SpriteFont Font
        {
            get { return font; }
            set
            {
                font = value;
                foreach (SideMenuButton b in buttons)
                    b.Font = value;
            }
        }

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

            addButton(new Action(Game.SetMouseMode_Stockpiles), "Pile");
            addButton(new Action(Game.SetMouseMode_Barriers), "Wall");

            addButton(new Action(Game.SetMouseMode_NoAction), "(Clear)");
        }

        private void addButton(Action action, String text)
        {
            int buttonWidth = menuWidth - (2 * buttonOffsetX + 1);

            SideMenuButton button = new SideMenuButton(
                buttonWidth, buttonHeight, buttonOffsetX, currentButtonTop,
                Game, action, text, Font);

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

        #region Mouse Behavior
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

            //but regardless, deselect anything not in play
            foreach (SideMenuButton button in buttons)
            {
                if (!leftMouseButtonPressed || !button.ContainsPoint(x, y))
                    button.Selected = false;
            }
        }
        #endregion
    }
}
