using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CowMouse.UserInterface;

namespace CowMouse
{
    /// <summary>
    /// This just manages, on the game end, all the different things that need to get drawn
    /// that don't exist in the game world (like UI elements).  It doesn't do much atm,
    /// and is basically a stand-in for the existing draw manager.  Will probably go away
    /// eventually and have its code sent to the Game class.
    /// </summary>
    public class CowMouseComponent : DrawableGameComponent
    {
        public WorldManager WorldManager { get; private set; }
        public new CowMouseGame Game { get; private set; }

        public CowMouseComponent(CowMouseGame game, WorldManager worldManager)
            : base(game)
        {
            this.Game = game;

            this.WorldManager = worldManager;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            this.WorldManager.Draw(gameTime);
        }

        public override void Initialize()
        {
            base.Initialize();

            this.WorldManager.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            this.WorldManager.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.WorldManager.Update(gameTime);
        }
    }
}
