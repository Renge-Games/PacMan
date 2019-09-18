using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PacMan {
    public class Enemy : Entity {
        protected float SavedSpeed { get; set; }
        public EnemyType Type { get; set; }
        public AIModifier SavedModifier { get; set; }
        protected AIModifier PreviousModifier { get; set; }
        public AIModifier CurrentModifier { get; set; }
        protected Stopwatch Timer { get; set; }

        protected List<Point> frightened;
        protected List<Point> recovering;

        //for the sprite sheet
        protected List<Point> upD;
        protected List<Point> downD;
        protected List<Point> leftD;
        protected List<Point> rightD;

        public Enemy(Point originPosition, EnemyType type)
            : base(originPosition, new Point(-1, 0), 2, 1, new Point(0, 9), new Point(14)) {
            Type = type;
            SavedSpeed = Speed;
            int row = 0;
            switch (type) {
                case EnemyType.Red:
                    row = 4;
                    PreviousModifier = CurrentModifier = AIModifier.Chase;
                    break;
                case EnemyType.Pink:
                    row = 3;
                    PreviousModifier = CurrentModifier = AIModifier.Emerging;
                    Direction = new Point(0, -1);
                    TargetGridPosition = GridPosition + Direction;
                    break;
                case EnemyType.Blue:
                    row = 1;
                    PreviousModifier = CurrentModifier = AIModifier.Jailed;
                    Direction = new Point(0, -1);
                    TargetGridPosition = GridPosition + Direction;
                    break;
                case EnemyType.Orange:
                    row = 2;
                    PreviousModifier = CurrentModifier = AIModifier.Jailed;
                    Direction = new Point(0, -1);
                    TargetGridPosition = GridPosition + Direction;
                    break;
            }
            Timer = new Stopwatch();
            Timer.Start();

            frightened = new List<Point>(); frightened.Add(new Point(2, 0)); frightened.Add(new Point(3, 0));
            recovering = new List<Point>(); recovering.Add(new Point(0, 0)); recovering.Add(new Point(1, 0));

            upD = new List<Point>(); upD.Add(new Point(5, 0));
            downD = new List<Point>(); downD.Add(new Point(6, 0));
            leftD = new List<Point>(); leftD.Add(new Point(4, 0));
            rightD = new List<Point>(); rightD.Add(new Point(7, 0));

            up = new List<Point>(); up.Add(new Point(0, row)); up.Add(new Point(1, row));
            down = new List<Point>(); down.Add(new Point(2, row)); down.Add(new Point(3, row));
            left = new List<Point>(); left.Add(new Point(4, row)); left.Add(new Point(5, row));
            right = new List<Point>(); right.Add(new Point(6, row)); right.Add(new Point(7, row));
        }

        public void SwitchDirection() {
            Direction = new Point(-Direction.X, -Direction.Y);
            TargetGridPosition = GridPosition + Direction;
        }

        public void SetSpeed(float val) {
            SavedSpeed = val;
            Speed = val;
        }

        public override bool IsMoving(bool considerStoppedFlag) {
            switch (CurrentModifier) {
                case AIModifier.Jailing:
                case AIModifier.Emerging:
                    if (TargetPosition != null) {
                        if (TargetGridPosition.X < 0 || TargetGridPosition.X >= GameData.TileCount.X ||
                           TargetGridPosition.Y < 0 || TargetGridPosition.Y >= GameData.TileCount.Y)
                            return true;
                        if (GameData.CurrentLevel.Tiles[TargetGridPosition.X, TargetGridPosition.Y].TileColor == Colors.Door ||
                            !GameData.CurrentLevel.Tiles[TargetGridPosition.X, TargetGridPosition.Y].IsWall())
                            return true;
                    }
                    return false;
                default:
                    return base.IsMoving(considerStoppedFlag);
            }
        }

        public void SetModifier(AIModifier modifier) {
            Timer.Restart();
            CurrentModifier = modifier;
            if (SavedModifier == AIModifier.Jailing && CurrentModifier == AIModifier.Frightened)
                CurrentModifier = SavedModifier;
        }

        public override void Update() {
            base.Update();
            bool ignoreDoors = false;

            if(CurrentModifier != AIModifier.Frightened && !GameData.Player.IsDead && CurrentModifier != AIModifier.Jailing && GridPosition == GameData.Player.GridPosition) {
                GameData.Player.Kill();
            }

            switch (CurrentModifier) {
                case AIModifier.Chase:
                    SavedModifier = AIModifier.Chase;
                    Speed = SavedSpeed;
                    //after 20 seconds, lay off the player and go to one of the map corners
                    if (Timer.ElapsedMilliseconds > 20000) {
                        SetModifier(AIModifier.Scatter);
                    }
                    break;
                case AIModifier.Scatter:
                    SavedModifier = AIModifier.Scatter;
                    Speed = SavedSpeed;
                    //after 7 seconds resume chasing the player
                    if (Timer.ElapsedMilliseconds > 7000) {
                        SetModifier(AIModifier.Chase);
                    }
                    break;
                case AIModifier.Frightened:
                    if (GameData.Player.GridPosition == GridPosition) {
                        SetModifier(AIModifier.Jailing);
                        GameData.GameState = GameState.AteGhost;
                        GameData.Player.GhostEatCount++;
                        GameData.Player.Score += (GameData.GhostEatScore * GameData.GhostEatMultiplier * GameData.Player.GhostEatCount);
                    }
                    //before 6 seconds pass, give the ghost a solid blue sprite
                    //after 6 seconds, alternate blue and white sprites every 100 milliseconds to indicate end of frightened state
                    if (Timer.ElapsedMilliseconds < 6000 || Timer.ElapsedMilliseconds % 200 < 100)
                        SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * frightened[(int)animCounter], frameSize);
                    else
                        SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * recovering[(int)animCounter], frameSize);
                    Speed = SavedSpeed / 2;
                    if (Timer.ElapsedMilliseconds > 10000) {
                        SetModifier(SavedModifier);
                    }
                    break;
                case AIModifier.Jailed:
                    SavedModifier = AIModifier.Jailed;
                    Speed = SavedSpeed / 2;
                    if (Direction.X != 0) {
                        Direction = new Point(0, 1);
                        TargetGridPosition = GridPosition + Direction;
                    }
                    if (GameData.CurrentLevel.Tiles[GridPosition.X, GridPosition.Y + 1].IsWall() && Direction.Y == 1)
                        SwitchDirection();
                    else if (GameData.CurrentLevel.Tiles[GridPosition.X, GridPosition.Y - 1].IsWall() && Direction.Y == -1)
                        SwitchDirection();
                    if (GameData.Player.CollectedPills >= 30 && Type == EnemyType.Blue ||
                        GameData.CurrentLevel.GetRemainingPillCount() < GameData.TotalPillCount * 0.66 && Type == EnemyType.Orange) {
                        SetModifier(AIModifier.Emerging);
                        Direction = new Point(0, -1);
                        TargetGridPosition = GridPosition + Direction;
                    }
                    break;
                case AIModifier.Emerging:
                    SavedModifier = AIModifier.Emerging;
                    Speed = SavedSpeed / 2;
                    ignoreDoors = true;
                    if (GridPosition == GameData.CurrentLevel.GetDoorGridPosition() - new Point(0, 1)) {
                        CurrentModifier = AIModifier.Chase;
                        Timer.Restart();
                    }
                    break;
                case AIModifier.Jailing:
                    SavedModifier = AIModifier.Jailing;
                    if (Direction.X < 0) {
                        SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * leftD[0], frameSize);
                    } else if (Direction.X > 0) {
                        SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * rightD[0], frameSize);
                    } else if (Direction.Y < 0) {
                        SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * upD[0], frameSize);
                    } else if (Direction.Y > 0) {
                        SourceRect = new Rectangle(spriteSheetOrigin * new Point((int)SpriteSize.Tile) + frameSize * downD[0], frameSize);
                    }
                    ignoreDoors = true;
                    Speed = SavedSpeed * 1.2f;
                    if (GridPosition == GameData.CurrentLevel.GetDoorGridPosition() + new Point(1, 1)) {
                        CurrentModifier = AIModifier.Emerging;
                        Timer.Restart();
                    }
                    break;
            }

            if (PreviousModifier != CurrentModifier) {
                PreviousModifier = CurrentModifier;
                if (CurrentModifier != AIModifier.Jailed)
                    SwitchDirection();
                Timer.Restart();
            }

            SetNextMove(Type.BestTurn(SavedModifier, CurrentModifier, Direction, GridPosition, ignoreDoors));
            Move(ignoreDoors);
        }

        public override void Draw(SpriteBatch sb) {
            //Lousy hack
            if (CurrentModifier == AIModifier.Jailed || CurrentModifier == AIModifier.Emerging || 
                (CurrentModifier == AIModifier.Frightened && SavedModifier == AIModifier.Jailed) ||
                (CurrentModifier == AIModifier.Frightened && SavedModifier == AIModifier.Emerging)) {
                Vector2 p = Position;
                Position = Position - new Vector2(GameData.TileSize.X / 2, 0);
                base.Draw(sb);
                Position = p;
            } else {
                base.Draw(sb);
            }

                
        }
    }
}