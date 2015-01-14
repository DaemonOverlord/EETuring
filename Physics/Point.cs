namespace EETuring.Physics
{
    public class Point
    {
        public int x, y;

        public override bool Equals(object obj)
        {
            if (obj is Point)
            {
                return ((Point)obj).x == x &&
                       ((Point)obj).y == y;
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return string.Format("x: {0}, y: {1}", x, y);
        }

        public Point(int xx, int yy)
        {
            x = xx;
            y = yy;
        }
    }
}
