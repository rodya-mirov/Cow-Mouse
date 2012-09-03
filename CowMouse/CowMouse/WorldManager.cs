using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TileEngine;
using Microsoft.Xna.Framework.Input;

namespace CowMouse
{
    /// <summary>
    /// This handles the world in the main game.  It's actually that functionality,
    /// along with all the base functionality of the actual TileMapComponent, which
    /// it extends.
    /// </summary>
    public class WorldManager : TileMapComponent
    {
        private const string TileSheetPath = @"Images\Tilesets\TileSheet";

        private MapCell defaultHighlightCell { get; set; }

        public WorldManager(Game game, TileMap map)
            : base(game, map, TileSheetPath)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            defaultHighlightCell = new MapCell(2, 0, 0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            KeyboardState ks = Keyboard.GetState();
            keyboardMove(ks);

            MouseState ms = Mouse.GetState();
            updateMouseActions(ms);
        }

        private ButtonState leftMouseButtonHeld { get; set; }
        private Point MouseClickStartSquare { get; set; }
        private Point MouseClickEndSquare { get; set; }

        private void updateMouseActions(MouseState ms)
        {
            if (ms.LeftButton != leftMouseButtonHeld)
            {
                leftMouseButtonHeld = ms.LeftButton;
                if (leftMouseButtonHeld == ButtonState.Pressed)
                {
                    MouseClickStartSquare = MouseSquare;
                    updateSelectedBlock();
                }
                else
                {
                    MyMap.ClearOverrides();
                }
            }
            else if (ms.LeftButton == ButtonState.Pressed)
            {
                if (MouseClickEndSquare != MouseSquare)
                {
                    updateSelectedBlock();
                }
            }
        }

        private void updateSelectedBlock()
        {
            MouseClickEndSquare = MouseSquare;

            MyMap.ClearOverrides();
            int xmin = Math.Min(MouseClickStartSquare.X, MouseClickEndSquare.X);
            int xmax = Math.Max(MouseClickStartSquare.X, MouseClickEndSquare.X);

            int ymin = Math.Min(MouseClickStartSquare.Y, MouseClickEndSquare.Y);
            int ymax = Math.Max(MouseClickStartSquare.Y, MouseClickEndSquare.Y);

            for (int x = xmin; x <= xmax; x++)
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    MyMap.SetOverride(defaultHighlightCell, x, y);
                }
            }
        }

        private void keyboardMove(KeyboardState ks)
        {
            if (ks.IsKeyDown(Keys.Up) || ks.IsKeyDown(Keys.W))
                Camera.Move(0, -2);

            if (ks.IsKeyDown(Keys.Down) || ks.IsKeyDown(Keys.S))
                Camera.Move(0, 2);

            if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.A))
                Camera.Move(-2, 0);

            if (ks.IsKeyDown(Keys.Right) || ks.IsKeyDown(Keys.D))
                Camera.Move(2, 0);
        }
    }
}
