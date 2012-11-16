using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework;
using CowMouse.InGameObjects;

namespace CowMouse.NPCs
{
    class LogHunter : TownsMan
    {
        public LogHunter(CowMouseGame game, int xCoordinate, int yCoordinate, bool usingTileCoordinates, TileMap map)
            : base(game, xCoordinate, yCoordinate, usingTileCoordinates, map)
        {
        }

        protected Log goalLog;

        public override void Update()
        {
            if (HasDestination)
            {
                MoveTowardDestination();
            }
            else if (QueuedDestinations.Count > 0)
            {
                SetDestination(QueuedDestinations.Dequeue());
            }
            else
            {
                if (goalLog != null)
                {
                    if (goalLog.CanBePickedUp)
                        goalLog.GetPickedUp(this);

                    goalLog = null;
                }

                goalLog = findClosestLog();

                if (goalLog != null)
                {
                    makePathToLog();
                }
            }
        }

        private Log findClosestLog()
        {
            Log candidate = null;
            int bestDistance = int.MaxValue;

            CowMouseGame myGame = (CowMouseGame)Game;

            foreach (InanimateObject obj in myGame.worldManager.Carryables)
            {
                Log log = (obj as Log);
                if (log != null)
                {
                    int newDistance = Math.Abs(xPositionWorld - log.xPositionWorld) + Math.Abs(yPositionWorld - log.yPositionWorld);
                    if (newDistance < bestDistance)
                    {
                        bestDistance = newDistance;
                        candidate = log;
                    }
                }
            }

            return candidate;
        }

        private void makePathToLog()
        {
            int destX = FindXSquare(goalLog.xPositionWorld, goalLog.yPositionWorld);
            int destY = FindYSquare(goalLog.xPositionWorld, goalLog.yPositionWorld);

            SetDestination(destX, destY);
        }
    }
}
