using EETuring.Physics;
using PlayerIOClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EETuring
{
    class Program
    {
        public static WorldData GetWorldData(string worldid)
        {
            Client guestClient = PlayerIO.QuickConnect.SimpleConnect("everybody-edits-su9rn58o40itdbnw69plyw", "guest", "guest", null);
            DatabaseObject worldObject = guestClient.BigDB.Load("worlds", worldid);
            object width, height;
            worldObject.TryGetValue("width", out width);
            worldObject.TryGetValue("height", out height);

            int[][] foreGroundTiles = new int[(int)width][];
            int[][][] tileData = new int[(int)width][][];

            DatabaseArray databaseArray = worldObject.Contains("worlddata") ? ((DatabaseArray)worldObject["worlddata"]) : null;

            if (databaseArray == null)
            {
                return new WorldData();
            }

            using (IEnumerator<object> enumerator = ((IEnumerable<object>)databaseArray).GetEnumerator())
            {
                DatabaseObject last = null;
                while (enumerator.MoveNext())
                {
                    DatabaseObject blockData = (DatabaseObject)enumerator.Current;

                    int rotation = blockData.Contains("rotation") ? Convert.ToInt32(blockData["rotation"]) : 0;
                    byte[] bytes = blockData.GetBytes("x");
                    byte[] bytes2 = blockData.GetBytes("y");
                    int id = Convert.ToInt32(blockData["type"]);

                    for (int i = 0; i < bytes.Length; i += 2)
                    {
                        int x = (int)bytes[i] << 8 | (int)bytes[i + 1];
                        int y = (int)bytes2[i] << 8 | (int)bytes2[i + 1];
                        if (blockData.Contains("layer"))
                        {
                            int z = blockData.GetInt("layer");
                            switch (id)
                            {
                                case ItemId.BlueCoindoor:
                                case ItemId.Coindoor:
                                    int doorgoal = Convert.ToInt32(blockData["goal"]);

                                    if (tileData[x] == null)
                                    {
                                        tileData[x] = new int[(int)height][];
                                        tileData[x][y] = new int[1];
                                    }
                                    else if (tileData[x][y] == null)
                                    {
                                        tileData[x][y] = new int[1];
                                    }

                                    tileData[x][y][0] = doorgoal;
                                    break;
                                case ItemId.BlueCoingate:
                                case ItemId.Coingate:
                                    int gategoal = Convert.ToInt32(blockData["goal"]);

                                    if (tileData[x] == null)
                                    {
                                        tileData[x] = new int[(int)height][];
                                        tileData[x][y] = new int[1];
                                    }
                                    else if (tileData[x][y] == null)
                                    {
                                        tileData[x][y] = new int[1];
                                    }

                                    tileData[x][y][0] = gategoal;
                                    break;
                                case ItemId.Portal:
                                case ItemId.PortalInvisible:
                                    int portalid = Convert.ToInt32(blockData["id"]);
                                    int portaltarget = Convert.ToInt32(blockData["target"]);

                                    if (tileData[x] == null)
                                    {
                                        tileData[x] = new int[(int)height][];
                                        tileData[x][y] = new int[2];
                                    }
                                    else if (tileData[x][y] == null)
                                    {
                                        tileData[x][y] = new int[2];
                                    }

                                    tileData[x][y][0] = portalid;
                                    tileData[x][y][1] = portaltarget;
                                    break;
                            }

                            if (z == 0)
                            {
                                if (foreGroundTiles[x] == null)
                                {
                                    foreGroundTiles[x] = new int[(int)height];
                                }

                                foreGroundTiles[x][y] = id;
                            }
                        }
                    }
                    last = blockData;
                }
            }

            guestClient.Logout();
            return new WorldData(foreGroundTiles, tileData, (int)width, (int)height);
        }

        public static void Print2D(int[,] array)
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    string num = array[j, i].ToString();
                    if (num.Length < 2)
                    {
                        num = "0" + num;
                    }

                    Console.Write(string.Format("{0} ", num));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }

        public static void Print2D(double[,] array)
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    string num = Math.Round(array[j, i], 1).ToString().Replace(".", "");
                    if (num.Length < 2)
                    {
                        num = "0" + num;
                    }

                    Console.Write(string.Format("{0} ", num));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }

        public static void Print2D(int[][] array)
        {
            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    string num = array[j][i].ToString();
                    if (num.Length < 2)
                    {
                        num = "0" + num;
                    }

                    Console.Write(string.Format("{0} ", num));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }

        public static void TestPF()
        {
            WorldData data = GetWorldData("PW1W4Ubqjwa0I");

            PathFinder pf = new PathFinder(data, true);
            EETuring.Physics.Point[] pts = pf.Solve(new EETuring.Physics.Point(1, 23), new EETuring.Physics.Point(23, 23));

            WebRequest req = WebRequest.Create("http://api.everybodyedits.info/MapImageGenerator?id=PW1W4Ubqjwa0I");

            System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(req.GetResponse().GetResponseStream());
            System.Drawing.Graphics gfx = System.Drawing.Graphics.FromImage(bmp);
            if (pts != null)
            {
                for (int i = 0; i < pts.Length - 1; i++)
                {
                    gfx.DrawLine(System.Drawing.Pens.Red, new System.Drawing.PointF(pts[i].x * 16 + 8, pts[i].y * 16 + 8), new System.Drawing.PointF(pts[i + 1].x * 16 + 8, pts[i + 1].y * 16 + 8));
                }
            }



            gfx.Dispose();
            bmp.Save(@"C:\Users\Austin Green\Desktop\path.png");
            bmp.Dispose();
        }

        public static void Main(string[] args)
        {
            /*
             * Inspiring topic: http://eeforumify.com/viewtopic.php?id=32553
             * 
             * Create a bot such that it can test whether a level is possible to complete from A to B
             * 
             * Resources needed:
             * -EE Physics engine, we will need to modify heavily to get nodes and set the engine variables per node
             * -PlayerIOClient, for physics engine compatibility
             * -Maybe a EE World SDK For blocks (Skylight), but probably not, since the physics engine will have this
             * 
             * Starting pseudocode:
             * -Tackle the problem of infinite paths through iterative deepening
             * -Load World W
             * -Set the player at position A
             * -Create algorithm to get nodes for possible physic keys
             * -Nodes branch off every tick
             * -Use A* as a heuristic towards the goal B
             * 
             * Ideas for improvements
             * -Use hashing to avoid the player from reaching a node twice
             * 
             * Limitations
             * -Avoid the ideas of keys/timedoor at the moment (return no right away if keys in path)
             *      -keys cause too many problems, time algorithms, and other players messing with the keys
             * 
             * Expectations
             * -Algorithm should be able to return a boolean yes or no if possible
             */

            TestPF();

            WorldData data = GetWorldData("PW1W4Ubqjwa0I");


            WorkbackA searcher = new WorkbackA(data);
            bool possible = searcher.Search(new Point(1, 23), new Point(23, 23));

            //APhysics searcher2 = new APhysics(data);
            //bool possible = searcher2.Search(new Point(1, 16), new Point(18, 16));

            string possibleStr = (possible) ? "Possible." : "Impossible.";
            Console.WriteLine(possibleStr);
            Console.ReadKey();
        }
    }
}
