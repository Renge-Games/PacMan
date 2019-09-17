using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PacMan {
    public class Level {
        public Tile[,] Tiles { get; set; }

        /// <summary>
        /// Gets the count of the remaining pills on the level
        /// </summary>
        /// <returns></returns>
        public int GetRemainingPillCount() {
            int count = 0;
            foreach(var t in Tiles) {
                if (t.TileColor == Colors.BigPill || t.TileColor == Colors.SmallPill)
                    count++;
            }
            return count;
        }

        public Point GetDoorGridPosition() {
            for (int x = 0; x < GameData.TileCount.X; x++) {
                for (int y = 0; y < GameData.TileCount.Y; y++) {
                    if (Tiles[x, y].TileColor == Colors.Door)
                        return new Point(x, y);
                }
            }
            return new Point();
        }

        /// <summary>
        /// Load a map from a file in the "Content" directory
        /// </summary>
        /// <param name="path">the name of the level file</param>
        /// <param name="content">the ContentManager object from Game1</param>
        public void Load(string path, ContentManager content) {
            //load the .png map into a texture2d and put the data into an array of colors
            Texture2D map = content.Load<Texture2D>(path);
            Color[] c = new Color[map.Width * map.Height];
            map.GetData(c);

            Tiles = new Tile[GameData.TileCount.X, GameData.TileCount.Y];

            //load the map with the color array
            for(int x = 0; x < GameData.TileCount.X; x++) {
                for(int y = 0; y < GameData.TileCount.Y; y++) {
                    Color tmp = c[x + y * GameData.TileCount.X];
                    Tiles[x, y] = new Tile() {
                        TileColor = tmp.ToColors(),
                        SourceRect = new Rectangle(tmp.R * (int)SpriteSize.Tile, tmp.G * (int)SpriteSize.Tile, (int)SpriteSize.Tile, (int)SpriteSize.Tile)
                    };
                }
            }
        }

        /// <summary>
        /// Set a node to the Colors.Nothing tile.
        /// Useful for removing Pills.
        /// </summary>
        /// <param name="p">The node's location on the grid</param>
        public void RemoveAt(Point p) {
            Tiles[p.X, p.Y] = new Tile() {
                TileColor = Colors.Nothing,
                SourceRect = new Rectangle()
            };
        }

        /// <summary>
        /// Get the player origin
        /// </summary>
        /// <returns>Returns a Point object of the player origin</returns>
        public Point GetPlayerOrigin() {
            for (int x = 0; x < GameData.TileCount.X; x++) {
                for (int y = 0; y < GameData.TileCount.Y; y++) {
                    if (Tiles[x, y].TileColor == Colors.OriginPlayerPos)
                        return new Point(x, y);
                }
            }
            return new Point(0);
        }

        /// <summary>
        /// Get all the enemy origins.
        /// Order: Blue, Red, Pink, Orange
        /// </summary>
        /// <returns>Returns a list of Point objects with the enemy origins. Item 0: Blue, item 3: Orange</returns>
        public List<Point> GetEnemyOrigins() {
            List<Point> origins = new List<Point>();
            for (int x = 0; x < GameData.TileCount.X; x++) {
                for (int y = 0; y < GameData.TileCount.Y; y++) {
                    if (Tiles[x, y].TileColor == Colors.OriginEnemyPos)
                        origins.Add(new Point(x, y));
                }
            }
            return origins;
        }

        public void Draw(SpriteBatch sb) {
            for (int x = 0; x < GameData.TileCount.X; x++) {
                for (int y = 0; y < GameData.TileCount.Y; y++) {
                    Colors c = Tiles[x, y].TileColor;
                    if (c != Colors.Nothing && c != Colors.UpwardsRestricted &&
                        c != Colors.OriginEnemyPos && c != Colors.OriginPlayerPos &&
                        c != Colors.EnemySpriteStart && c != Colors.PlayerSpriteStart && c != Colors.FruitStart) {

                        if (c == Colors.BigPill || c == Colors.SmallPill || c == Colors.Door)
                            sb.Draw(GameData.SpriteSheet, new Rectangle(GameData.TileSize * new Point(x, y) + GameData.LevelOffset, GameData.TileSize), Tiles[x, y].SourceRect, Color.White);
                        else
                            sb.Draw(GameData.SpriteSheet, new Rectangle(GameData.TileSize * new Point(x, y) + GameData.LevelOffset, GameData.TileSize), Tiles[x, y].SourceRect, Color.Blue);
                    }
                }
            }
        }
    }
}