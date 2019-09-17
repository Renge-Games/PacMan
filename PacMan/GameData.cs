using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace PacMan {
    static class GameData {
        public static Texture2D SpriteSheet { get; set; }
        public static Level CurrentLevel { get; set; }
        public static Point TileCount { get; set; }
        public static Point TileSize { get; set; }
        public static Player Player { get; set; }
        public static List<Enemy> Enemies { get; set; }
        public static int TotalPillCount { get; internal set; }
        public static int SmallPillScore { get; internal set; }
        public static int BigPillScore { get; internal set; }
        public static int FruitScore { get; internal set; }
        public static int GhostEatScore { get; internal set; }
        public static int GhostEatMultiplier { get; internal set; }
        public static int LevelCompleteScore { get; internal set; }
        public static Point LevelOffset { get; internal set; }
        public static GameState GameState { get; set; }
        public static int GameLevel { get; set; }
        public static int HighScore { get; internal set; }

        public static bool OutOfBounds(Point p) {
            if (p.X < 0 || p.Y < 0 || p.X >= TileCount.X || p.Y >= TileCount.Y)
                return true;
            return false;
        }
    }
}
