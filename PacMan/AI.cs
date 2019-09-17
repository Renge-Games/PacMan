using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PacMan {
    public class AI {
        public static Point LowestHScore(List<Point> neighbors, Point pos, Point target) {
            double lowestScore = double.MaxValue;
            Point result = new Point();
            Point d = new Point();
            foreach (Point p in neighbors) {
                d = target - (pos + p) * GameData.TileSize;
                double mag = Math.Sqrt(d.X * d.X + d.Y * d.Y);
                if (mag < lowestScore) {
                    lowestScore = mag;
                    result = p;
                }
            }
            return result;
        }
    }
}