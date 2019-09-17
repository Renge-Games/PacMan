using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace PacMan {
    public class Player : Entity {
        public int Lives { get; set; }
        public int Score { get; set; }
        public int CollectedPills { get; set; }
        public bool IsDead { get; set; }
        public int GhostEatCount { get; internal set; }

        protected Point origin;

        protected List<Point> death;

        public Player(Point originPosition)
            : base(originPosition, new Point(-1, 0), 3, 3, new Point(0, 3), new Point((int)SpriteSize.Player)) {
            Lives = 3;
            Score = 0;
            IsDead = false;
            origin = originPosition;

            //Setting up animation frames
            up = new List<Point>(); up.Add(new Point(0, 0)); up.Add(new Point(4, 0)); up.Add(new Point(4, 1));
            down = new List<Point>(); down.Add(new Point(0, 0)); down.Add(new Point(3, 0)); down.Add(new Point(3, 1));
            left = new List<Point>(); left.Add(new Point(0, 0)); left.Add(new Point(2, 0)); left.Add(new Point(2, 1));
            right = new List<Point>(); right.Add(new Point(0, 0)); right.Add(new Point(1, 0)); right.Add(new Point(1, 1));

            death = new List<Point>(); death.Add(new Point(0, 2)); death.Add(new Point(1, 2)); death.Add(new Point(2, 2));
            death.Add(new Point(3, 2)); death.Add(new Point(4, 2)); death.Add(new Point(5, 2)); death.Add(new Point(6, 2));
            death.Add(new Point(7, 2)); death.Add(new Point(8, 2)); death.Add(new Point(9, 2));
        }

        public void Kill() {
            IsDead = true;
            animCounter = 0;
            foreach(var e in GameData.Enemies) {
                e.Stop();
            }
        }

        public override void Update() {
            if (!IsDead) {
                base.Update();

                Move();

                bool enemyDead = false;
                foreach(var e in GameData.Enemies) {
                    if(e.CurrentModifier == AIModifier.Jailing) {
                        enemyDead = true;
                        break;
                    }
                }
                if (!enemyDead)
                    GhostEatCount = 0;

                if (!GameData.OutOfBounds(GridPosition)) {
                    if (GameData.CurrentLevel.Tiles[GridPosition.X, GridPosition.Y].IsSmallPill()) {
                        CollectedPills++;
                        Score += GameData.SmallPillScore;
                        GameData.CurrentLevel.RemoveAt(GridPosition);
                    } else if (GameData.CurrentLevel.Tiles[GridPosition.X, GridPosition.Y].IsBigPill()) {
                        CollectedPills++;
                        Score += GameData.BigPillScore;
                        foreach (var e in GameData.Enemies) {
                            e.SetModifier(AIModifier.Frightened);
                        }
                        GameData.CurrentLevel.RemoveAt(GridPosition);
                    }
                }
            } else {
                SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * death[(int)animCounter], frameSize);

                animCounter += 0.1f;
                if (animCounter > death.Count) {
                    Position = origin.ToVector2();
                    IsDead = false;
                    Direction = new Point(-1, 0);
                    TargetGridPosition = GridPosition + Direction;
                    List<Point> eo = GameData.CurrentLevel.GetEnemyOrigins();
                    List<Enemy> enemies = new List<Enemy>();
                    enemies.Add(new Enemy(eo[0] * GameData.TileSize, EnemyType.Blue));
                    enemies.Add(new Enemy(eo[1] * GameData.TileSize, EnemyType.Red));
                    enemies.Add(new Enemy(eo[2] * GameData.TileSize, EnemyType.Pink));
                    enemies.Add(new Enemy(eo[3] * GameData.TileSize, EnemyType.Orange));
                    GameData.Enemies = enemies;
                    enemies[0].Start();
                    enemies[1].Start();
                    enemies[2].Start();
                    enemies[3].Start();
                    enemies[0].SetSpeed(Math.Min(7f, 0.5f + 1.1f * (GameData.GameLevel + 1) * 0.5f));
                    enemies[1].SetSpeed(Math.Min(7f, 0.5f + 1.1f * (GameData.GameLevel + 1) * 0.5f));
                    enemies[2].SetSpeed(Math.Min(7f, 0.5f + 1.1f * (GameData.GameLevel + 1) * 0.5f));
                    enemies[3].SetSpeed(Math.Min(7f, 0.5f + 1.1f * (GameData.GameLevel + 1) * 0.5f));
                    GhostEatCount = 0;
                    Lives--;
                }
            }
        }
    }
}