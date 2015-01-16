using EETuring.Physics;
using System.Collections.Generic;

namespace EETuring
{
    public class Path
    {
        private List<Point> pathPoints;

        /// <summary>
        /// Gets the length of the path in blocks
        /// </summary>
        public int Length { get { return pathPoints.Count; } }

        /// <summary>
        /// Inserts a new point into the path
        /// </summary>
        public void Map(Point pt)
        {
            pathPoints.Insert(0, pt);
        }

        /// <summary>
        /// Gets the mapped path of points
        /// </summary>
        public List<Point> GetPoints()
        {
            return pathPoints;
        }

        /// <summary>
        /// Create a new empty path
        /// </summary>
        public Path()
        {
            pathPoints = new List<Point>();
        }

        /// <summary>
        /// Create a path from a list of direct path points
        /// </summary>
        public Path(Point[] directPath)
        {
            pathPoints = new List<Point>(directPath);
        }
    }
}
