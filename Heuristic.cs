using EETuring.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EETuring
{
    public class Heuristic
    {
        private double[,] heuristicMap;

        public Point A { get; set; }
        public Point B { get; set; }

        private double Dis(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
        }

        private double Dis(double x1, double x2, double y1, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public double DisBetween(PlayerNode a, PlayerNode b)
        {
            return Dis(a.X, b.X, a.Y, b.Y) / 16;
        }

        /// <summary>
        /// weight of two nodes
        /// </summary>
        public double EstimateCost(PlayerNode u)
        {
            return Dis(u.X, u.Y, B.x * 16, B.y * 16);
            //return heuristicMap[u.BX, u.BY];
        }

        private Point[] ParentLocations(Point current, int gridWidth, int gridHeight)
        {
            List<Point> possible = new List<Point>();
            if (current.x != 0)
                possible.Add(new Point(current.x - 1, current.y));
            if (current.x != 0 && current.y != 0)
                possible.Add(new Point(current.x - 1, current.y - 1));
            if (current.y != 0)
                possible.Add(new Point(current.x, current.y - 1));
            if (current.x != gridWidth - 1 && current.y != 0)
                possible.Add(new Point(current.x + 1, current.y - 1));
            if (current.x != gridWidth - 1)
                possible.Add(new Point(current.x + 1, current.y));
            if (current.x != gridWidth - 1 && current.y != gridHeight - 1)
                possible.Add(new Point(current.x + 1, current.y + 1));
            if (current.y != gridHeight - 1)
                possible.Add(new Point(current.x, current.y + 1));
            if (current.x != 0 && current.y != gridHeight - 1)
                possible.Add(new Point(current.x - 1, current.y + 1));
            return possible.ToArray();
        }

        private void PerformDecay(Point[] solution)
        {
            Queue<Point> nodes = new Queue<Point>();
            for (int i = 0; i < solution.Length; i++)
            {
                nodes.Enqueue(solution[i]);
            }

            List<Point> closedSet = new List<Point>();
            closedSet.AddRange(solution);
            bool startNode = true;

            while (nodes.Count > 0)
            {
                Point cur = nodes.Dequeue();
                
                Point[] parents = ParentLocations(cur, heuristicMap.GetLength(0), heuristicMap.GetLength(1));

                double avgTotal = 0;
                int length = 0;
                double highest = 0;
                for (int j = 0; j < parents.Length; j++)
                {
                    if (!closedSet.Contains(parents[j]))
                    {
                        nodes.Enqueue(parents[j]);
                        closedSet.Add(parents[j]);                    
                    }
                    else
                    {
                        avgTotal += heuristicMap[parents[j].x, parents[j].y];
                        highest = Math.Max(highest, heuristicMap[parents[j].x, parents[j].y]);
                        length++;
                    }
                }

                if (length != 0 && !startNode)
                {
                    heuristicMap[cur.x, cur.y] = highest + (avgTotal / length);
                }
                
                startNode = false;
            }
        }

        private void Invert(double z)
        {
            for (int y = 0; y < heuristicMap.GetLength(1); y++)
            {
                for (int x = 0; x < heuristicMap.GetLength(0); x++)
                {
                    heuristicMap[x, y] = z - heuristicMap[x, y];
                }
            }
        }

        public bool Calculate(WorldData data, bool usePhysics)
        {
            PathFinder pathFinder = new PathFinder(data, usePhysics);
            Point[] solution = pathFinder.Solve(A, B);

            if (solution != null)
            {
                heuristicMap = new double[data.Width, data.Height];
                for (int i = 0; i < solution.Length; i++)
                {
                    heuristicMap[solution[i].x, solution[i].y] = i + 1;
                }

                PerformDecay(solution);
                Invert(solution.Length);

                return true;
            }

            return solution != null;
        }
    }
}
