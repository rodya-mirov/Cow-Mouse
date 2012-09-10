using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using CowMouse.UserInterface;

namespace CowMouse
{
    public class WorldComponent : DrawableGameComponent
    {
        public WorldManager WorldManager { get; private set; }
        public HUD HUD { get; private set; }

        public WorldComponent(Game game, WorldManager worldManager)
            : base(game)
        {
            this.WorldManager = worldManager;
            HUD = new HUD(worldManager);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            this.WorldManager.Draw(gameTime);
            this.HUD.Draw(gameTime);
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
            this.HUD.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.WorldManager.Update(gameTime);
            this.HUD.Update(gameTime);
        }
    }
}
