using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PacMan {
    public class Entity {

        protected float animCounter;
        protected int frameCount;
        protected Point spriteSheetOrigin;
        protected Point frameSize;
        protected List<Point> up;
        protected List<Point> down;
        protected List<Point> left;
        protected List<Point> right;
        protected Point drawOrigin;
        protected float remainingPixelsToTarget;

        //flag indicating movement
        protected bool stopped;
        protected Point nextMove;
        public Vector2 Position { get; set; }
        public Point TargetPosition { get; set; }
        public Rectangle SourceRect { get; set; }
        public float Speed { get; set; }
        public Point Size { get; set; }
        public Point GridPosition {
            get {
                Point p = Position.ToPoint() / GameData.TileSize;
                if (Position.X < 0)
                    p.X -= 1;
                if (Position.Y < 0)
                    p.Y -= 1;
                return p;
            }
            set {
                Position = (value * GameData.TileSize + GameData.TileSize / new Point(2)).ToVector2();
            }
        }
        public Point TargetGridPosition {
            get {
                return TargetPosition / GameData.TileSize;
            }
            set {
                TargetPosition = value * GameData.TileSize;
            }
        }
        public Point Direction { get; set; }

        /// <summary>
        /// Initialize an Entity
        /// </summary>
        /// <param name="originPosition">The Entity position at the start of the level</param>
        /// <param name="targetDirection">The direction the entity will move in immediately</param>
        /// <param name="speed">The speed in which the entity will traverse the map</param>
        /// <param name="frameCount">the amount of animation frames per animation</param>
        /// <param name="spriteOrigin">the grid position of the sprite in the sprite map</param>
        /// <param name="frameSize">the size of one frame (sprite) in pixels</param>
        public Entity(Point originPosition, Point targetDirection, float speed,
            int frameCount, Point spriteOrigin, Point frameSize) {

            this.frameCount = frameCount;
            spriteSheetOrigin = spriteOrigin;
            this.frameSize = frameSize;
            drawOrigin = frameSize / new Point(2);

            Size = (GameData.TileSize.ToVector2() * (frameSize.ToVector2() / new Vector2((int)SpriteSize.Tile))).ToPoint();
            Position = (originPosition + new Point(0, GameData.TileSize.Y / 2)).ToVector2();
            TargetGridPosition = GridPosition + targetDirection;
            Direction = targetDirection;
            Speed = speed;
            stopped = true;
            remainingPixelsToTarget = GameData.TileSize.X / 2;
        }

        /// <summary>
        /// Sets the next movement target for the entity (Entity will move to this target when possible)
        /// </summary>
        /// <param name="p">The movement direction</param>
        public void SetNextMove(Point p) {
            nextMove = p;
        }

        /// <summary>
        /// Move the entity toward the specified position at its maximum speed
        /// </summary>
        /// <param name="p">the target location</param>
        public void MoveTo(Point p) {
            TargetPosition = p;
            if (Position.ToPoint() != TargetPosition) {
                Position += Position - TargetPosition.ToVector2() * Speed;
            }
        }

        /// <summary>
        /// Move the entity to the target node at its maximum speed
        /// </summary>
        public void Move(bool IgnoreDoors = false) {
            if (IsMoving(true)) {
                Position += Direction.ToVector2() * new Vector2(Speed);
                remainingPixelsToTarget -= Speed;
            }
            if (remainingPixelsToTarget <= 0 || !IsMoving(true)) {
                remainingPixelsToTarget = GameData.TileSize.X;
                TargetGridPosition = GridPosition + Direction;
                GridPosition = GridPosition; // this is correct because of the way it's defined (check get and set of GridPosition). Basically aligns the entity to the center of the node

                Point np = GridPosition + nextMove; //next position
                if (nextMove != null && nextMove != new Point(0) && !GameData.OutOfBounds(GridPosition) && 
                    (GameData.OutOfBounds(np) || IgnoreDoors && GameData.CurrentLevel.Tiles[np.X, np.Y].TileColor == Colors.Door || !GameData.CurrentLevel.Tiles[np.X, np.Y].IsWall())) {
                    Direction = nextMove;
                    TargetGridPosition = np;
                    nextMove = new Point(0);
                }
            }

            if (GridPosition.Y < -1) {
                GridPosition = new Point(GridPosition.X, GameData.TileCount.Y);
                TargetPosition = new Point(TargetPosition.X, GameData.TileCount.Y - 1);
            }
            if (GridPosition.Y > GameData.TileCount.Y + 1) {
                GridPosition = new Point(GridPosition.X, -1);
                TargetPosition = new Point(TargetPosition.X, 0);
            }
            if (GridPosition.X < -1) {
                GridPosition = new Point(GameData.TileCount.X, GridPosition.Y);
                TargetPosition = new Point(GameData.TileCount.X - 1, TargetPosition.Y);
            }
            if (GridPosition.X > GameData.TileCount.X + 1) {
                GridPosition = new Point(-1, GridPosition.Y);
                TargetPosition = new Point(0, TargetPosition.Y);
            }
        }

        public virtual void Update() {
            if (IsMoving(true)) {
                if (animCounter >= frameCount)
                    animCounter = 0;

                if (Direction.X < 0) {
                    SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * left[(int)animCounter], frameSize);
                } else if (Direction.X > 0) {
                    SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * right[(int)animCounter], frameSize);
                } else if (Direction.Y < 0) {
                    SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * up[(int)animCounter], frameSize);
                } else if (Direction.Y > 0) {
                    SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * down[(int)animCounter], frameSize);
                }

                animCounter += 0.1f;
            }
            if (stopped) {
                SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * left[0], frameSize);
            }
        }

        public virtual bool IsMoving(bool considerStoppedFlag) {
            if (TargetPosition != null && (!considerStoppedFlag || !stopped)) {
                if (TargetGridPosition.X < 0 || TargetGridPosition.X >= GameData.TileCount.X ||
                   TargetGridPosition.Y < 0 || TargetGridPosition.Y >= GameData.TileCount.Y)
                    return true;
                if (!GameData.CurrentLevel.Tiles[TargetGridPosition.X, TargetGridPosition.Y].IsWall())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Stop movement
        /// </summary>
        public void Stop() {
            stopped = true;
        }

        /// <summary>
        /// Start movement
        /// </summary>
        public void Start() {
            stopped = false;
        }

        public virtual void Draw(SpriteBatch sb) {

            sb.Draw(GameData.SpriteSheet, new Rectangle(Position.ToPoint() + GameData.LevelOffset, Size), SourceRect, Color.White, 0.0f, drawOrigin.ToVector2(), SpriteEffects.None, 0);
        }
    }
}