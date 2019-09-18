using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PacMan {
    public class AI {
        /// <summary>
        /// Find closest neighbor to target
        /// </summary>
        /// <param name="neighbors"></param>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Point LowestHScore(List<Point> neighbors, Point pos, Point target) {
            double lowestScore = double.MaxValue;
            Point result = new Point();
            foreach (Point p in neighbors) {
                Point d = target - (pos + p) * GameData.TileSize;
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