using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacMan {
    /// <summary>
    /// X -> R, Y -> G: for map loading.
    /// Grid location : for sprite loading (for real locations multiply result by SpriteSize.Tile).
    /// Regarding OriginPlayerPos, OriginEnemyPos and UpwardsRestricted: these represent colors for map loading, initialization and nothing else
    /// their purpose is for spawn locations and AI restrictions respectively.
    /// </summary>
    public enum Colors {
        BigPill,
        SmallPill,
        PlayerSpriteStart,
        EnemySpriteStart,
        FruitStart,
        OriginPlayerPos,
        OriginEnemyPos,
        UpwardsRestricted,
        Nothing,
        Door,
        Error
    }

    public enum GameState {
        Start,
        GameOver,
        Playing,
        NextLevel,
        AteGhost
    }

    public enum SpriteSize {
        Tile = 8,
        Player = 13,
        Enemy = 14,
        Extra = 14
    }

    public enum EnemyType {
        Red,
        Blue,
        Orange,
        Pink
    }

    public enum AIModifier {
        Chase,
        Scatter,
        Frightened,
        Jailed,
        Emerging,
        Jailing
    }

    public static class MyExtensions {
        /// <summary>
        /// Get the grid location on the spriteSheet of the specified Color.
        /// Also useful for map loading.
        /// </summary>
        /// <param name="self">The specified parameter</param>
        /// <returns>returns a grid based position, multiply by SpriteSize.Tile for real positions.</returns>
        public static Color ToColor(this Colors self) {
            switch (self) {
                case Colors.BigPill:
                    return new Color(10, 2, 0);
                case Colors.SmallPill:
                    return new Color(11, 2, 0);
                case Colors.Door:
                    return new Color(8, 2, 0);
                case Colors.PlayerSpriteStart:
                    return new Color(0, 3, 0);
                case Colors.EnemySpriteStart:
                    return new Color(0, 9, 0);
                case Colors.FruitStart:
                    return new Color(0, 18, 0);
                case Colors.OriginPlayerPos:
                    return new Color(100, 100, 0);
                case Colors.OriginEnemyPos:
                    return new Color(200, 200, 0);
                case Colors.UpwardsRestricted:
                    return new Color(123, 123, 0);
                case Colors.Nothing:
                    return new Color(255, 255, 255);
                case Colors.Error:
                    return new Color(12, 34, 56);
            }
            return new Color();
        }

        /// <summary>
        /// Convert a Color object to a Colors enum object
        /// </summary>
        /// <param name="self">the Color object</param>
        /// <returns>Returns a new Colors enum object based on the passed in Color</returns>
        public static Colors ToColors(this Color self) {
            //sorted roughly by most common to least common
            if (self == Colors.SmallPill.ToColor())
                return Colors.SmallPill;
            if (self == Colors.BigPill.ToColor())
                return Colors.BigPill;
            if (self == Colors.Nothing.ToColor())
                return Colors.Nothing;
            if (self == Colors.Door.ToColor())
                return Colors.Door;
            if (self == Colors.OriginEnemyPos.ToColor())
                return Colors.OriginEnemyPos;
            if (self == Colors.OriginPlayerPos.ToColor())
                return Colors.OriginPlayerPos;
            if (self == Colors.UpwardsRestricted.ToColor())
                return Colors.UpwardsRestricted;
            if (self == Colors.PlayerSpriteStart.ToColor())
                return Colors.PlayerSpriteStart;
            if (self == Colors.EnemySpriteStart.ToColor())
                return Colors.EnemySpriteStart;
            if (self == Colors.FruitStart.ToColor())
                return Colors.FruitStart;
            return Colors.Error;
        }

        /// <summary>
        /// Finds the best node for the specified EnemyType and adjusts the target according to the given modifier.
        /// </summary>
        /// <param name="self">The specific enemy AI pattern</param>
        /// <param name="modifier">The AI modifier which determines the target for each enemy type</param>
        /// <param name="pos">The location of the enemy on the grid!</param>
        /// <returns>returns the closest non-wall node position from the target.</returns>
        public static Point BestTurn(this EnemyType self, AIModifier LastModifier, AIModifier modifier, Point direction, Point pos, bool ignoreDoors) {
            Point target = new Point();
            List<Point> possiblePaths = new List<Point>();
            Random rand = new Random();

            if (GameData.OutOfBounds(pos))
                return new Point();

            //Get possible paths
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    //diagonal
                    if (x != 0 && y != 0)
                        continue;
                    //== pos
                    if (x == 0 && y == 0)
                        continue;
                    if (x != 0 && LastModifier == AIModifier.Jailed && modifier == AIModifier.Frightened || modifier == AIModifier.Jailed)
                        continue;
                    //out of bounds
                    if (pos.X + x < 0 || pos.X + x >= GameData.TileCount.X || pos.Y + y <= 0 || pos.Y + y >= GameData.TileCount.Y)
                        continue;
                    //can't reverse
                    if (new Point(pos.X + x, pos.Y + y) == pos - direction && (LastModifier != AIModifier.Jailed || modifier != AIModifier.Frightened))
                        continue;
                    //wall
                    if (GameData.CurrentLevel.Tiles[pos.X + x, pos.Y + y].IsWall() && (!ignoreDoors || GameData.CurrentLevel.Tiles[pos.X + x, pos.Y + y].TileColor != Colors.Door))
                        continue;
                    //node has an upwards movement restriction
                    if (x == 0 && y == -1 && modifier != AIModifier.Frightened && GameData.CurrentLevel.Tiles[pos.X, pos.Y].TileColor == Colors.UpwardsRestricted)
                        continue;
                    //add: no obstacles
                    possiblePaths.Add(new Point(x, y));
                }
            }

            //determine best turn
            switch (self) {
                case EnemyType.Red:
                    switch (modifier) {
                        case AIModifier.Chase:
                            //target is player
                            target = GameData.Player.Position.ToPoint();
                            break;
                        case AIModifier.Scatter:
                            //at this point, scatter becomes irrelevant for red
                            if (GameData.CurrentLevel.GetRemainingPillCount() < GameData.TotalPillCount * 0.1)
                                target = GameData.Player.Position.ToPoint();
                            else
                                target = new Point(GameData.TileCount.X - 1, 0) * GameData.TileSize;
                            break;
                        case AIModifier.Frightened:
                            if (LastModifier == AIModifier.Emerging) {
                                //Act like emerging
                                target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, -1)) * GameData.TileSize;
                                break;
                            }
                            if (possiblePaths.Count > 0)
                                return possiblePaths[rand.Next(possiblePaths.Count)];
                            else
                                return new Point();
                        case AIModifier.Emerging:
                            target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, -1)) * GameData.TileSize;
                            break;
                        case AIModifier.Jailing:
                            target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, 1)) * GameData.TileSize;
                            break;
                    }
                    break;
                case EnemyType.Blue:
                    switch (modifier) {
                        case AIModifier.Chase:
                            //find red enemy for calculation
                            Enemy red = new Enemy(new Point(), EnemyType.Red);
                            foreach (var e in GameData.Enemies) {
                                if (e.Type == EnemyType.Red) {
                                    red = e;
                                    break;
                                }
                            }
                            //A is 2 nodes ahead of player. B is the Delta of Red and A. target is reds position plus 2B
                            Point a = GameData.Player.Position.ToPoint() + GameData.Player.Direction * new Point(2);
                            Point b = a - red.Position.ToPoint();
                            target = red.Position.ToPoint() + b * new Point(2);
                            break;
                        case AIModifier.Scatter:
                            target = new Point(GameData.TileCount.X - 1, GameData.TileCount.Y - 1) * GameData.TileSize;
                            break;
                        case AIModifier.Frightened:
                            if (LastModifier == AIModifier.Emerging) {
                                //Act like emerging
                                target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, -1)) * GameData.TileSize;
                                break;
                            } else if (LastModifier == AIModifier.Jailed) {
                                Point p2 = pos - new Point(0, 1);
                                if (!GameData.CurrentLevel.Tiles[p2.X, p2.Y].IsWall())
                                    target = p2 * GameData.TileSize;
                                else
                                    target = (pos + new Point(0, 1)) * GameData.TileSize;
                            }
                            if (possiblePaths.Count > 0)
                                return possiblePaths[rand.Next(possiblePaths.Count)];
                            else
                                return new Point();
                        case AIModifier.Emerging:
                            target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, -1)) * GameData.TileSize;
                            break;
                        case AIModifier.Jailing:
                            target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, 1)) * GameData.TileSize;
                            break;
                        case AIModifier.Jailed:
                            Point p = pos - new Point(0, 1);
                            if (!GameData.CurrentLevel.Tiles[p.X, p.Y].IsWall())
                                target = p * GameData.TileSize;
                            else
                                target = (pos + new Point(0, 1)) * GameData.TileSize;
                            break;
                    }
                    break;
                case EnemyType.Orange:
                    switch (modifier) {
                        case AIModifier.Chase:
                            Point delta = GameData.Player.Position.ToPoint() - pos;
                            double mag = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
                            //if orange is more than 8 nodes away from the player he targets the player, otherwise he targets his scatter corner 
                            if (mag > 8)
                                target = GameData.Player.Position.ToPoint();
                            else
                                target = new Point(0, GameData.TileCount.Y - 1) * GameData.TileSize;
                            break;
                        case AIModifier.Scatter:
                            target = new Point(0, GameData.TileCount.Y - 1);
                            break;
                        case AIModifier.Frightened:
                            if (LastModifier == AIModifier.Emerging) {
                                //Act like emerging
                                target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, -1)) * GameData.TileSize;
                                break;
                            } else if (LastModifier == AIModifier.Jailed) {
                                Point p2 = pos - new Point(0, 1);
                                if (!GameData.CurrentLevel.Tiles[p2.X, p2.Y].IsWall())
                                    target = p2 * GameData.TileSize;
                                else
                                    target = (pos + new Point(0, 1)) * GameData.TileSize;
                            }
                            if (possiblePaths.Count > 0)
                                return possiblePaths[rand.Next(possiblePaths.Count)];
                            else
                                return new Point();
                        case AIModifier.Emerging:
                            target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, -1)) * GameData.TileSize;
                            break;
                        case AIModifier.Jailing:
                            target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, 1)) * GameData.TileSize;
                            break;
                        case AIModifier.Jailed:
                            Point p = pos - new Point(0, 1);
                            if (!GameData.CurrentLevel.Tiles[p.X, p.Y].IsWall())
                                target = p * GameData.TileSize;
                            else
                                target = (pos + new Point(0, 1)) * GameData.TileSize;
                            break;
                    }
                    break;
                case EnemyType.Pink:
                    switch (modifier) {
                        case AIModifier.Chase:
                            //target is 4 nodes ahead of player
                            target = GameData.Player.Position.ToPoint() + GameData.Player.Direction * new Point(4);
                            break;
                        case AIModifier.Scatter:
                            if (LastModifier == AIModifier.Emerging) {
                                //Act like emerging
                                target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, -1)) * GameData.TileSize;
                                break;
                            }
                            target = new Point(0, 0);
                            break;
                        case AIModifier.Frightened:
                            if (possiblePaths.Count > 0)
                                return possiblePaths[rand.Next(possiblePaths.Count)];
                            else
                                return new Point();
                        case AIModifier.Emerging:
                            target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, -1)) * GameData.TileSize;
                            break;
                        case AIModifier.Jailing:
                            target = (GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, 1)) * GameData.TileSize;
                            break;
                    }
                    break;
            }

            //return available node closest to target
            return AI.LowestHScore(possiblePaths, pos, target);
        }
    }
}