using EETuring.Physics;

namespace EETuring
{
    public class FillNode
    {
        public Point P { get; set; }
        public int HitValue { get; set; }

        public FillNode (Point p)
        {
            P = p;
            HitValue = 1;
        }

        public FillNode(Point p, int value)
        {
            P = p;
            HitValue = value;
        }
    }
}
