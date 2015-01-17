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

        private bool CheckPhysicsAir(Point loc, PlayerState state, int cx, int cy)
        {
            return state.SpeedY <= -w.WorldGravity || cy > 0;
        }
        private bool CheckPhysicsArrowLeft(Point loc, PlayerState state, int cx, int cy)
        {
            return state.SpeedX >= w.WorldGravity || cx < 0;
        }
        private bool CheckPhysicsArrowRight(Point loc, PlayerState state, int cx, int cy)
        {
            return state.SpeedX <= -w.WorldGravity || cx > 0;
        }
        private bool CheckPhysicsArrowUp(Point loc, PlayerState state, int cx, int cy)
        {
            return state.SpeedY >= w.WorldGravity || cy < 0;
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
                    bool valid = false;
                    for (int s = 0; s < n.PNodes.Count; s++)
                    {
                        switch (parentBlock)
                        {
                            case 0:
                                if (CheckPhysicsAir(locations[i], n.PNodes[s].State, cx, cy))
                                {
                                    valid = true;
                                }
                                break;
                            case 1:
                                if (CheckPhysicsArrowLeft(locations[i], n.PNodes[s].State, cx, cy))
                                {
                                    valid = true;
                                }
                                break;
                            case 2:
                                if (CheckPhysicsArrowUp(locations[i], n.PNodes[s].State, cx, cy))
                                {
                                    valid = true;
                                }
                                break;
                            case 3:
                                if (CheckPhysicsArrowRight(locations[i], n.PNodes[s].State, cx, cy))
                                {
                                    valid = true;
                                }
                                break;
                        }

                        if (valid)
                        {
                            break;
                        }
                    }

                    if (!valid)
                    {
                        continue;
                    }
                }

                if (!ItemId.IsSolid(parentBlock) && !used.Contains(locations[i]))
                {
                    yield return locations[i];
                }
            }
        }

        /// <summary>
        /// Gets a list of points sorted by heuristic cost value
        /// </summary>
        /// <param name="current">Current node point</param>
        /// <param name="diagram">Diagram of values</param>
        /// <param name="offset">Offset for pre elimination</param>
        private Point[] Heuristic(Point current, Diagram diagram, int offset)
        {
            Point[] adj = GetAdjacent(current, world.Length, world[0].Length);           
            Point temp = null;
            int inner = 0;

            for (int outer = 1; outer < adj.Length; outer++)
            {
                temp = adj[outer];
                inner = outer;
                while (inner > 0 && diagram[adj[inner - 1]] >= diagram[temp])
                {
                    adj[inner] = adj[inner - 1];
                    inner--;
                }

                adj[inner] = temp;
            } 

            return adj;
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
            OpenSet<Node> nodes = new OpenSet<Node>(n => n.Cost);
            Queue<Node> checkpoints = new Queue<Node>();
            List<Node> pastcheckpoints = new List<Node>();
            nodes.Add(new Node(start));
            path.Add(start);

            bool found = false;
            int  i = 0;
            while (nodes.Count > 0)
            {
                Node current = nodes.Dequeue();
                Point cur = current.Point;

                current.Cost = Math.Sqrt(Math.Pow(cur.x - end.x, 2) + Math.Pow(cur.y - end.y, 2));
                current.PNodes = w.GetNodes(p, current.Point, 1);

                if (current.Equals(end))
                {
                    found = true;
                    diagram.Set(current.Point, i++);
                    break;
                }
            
                diagram.Set(current.Point, i++);
                path.Add(cur);

                if (!usePhysics)
                {
                    if (GetAvailible(current, path) > 0 && !pastcheckpoints.Contains(current))
                    {
                        checkpoints.Enqueue(current);
                        pastcheckpoints.Add(current);
                    }

                    Node next = ClosestPossible(current, path);
                    if (next == null)
                    {
                        if (checkpoints.Count > 0)
                        {
                            Node check = checkpoints.Dequeue();
                            nodes.Add(check);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (!path.Contains(next.Point))
                    {
                        nodes.Add(next);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    IEnumerator<Point> safe = SafeParents(current, path);
                    while (safe.MoveNext())
                    {                     
                        nodes.Add(new Node(safe.Current));
                    }
                }
            }

            if (found)
            {
                Point[] constructedPath = ConstructPath(diagram, start, end, 0);
                if (constructedPath == null)
                {
                    return null;
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
                        traveled.Add(end);
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
