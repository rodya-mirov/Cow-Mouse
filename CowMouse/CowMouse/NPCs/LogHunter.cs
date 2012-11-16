using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TileEngine;
using Microsoft.Xna.Framework;
using CowMouse.InGameObjects;
using CowMouse.Utilities.Pathfinding;

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

                // now find the path to the next log ...
                Path logPath = PathHunter.GetPath(
                    new Point(FindXSquare(xPositionWorld, yPositionWorld), FindYSquare(xPositionWorld, yPositionWorld)),
                    logPositions(),
                    30,
                    Game.worldManager
                    );

                if (logPath != null)
                {
                    goalLog = findAvailableLogAtPosition(logPath.End);

                    if (goalLog != null)
                    {
                        goalLog.MarkForCollection(this);

                        foreach (Point point in logPath.PointsTraveled())
                            QueuedDestinations.Enqueue(point);
                    }
                    else
                    {
                        throw new InvalidOperationException("We found a path to an available log but the log doesn't exist?");
                    }
                }
            }
        }

        private Log findAvailableLogAtPosition(Point pos)
        {
            //hunt through the list of carryables and find the available log
            //which matches the end of the path
            goalLog = null;

            foreach (Carryable car in Game.worldManager.Carryables)
            {
                Log log = (car as Log);
                if (log != null && log.CanBePickedUp && !log.IsMarkedForCollection)
                {
                    Point logPos = new Point(
                        FindXSquare(log.xPositionWorld, log.yPositionWorld),
                        FindYSquare(log.xPositionWorld, log.yPositionWorld)
                        );

                    if (logPos.Equals(pos))
                    {
                        return log;
                    }
                }
            }

            return null;
        }

        private IEnumerable<Point> logPositions()
        {
            foreach (Carryable car in Game.worldManager.Carryables)
            {
                Log log = (car as Log);
                if (log != null && log.CanBePickedUp && !log.IsMarkedForCollection)
                {
                    yield return new Point(
                        FindXSquare(log.xPositionWorld, log.yPositionWorld),
                        FindYSquare(log.xPositionWorld, log.yPositionWorld)
                        );
                }
            }
        }

        private Log findClosestLog()
        {
            Log candidate = null;
            int bestDistance = int.MaxValue;

            CowMouseGame myGame = (CowMouseGame)Game;

            foreach (Carryable obj in myGame.worldManager.Carryables)
            {
                if (obj.IsMarkedForCollection || !obj.CanBePickedUp)
                    continue;

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

            int startX = FindXSquare(this.xPositionWorld, this.yPositionWorld);
            int startY = FindYSquare(this.xPositionWorld, this.yPositionWorld);

            int pathX = startX;
            int pathY = startY;

            while (pathX < destX)
            {
                pathX++;
                QueuedDestinations.Enqueue(new Point(pathX, pathY));
            }
            while (pathX > destX)
            {
                pathX--;
                QueuedDestinations.Enqueue(new Point(pathX, pathY));
            }
            while (pathY < destY)
            {
                pathY++;
                QueuedDestinations.Enqueue(new Point(pathX, pathY));
            }
            while (pathY > destY)
            {
                pathY--;
                QueuedDestinations.Enqueue(new Point(pathX, pathY));
            }
        }
    }
}
