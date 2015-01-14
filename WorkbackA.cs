using EETuring.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace EETuring
{
    public class WorkbackA : IDisposable
    {
        private PhysicsPlayer player;
        private PhysicsWorld world;
        private WorldData worldData;

        private Heuristic heuristic;
        private HashTable hashTable;

        public bool Goal(PlayerNode u, Point t)
        {
            return u.IsGoal(t);
        }

        public bool Search(Point a, Point b)
        {         
            for (int s = 0; s < 2; s++)
            {
                Console.Clear();
                Console.WriteLine("Calculating new path.");

                //For now this algorithm will only work with the shortest path
                bool usePhysicsPF = s == 0 ? false : true;
                PathFinder pathFinder = new PathFinder(worldData, usePhysicsPF);
                Point[] shortestPath = pathFinder.Solve(a, b);
                if (shortestPath == null)
                {
                    return false;
                }

                Console.Write("Doing quick search...");
                long estimatatedTime = 1000 * shortestPath.Length;
                if (SearchPlane(a, b, estimatatedTime, usePhysicsPF))
                {
                    return true;
                }

                Console.WriteLine("Failed, looking in depth.");



                //Start the workback algorithm
                if (WorkbackSearch(shortestPath, shortestPath.Length - 2, shortestPath.Length - 1, false, usePhysicsPF))
                {
                    return true;
                }
            }

            return false;
        }

        private bool WorkbackSearch(Point[] guide, int a, int b, bool backtracking, bool usePhysicsPF)
        {
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
                    Console.WriteLine("Mapped ({0}) -> ({1})", guide[a], guide[b]);
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
                            Console.WriteLine("Mapped ({0}) -> ({1}) -> ({2})", guide[a], guide[b], guide[tb]);
                            return WorkbackSearch(guide, a - 1, a, backtracking, usePhysicsPF);
                        }
                    }
                }

                if (!backtracking)
                {
                    Console.WriteLine("Failed to map ({0}) -> ({1})", guide[a], guide[b]);
                }
                else
                {
                    Console.WriteLine("\tFailed to map ({0}) -> ({1})", guide[a], guide[b]);
                    return false;
                }

                return WorkbackSearch(guide, a - 1, b, backtracking, usePhysicsPF);
            }
        }

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

            OpenSet openSet = new OpenSet(hashTable);
            openSet.Add(start);

            Stopwatch sw = Stopwatch.StartNew();
            while (openSet.Count > 0)
            {
                PlayerNode current = openSet.Dequeue();
                if (sw.ElapsedMilliseconds > timeout)
                {
                    break;
                }

                int inc = 5;
                for (int t = 0; t <= 100; t += inc)
                {
                    List<PlayerNode> branches = world.GetNodes(player, current.State, t);
                    if (sw.ElapsedMilliseconds > timeout)
                    {
                        break;
                    }

                    for (int i = 0; i < branches.Count; i++)
                    {
                        if (hashTable.Lookup(branches[i].State))
                        {
                            continue;
                        }

                        if (branches[i].IsGoal(b))
                        {
                            return true;
                        }


                        hashTable.Add(branches[i].State);
                        branches[i].F = heuristic.EstimateCost(branches[i]);
                        openSet.Add(branches[i]);
                    }

                    inc += 5;
                }
            }

            return false;
        }

        public void Dispose()
        {
            hashTable.Dispose();
        }

        public WorkbackA(WorldData worldData)
        {
            world = new PhysicsWorld(worldData);
            player = new PhysicsPlayer(world);
            heuristic = new Heuristic();        

            hashTable = new HashTable();
            this.worldData = worldData;
        }
    }
}
