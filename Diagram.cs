using EETuring.Physics;
using System;
using System.Collections.Generic;

namespace EETuring
{
    public class Diagram
    {
        private int[,] map;

        public bool IsSet(int x, int y)
        {
            return map[x, y] >= 0;
        }

        public void Set(Point p, int value)
        {
            map[p.x, p.y] = value;
        }

        public int this[Point index]
        {
            get { return IsSet(index.x, index.y) ? map[index.x, index.y] : -1; }
            set { map[index.x, index.y] = value; }
        }


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
