using EETuring.Physics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EETuring
{
    public class WorkbackProcessor : IDisposable
    {
        private PhysicsPlayer player;
        private PhysicsWorld world;
        private WorldData worldData;

        private Heuristic heuristic;
        private HashTable hashTable;

        private int currentSearch = 0;
        private int currentPathNodesComplete = 0;
        private ProgressCallback pCallback;
        private TestCompleteCallback tCallback;

        /// <summary>
        /// Tests if a node is the goal
        /// </summary>
        private bool IsGoal(PlayerNode u, Point t)
        {
            return u.IsGoal(t);
        }

        /// <summary>
        /// Starts a search
        /// </summary>
        /// <param name="a">Start point</param>
        /// <param name="b">Goal</param>
        public void Search(Point a, Point b)
        {         
            for (int s = 0; s < 2; s++)
            {
                //For now this algorithm will only work with the shortest path
                bool usePhysicsPF = s == 0 ? true : false;
                PathFinder pathFinder = new PathFinder(worldData, usePhysicsPF);
                Point[] shortestPath = pathFinder.Solve(a, b);

                if (shortestPath == null)
                {
                    continue;
                }

                currentSearch = s;
                currentPathNodesComplete = 0;

                long estimatatedTime = 250 * shortestPath.Length;
                if (SearchPlane(a, b, estimatatedTime, usePhysicsPF))
                {
                    if (pCallback != null) pCallback(100);
                    if (tCallback != null) tCallback(true);
                    return;
                }

                //Start the workback algorithm
                if (WorkbackSearch(shortestPath, shortestPath.Length - 2, shortestPath.Length - 1, false, usePhysicsPF))
                {
                    if (pCallback != null) pCallback(100);
                    if (tCallback != null) tCallback(true);
                    return;
                }
            }

            if (pCallback != null) pCallback(100);
            if (tCallback != null) tCallback(false);
            return;
        }

        /// <summary>
        /// Recursive workback search
        /// </summary>
        private bool WorkbackSearch(Point[] guide, int a, int b, bool backtracking, bool usePhysicsPF)
        {
            if (!backtracking)
            {
                if (pCallback != null) pCallback(100 * ((currentSearch * 0.5) + (0.5 * (currentPathNodesComplete / (double)guide.Length))));
            }

            if (b == 0)
            {
                return true;
            }
            else if (a < 0)
            {
                return false;
            }

            Point start = guide[a];
            Point goal = guide[b];

            int length = b - a;
            long toMs = (backtracking) ? 500 : 1000;
            if (SearchPlane(start, goal, toMs, usePhysicsPF))
            {
                if (!backtracking)
                {
                    currentPathNodesComplete++;
                    return WorkbackSearch(guide, a - 1, a, backtracking, usePhysicsPF);
                }
                else
                {
                    //return a pruned true
                    return true;
                }              
            }
            else
            {
                if (!backtracking)
                {
                    //If in depth check all the nodes before the last successful node
                    for (int tb = b + 1; tb < guide.Length; tb++)
                    {
                        if (WorkbackSearch(guide, a, tb, true, usePhysicsPF))
                        {
                            currentPathNodesComplete++;
                            return WorkbackSearch(guide, a - 1, a, backtracking, usePhysicsPF);
                        }
                    }
                }
                else
                {
                    //return a pruned false
                    return false;
                }

                currentPathNodesComplete++;
                return WorkbackSearch(guide, a - 1, b, backtracking, usePhysicsPF);
            }
        }

        /// <summary>
        /// Searchs the plane, efficiency increases the closer of points
        /// </summary>
        private bool SearchPlane(Point a, Point b, long timeout, bool usePhysicsPF)
        {
            PlayerState sState = new PlayerState(a);

            if (a.Equals(b))
            {
                return true;
            }

            heuristic.A = a;
            heuristic.B = b;
            if (!heuristic.Calculate(worldData, usePhysicsPF))
            {
                return false;
            }

            PlayerNode start = new PlayerNode(Input.Nothing, sState);
            start.F = heuristic.EstimateCost(start);

            hashTable.Release();
            OpenSet<PlayerNode> openSet = new OpenSet<PlayerNode>(n => n.F);
            openSet.Add(start);

            bool found = false;
            Stopwatch sw = Stopwatch.StartNew();
            while (openSet.Count > 0)
            {
                PlayerNode current = openSet.Dequeue();
                if (sw.ElapsedMilliseconds > timeout)
                {
                    break;
                }

                int inc = 0;
                for (int t = 0; t <= 100; t += inc)
                {
                    List<PlayerNode> branches = world.GetNodes(player, current.State, t);

                    for (int i = 0; i < branches.Count; i++)
                    {
                        if (hashTable.Lookup(branches[i].State))
                        {
                            continue;
                        }

                        if (branches[i].IsGoal(b))
                        {
                            found = true;
                        }

                        hashTable.Add(branches[i].State);
                        branches[i].F = heuristic.EstimateCost(branches[i]);
                        openSet.Add(branches[i]);
                    }

                    inc += 5;
                }

                if (found)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Disposes hash resources
        /// </summary>
        public void Dispose()
        {
            hashTable.Dispose();
        }

        /// <summary>
        /// Creates a new processor
        /// </summary>
        /// <param name="worldData">WorldData</param>
        public WorkbackProcessor(WorldData worldData, ProgressCallback pCallback, TestCompleteCallback tCallback)
        {
            world = new PhysicsWorld(worldData);
            player = new PhysicsPlayer(world);
            heuristic = new Heuristic();        

            hashTable = new HashTable();
            this.worldData = worldData;
            this.pCallback = pCallback;
            this.tCallback = tCallback;
        }
    }
}
