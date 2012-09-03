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

        private ResourceTracker Resources { get; set; }

        private MapCell defaultHighlightCell { get; set; }

        public WorldManager(Game game)
            : base(game, TileSheetPath)
        {
            Resources = new ResourceTracker();
        }

        public override void Initialize()
        {
            base.Initialize();

            Resources.GetIncome(ResourceType.WOOD, 500);
            defaultHighlightCell = new MapCell(2, 0, 0);
        }

        protected override TileMap makeMap()
        {
            return new TileMap();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            KeyboardState ks = Keyboard.GetState();
            keyboardMove(ks);

            MouseState ms = Mouse.GetState();
            updateMouseActions(ms);
        }

        private bool Dragging { get; set; }
        private ButtonState leftMouseButtonHeld { get; set; }
        private ButtonState rightMouseButtonHeld { get; set; }
        private Point MouseClickStartSquare { get; set; }
        private Point MouseClickEndSquare { get; set; }

        private void updateMouseActions(MouseState ms)
        {
            //if we changed the button, start that process
            if (ms.LeftButton != leftMouseButtonHeld)
            {
                leftMouseButtonHeld = ms.LeftButton;

                //start a drag box
                if (leftMouseButtonHeld == ButtonState.Pressed)
                {
                    Dragging = true;
                    MouseClickStartSquare = MouseSquare;
                    updateSelectedBlock();
                }

                //end a drag box in loss of interest
                else
                {
                    Dragging = false;
                    MyMap.ClearOverrides();
                }
            }
            
            //alternately, just continue a drag, if appropriate
            else if (Dragging)
            {
                if (MouseClickEndSquare != MouseSquare)
                {
                    updateSelectedBlock();
                }

                //NEW right click means save
                if (rightMouseButtonHeld == ButtonState.Released && ms.RightButton == ButtonState.Pressed)
                    saveDraggedBlock();

                rightMouseButtonHeld = ms.RightButton;
            }
        }

        private void saveDraggedBlock()
        {
            Dragging = false;

            MyMap.ClearOverrides();

            int xmin = Math.Min(MouseClickStartSquare.X, MouseClickEndSquare.X);
            int xmax = Math.Max(MouseClickStartSquare.X, MouseClickEndSquare.X);

            int ymin = Math.Min(MouseClickStartSquare.Y, MouseClickEndSquare.Y);
            int ymax = Math.Max(MouseClickStartSquare.Y, MouseClickEndSquare.Y);

            //this is replacable code, just a stand-in!
            //if we can afford it, buy a bunch of wood flooring
            int cost = 5 * (xmax - xmin + 1) * (ymax - ymin + 1);
            if (Resources.CanAfford(ResourceType.WOOD, cost))
            {
                MapCell floorCell = new MapCell(70, 0, 0);

                for (int x = xmin; x <= xmax; x++)
                {
                    for (int y = ymin; y <= ymax; y++)
                    {
                        MyMap.AddConstructedCell(floorCell, x, y);
                    }
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
