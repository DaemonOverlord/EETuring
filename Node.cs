using EETuring.Physics;

namespace EETuring
{
    public class Node
    {
        public Point Point { get; set; }
        public PlayerState State { get; set; }

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
