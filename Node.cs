using EETuring.Physics;
using System.Collections.Generic;

namespace EETuring
{
    public class Node
    {
        public double Cost { get; set; }
        public Point Point { get; set; }
        public List<PlayerNode> PNodes { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Point)
            {
                Point p = (Point)obj;
                return p.Equals(Point);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public Node(Point p)
        {
            Point = p;
        }
    }
}
