using EETuring.Physics;
using System;
using System.Collections.Generic;

namespace EETuring
{
    public class Diagram
    {
        private int[,] map;

        /// <summary>
        /// Checks the map has a value set at a point
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        public bool IsSet(int x, int y)
        {
            return map[x, y] >= 0;
        }

        /// <summary>
        /// Sets a heuristic at a point
        /// </summary>
        public void Set(Point p, int value)
        {
            map[p.x, p.y] = value;
        }

        /// <summary>
        /// Gets a heuristic value
        /// </summary>
        /// <param name="index">Point index</param>
        public int this[Point index]
        {
            get { return IsSet(index.x, index.y) ? map[index.x, index.y] : -1; }
            set { map[index.x, index.y] = value; }
        }

        /// <summary>
        /// Creates a new heuristic map
        /// </summary>
        /// <param name="width">Map width</param>
        /// <param name="height">Map height</param>
        public Diagram(int width, int height)
        {
            map = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = -1;
                }
            }
        }
    }
}
