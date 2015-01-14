using EETuring.Physics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EETuring
{
    public class PathFinder
    {
        private int[][] world;
        private bool usePhysics;
        private Point start, end;

        private WorldData data;
        private PhysicsWorld w;
        private PhysicsPlayer p;

        /// <summary>
        /// Gets adjacent inbound nodes
        /// </summary>
        private Point[] GetAdjacent(Point current, int gridWidth, int gridHeight)
        {
            List<Point> possible = new List<Point>();
            if (current.x != 0) possible.Add(new Point(current.x - 1, current.y));
            if (current.y != 0) possible.Add(new Point(current.x, current.y - 1));
            if (current.x != gridWidth - 1) possible.Add(new Point(current.x + 1, current.y));
            if (current.y != gridHeight - 1) possible.Add(new Point(current.x, current.y + 1));
            return possible.ToArray();
        }

        /// <summary>
        /// Calculates safe parents for the path to use
        /// </summary>
        /// <param name="n">Current node</param>
        /// <param name="used">Used nodes</param>
        private IEnumerator<Point> SafeParents(Node n, List<Point> used)
        {
            Point current = n.Point;

            int currentB = world[current.x][current.y];
            Point[] locations = GetAdjacent(current, world.Length, world[0].Length);
            for (int i = 0; i < locations.Length; i++)
            {
                int cx = locations[i].x - current.x;
                int cy = locations[i].y - current.y;

                int parentBlock = world[locations[i].x][locations[i].y];
                if (usePhysics)
                {
                    switch (parentBlock)
                    {
                        case 0:
                            if (n.State.SpeedY <= -PhysicsConfig.Gravity || cy > 0)
                            {
                                break;
                            }

                            //check if there is a solid block below the location within a distance of ~10
                            bool valid = false;
                            int sx = locations[i].x;
                            int sy = locations[i].y;
                            for (int x = 0; x < data.Width; x++)
                            {
                                for (int y = sy + 1; y < sy + 4; y++)
                                {
                                    if (y < data.Height - 1)
                                    {
                                        bool canJumpOn = ItemId.CanJumpOn(world[x][y]);
                                        if (!canJumpOn && world[x][y] == 4)
                                        {
                                            if (x > 0)
                                            {
                                                if (ItemId.CanJumpOn(world[x - 1][y]))
                                                {
                                                    canJumpOn = true;
                                                }
                                            }

                                            if (x < data.Width - 1)
                                            {
                                                if (ItemId.CanJumpOn(world[x + 1][y]))
                                                {
                                                    canJumpOn = true;
                                                }
                                            }
                                        }

                                        if (canJumpOn && ItemId.IsSolid(world[x][y + 1]))
                                        {
                                            double dis = Math.Sqrt(Math.Pow(sx - x, 2) + Math.Pow(sy - y, 2));
                                            if (dis <= 10)
                                            {
                                                valid = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (valid)
                                {
                                    break;
                                }
                            }

                            if (valid)
                            {
                                break;
                            }
                            continue;
                    }
                }

                if (!ItemId.IsSolid(parentBlock) && !used.Contains(locations[i]))
                {
                    yield return locations[i];
                }
            }
            yield break;
        }

        /// <summary>
        /// Gets a list of points sorted by heuristic cost value
        /// </summary>
        /// <param name="current">Current node point</param>
        /// <param name="diagram">Diagram of values</param>
        /// <param name="offset">Offset for pre elimination</param>
        private Point[] Heuristic(Point current, Diagram diagram, int offset)
        {
            int best = 0;
            List<Point> bestParents = new List<Point>();
            Point[] parentLocations = GetAdjacent(current, world.Length, world[0].Length  );
            for (int i = 0; i < parentLocations.Length; i++)
            {
                int heu = diagram[parentLocations[i]];
                if (heu > best)
                {
                    best = heu;
                    bestParents.Add(parentLocations[i]);
                }
            }
            //bestParents.RemoveAll(t => Math.Abs(diagram[t] - best) > offset);
            return bestParents.ToArray();
        }

        /// <summary>
        /// Gets the availible nodes
        /// </summary>
        /// <param name="p">Current node</param>
        /// <param name="used">Used nodes</param>
        /// <returns></returns>
        private int GetAvailible(Node p, List<Point> used)
        {
            IEnumerator<Point> par = SafeParents(p, used);
            int total = 0;
            do
            {
                total++;
            }
            while (par.MoveNext());
            return total;
        }

        /// <summary>
        /// Gives the closest possible unused node
        /// </summary>
        /// <param name="current">Current node</param>
        /// <param name="used">Used nodes</param>
        private Node ClosestPossible(Node current, List<Point> used)
        {
            double closestDis = double.MaxValue;
            Point closest = null;
            IEnumerator<Point> parents = SafeParents(current, used);
            while(parents.MoveNext())
            {
                Point cur = parents.Current;
                double curDis = Math.Sqrt(Math.Pow(cur.x - end.x, 2) + Math.Pow(cur.y - end.y, 2));
                if (curDis < closestDis)
                {
                    closestDis = curDis;
                    closest = cur;
                }
            }

            if (closest == null)
            {
                return null;
            }
            else
            {
                return new Node(closest);
            }
        }

        /// <summary>
        /// Fiinds a path given a start and end
        /// </summary>
        /// <param name="start">Start of path</param>
        /// <param name="end">Path goal</param>
        public Point[] Solve(Point start, Point end)
        {
            this.start = start;
            this.end = end;

            List<Point> path = new List<Point>();
            Diagram diagram = new Diagram(world.Length, world[0].Length);
            Queue<Node> nodes = new Queue<Node>();
            Queue<Node> checkpoints = new Queue<Node>();
            List<Node> pastcheckpoints = new List<Node>();
            nodes.Enqueue(new Node(start));

            bool found = false;
            int  i = 0;
            while (nodes.Count > 0)
            {
                Node current = nodes.Dequeue();
                current.State = p.Tick(new PlayerState(current.Point), Input.Nothing, 1);

                if (current.Equals(end))
                {
                    found = true;
                    diagram.Set(current.Point, i++);
                    break;
                }

                if (GetAvailible(current, path) > 0 && !pastcheckpoints.Contains(current))
                {
                    checkpoints.Enqueue(current);
                    pastcheckpoints.Add(current);
                }

                path.Add(current.Point);
                diagram.Set(current.Point, i++);              
                Node next = ClosestPossible(current, path);
                if (next == null)
                {
                    if (checkpoints.Count > 0)
                    {
                        Node check = checkpoints.Dequeue();
                        nodes.Enqueue(check);
                    }
                    else
                    {
                        break;
                    }
                }
                else if (!path.Contains(next.Point))
                {
                    nodes.Enqueue(next);
                }
                else
                {
                    break;
                }
            }

            if (found)
            {
                Point[] constructedPath = ConstructPath(diagram, start, end, 0);
                if (constructedPath == null)
                {
                    return null;
                }

                if (constructedPath.Length == 1)
                {
                    return new Point[] { end };
                }

                return constructedPath;
            }
            
            return null;
        }

        /// <summary>
        /// Constructs a path from heuristics
        /// </summary>
        /// <param name="diagram">Diagram of values</param>
        /// <param name="start">Start pt</param>
        /// <param name="end">End pt</param>
        /// <param name="offset">Offset of elimination</param>
        private Point[] ConstructPath(Diagram diagram, Point start, Point end, int offset)
        {
            Queue<Point> joints = new Queue<Point>();
            Queue<Point[]> leftover = new Queue<Point[]>();
            joints.Enqueue(start);

            while (joints.Count > 0)
            {
                Point head = joints.Dequeue();
                List<Point> traveled = new List<Point>();
                if (leftover.Count > 0)
                {
                    traveled.AddRange(leftover.Dequeue());
                }

                Queue<Point> nodes = new Queue<Point>();
                nodes.Enqueue(head);

                int i = 0;
                while (nodes.Count > 0)
                {
                    Point current = nodes.Dequeue();
                    if (current.Equals(end))
                    {
                        return traveled.ToArray();
                    }

                    diagram.Set(current, i++);
                    traveled.Add(current);
                    Point[] paths = Heuristic(current, diagram, offset);
                    for (int j = paths.Length - 1; j >= 0; j--) //Go backwards because the last node will have the highest heuristic value, refer to Heuristic()
                    {
                        if (j == paths.Length - 1)
                        {
                            if (!traveled.Contains(paths[j]))
                            {
                                nodes.Enqueue(paths[j]);
                            }
                        }
                        else
                        {
                            if (!joints.Contains(paths[j]))
                            {
                                joints.Enqueue(paths[j]);
                                leftover.Enqueue(traveled.ToArray());
                            }
                        }
                    }

                    if (paths.Length == 0)
                    {
                        break;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a pathfinder
        /// </summary>
        /// <param name="data">WorldData</param>
        /// <param name="usePhysics">option of using a physics biased path</param>
        public PathFinder(WorldData data, bool usePhysics)
        {
            this.data = data;
            this.w = new PhysicsWorld(data);
            this.p = new PhysicsPlayer(w);
            this.world = data.ForeGroundTiles;
            this.usePhysics = usePhysics;
        }
    }
}
