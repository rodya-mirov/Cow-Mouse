using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CowMouse.InGameObjects
{
    public class Torch : InWorldObject
    {
        #region Light Amount
        public float AmountOfLight { get; protected set; }
        private const float MaxLight = 10;
        private const float MinLight = 8;
        private const float LightRange = MaxLight - MinLight;

        private float lightChange;
        private const float MaxChange = 0.06f;
        private const float MinChange = 0.04f;
        private const float ChangeRange = MaxChange - MinChange;
        #endregion

        public override int xPositionWorld { get { return xPosition; } }
        public override int yPositionWorld { get { return yPosition; } }

        protected int xPosition { get; set; }
        protected int yPosition { get; set; }

        #region Texturing
        private const int xDrawOffset = 31; //the true center of this thing is about 31 pixels right of xPos
        private const int yDrawOffset = 49; //the true center of this thing is about 49 pixels down of yPos

        public override int VisualOffsetX { get { return xDrawOffset; } }
        public override int VisualOffsetY { get { return yDrawOffset; } }

        private static Texture2D TorchTexture;
        private const string TexturePath = @"Images\Objects\Torch";

        public override Texture2D Texture
        {
            get { return TorchTexture; }
        }

        private int sourceIndex;
        private static Rectangle[] sourceRectangles;

        public override Rectangle SourceRectangle
        {
            get { return sourceRectangles[sourceIndex]; }
        }

        private const int framesPerSourceRectangle = 8;
        int frameChangeCounter = 0;

        private void updateSourceIndex()
        {
            frameChangeCounter++;
            if (frameChangeCounter >= framesPerSourceRectangle)
            {
                frameChangeCounter = 0;

                sourceIndex++;
                if (sourceIndex >= sourceRectangles.Length)
                    sourceIndex = 0;
            }
        }
        #endregion

        private static Random ran = new Random();

        public Torch(int xSquare, int ySquare, WorldManager manager)
            : base(manager)
        {
            this.xPosition = base.FindXCoordinate(xSquare, ySquare);
            this.yPosition = base.FindYCoordinate(xSquare, ySquare);

            sourceIndex = ran.Next(4);

            AmountOfLight = MinLight + ((float)ran.NextDouble()) * LightRange;
            lightChange = MinChange + ((float)ran.NextDouble()) * ChangeRange;
        }

        public static void LoadContent(Game game)
        {
            if (TorchTexture == null)
                TorchTexture = game.Content.Load<Texture2D>(TexturePath);

            sourceRectangles = new Rectangle[4];

            for (int i = 0; i < 4; i++)
                sourceRectangles[i] = new Rectangle(64 * i, 0, 64, 64);
        }

        public override void Update(GameTime gameTime)
        {
            updateSourceIndex();

            AmountOfLight += lightChange;
            if (AmountOfLight >= MaxLight)
            {
                AmountOfLight = MaxLight;
                lightChange = -Math.Abs(lightChange);
            }
            else if (AmountOfLight <= MinLight)
            {
                AmountOfLight = MinLight;
                lightChange = Math.Abs(lightChange);
            }
        }
    }
}
