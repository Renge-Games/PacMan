using Microsoft.Xna.Framework;

namespace PacMan {
    public class Tile {
        public Colors TileColor { get; set; }
        public Rectangle SourceRect { get; set; }

        public bool IsWall() {
            if (TileColor == Colors.BigPill || TileColor == Colors.SmallPill ||
               TileColor == Colors.OriginPlayerPos || TileColor == Colors.OriginEnemyPos ||
               TileColor == Colors.UpwardsRestricted || TileColor == Colors.Nothing)
                return false;
            return true;
        }

        public bool IsBigPill() {
            if (TileColor == Colors.BigPill)
                return true;
            return false;
        }

        public bool IsSmallPill() {
            if (TileColor == Colors.SmallPill)
                return true;
            return false;
        }
    }
}