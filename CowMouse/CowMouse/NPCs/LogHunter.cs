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
                    Game.worldManager.removeLog(goalLog);
                    goalLog = null;
                }

                goalLog = Game.worldManager.closestLog(xPositionWorld, yPositionWorld);

                if (goalLog != null)
                {
                    int destX = FindXSquare(goalLog.xPositionWorld, goalLog.yPositionWorld);
                    int destY = FindYSquare(goalLog.xPositionWorld, goalLog.yPositionWorld);

                    SetDestination(destX, destY);
                }
            }
        }
    }
}
