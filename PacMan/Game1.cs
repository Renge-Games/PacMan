using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace PacMan {
    public class Game1 : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D spriteSheet;
        Level level;
        Player player;
        List<Enemy> enemies;
        Texture2D coverUp;
        SpriteFont font;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 28 * 24;
            graphics.PreferredBackBufferHeight = 200 + 31 * 24;
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            base.Initialize();
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteSheet = Content.Load<Texture2D>("sheet");
            font = Content.Load<SpriteFont>("font32");
            GameData.LevelOffset = new Point(0, 150);
            GameData.SpriteSheet = spriteSheet;
            GameData.TileCount = new Point(28, 31);
            GameData.TileSize = new Point(24);
            GameData.SmallPillScore = 10;
            GameData.BigPillScore = 250;
            GameData.FruitScore = 500;
            GameData.GhostEatScore = 100;
            GameData.GhostEatMultiplier = 2;
            GameData.LevelCompleteScore = 1000;
            GameData.GameLevel = 0;

            using (StreamReader sr = new StreamReader("score.txt")) {
                GameData.HighScore = int.Parse(sr.ReadLine());
            }

            Color[] c = new Color[] { Color.White };
            coverUp = new Texture2D(GraphicsDevice, 1, 1);
            coverUp.SetData(c);

            Reload("levels/level1");
            GameData.GameState = GameState.Start;
        }

        protected void Reload(string lvl) {
            level = new Level();
            level.Load(lvl, Content);
            GameData.CurrentLevel = level;
            GameData.TotalPillCount = level.GetRemainingPillCount();
            GameData.GameLevel++;
            int lives = 0;
            int score = 0;
            player = new Player(level.GetPlayerOrigin() * GameData.TileSize);
            if (GameData.Player != null) {
                lives = GameData.Player.Lives;
                score = GameData.Player.Score;
                player.Lives = lives;
                player.Score = score;
            }
            player.Speed = Math.Min(8, 0.75f + 1.2f * (GameData.GameLevel + 1) * 0.5f);
            GameData.Player = player;
            player.Start();

            List<Point> eo = level.GetEnemyOrigins();
            enemies = new List<Enemy>();
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
        }

        protected override void UnloadContent() {
            using (StreamWriter sw = new StreamWriter("score.txt", false)) {
                sw.WriteLine(GameData.HighScore);
            }
            Content.Unload();
        }

        int counter = 0;
        GameState prevState;
        protected override void Update(GameTime gameTime) {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (counter > 0) {
                counter--;
            } else {
                switch (GameData.GameState) {
                    case GameState.Start:
                        prevState = GameState.Start;
                        counter = 100;
                        GameData.GameState = GameState.Playing;
                        player.Update();
                        foreach (var e in GameData.Enemies)
                            e.Update();
                        break;
                    case GameState.Playing:
                        prevState = GameState.Playing;
                        player.Update();
                        foreach (var e in GameData.Enemies)
                            e.Update();
                        if (GameData.HighScore < GameData.Player.Score)
                            GameData.HighScore = GameData.Player.Score;

                        if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W)) {
                            player.SetNextMove(new Point(0, -1));
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S)) {
                            player.SetNextMove(new Point(0, 1));
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A)) {
                            player.SetNextMove(new Point(-1, 0));
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D)) {
                            player.SetNextMove(new Point(1, 0));
                        }

                        if (GameData.CurrentLevel.GetRemainingPillCount() == 0) {
                            GameData.GameState = GameState.NextLevel;
                            counter = 100;
                        }
                        if (GameData.Player.Lives <= 0) {
                            GameData.GameState = GameState.GameOver;
                            counter = 100;
                        }
                        break;
                    case GameState.GameOver:
                        GameData.GameLevel = 0;
                        GameData.GameState = GameState.NextLevel;
                        GameData.Player.Lives = 3;
                        GameData.Player.Score = 0;
                        break;
                    case GameState.NextLevel:
                        Random rand = new Random();
                        int lvl = rand.Next(1, 3);
                        if (GameData.GameLevel < 5)
                            Reload("levels/level1");
                        else {
                            Reload("levels/level2");
                        }
                        GameData.GameState = GameState.Start;
                        break;
                    case GameState.AteGhost:
                        if(prevState != GameState.AteGhost) {
                            counter = 20;
                            prevState = GameState.AteGhost;
                        } else {
                            GameData.GameState = GameState.Playing;
                        }
                        break;
                }//switch
            }//else



            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);

            level.Draw(spriteBatch);
            foreach (var e in GameData.Enemies) {
                if (e.CurrentModifier != AIModifier.Jailing)
                    e.Draw(spriteBatch);
            }
            if (GameData.GameState != GameState.AteGhost) {
                player.Draw(spriteBatch);
                foreach (var e in GameData.Enemies) {
                    if (e.CurrentModifier == AIModifier.Jailing)
                        e.Draw(spriteBatch);
                }
            } else {
                string ges = (GameData.GhostEatScore * (GameData.GhostEatMultiplier * GameData.Player.GhostEatCount)).ToString();
                spriteBatch.DrawString(font, ges, GameData.Player.Position + GameData.LevelOffset.ToVector2(), Color.White, 0, font.MeasureString(ges) / 2, 0.5f, SpriteEffects.None, 0);
            }
            spriteBatch.Draw(coverUp, new Rectangle(0, 0, GameData.TileCount.X * GameData.TileSize.X, GameData.LevelOffset.Y), Color.Black);
            spriteBatch.Draw(coverUp, new Rectangle(0, GameData.TileSize.Y * GameData.TileCount.Y + GameData.LevelOffset.Y, GameData.TileCount.X * GameData.TileSize.X, 50), Color.Black);
            for (int i = 0; i < GameData.Player.Lives; i++) {
                Color gc = Colors.FruitStart.ToColor();
                Point gp = new Point(14 * 5, gc.G * 8);
                spriteBatch.Draw(GameData.SpriteSheet, new Rectangle(10 + 60 * i, GameData.TileSize.Y * GameData.TileCount.Y + GameData.LevelOffset.Y, 50, 50),
                    new Rectangle(gp, new Point(14)), Color.White);
            }
            spriteBatch.DrawString(font, "High-Score", new Vector2(GameData.TileCount.X * GameData.TileSize.X / 2, 10), Color.White, 0, new Vector2(font.MeasureString("High-Score").X / 2, 0), 1f, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, GameData.HighScore.ToString(), new Vector2(GameData.TileCount.X * GameData.TileSize.X / 2, 70), Color.White, 0, new Vector2(font.MeasureString(GameData.HighScore.ToString()).X / 2, 0), 1f, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Score: " + GameData.Player.Score, new Vector2(10, 10), Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, "Level: " + GameData.GameLevel, new Vector2(10, 40), Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
