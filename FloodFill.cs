using EETuring.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EETuring
{
    public class FloodFill
    {
        private WorldData data;

        private bool InBounds(Point p)
        {
            return p.x >= 0 && p.x < data.Width &&
                   p.y >= 0 && p.y < data.Height;
        }

        private void AddIfValid(Point p, FillNode last, List<FillNode> list)
        {
            if (InBounds(p))
            {
                list.Add(new FillNode(p, last.HitValue + 1));
            }
        }

        private List<FillNode> GetAdjacent4(FillNode last)
        {
            List<FillNode> a = new List<FillNode>();
            AddIfValid(new Point(last.P.x + 1, last.P.y), last, a);
            AddIfValid(new Point(last.P.x - 1, last.P.y), last, a);
            AddIfValid(new Point(last.P.x, last.P.y + 1), last, a);
            AddIfValid(new Point(last.P.x, last.P.y - 1), last, a);
            return a;
        }

        public int[,] CreateMap4(Point a)
        {
            int[,] nodemap = new int[data.Width, data.Height];
            if (ItemId.IsSolid(data.ForeGroundTiles[a.x][a.y]))
            {
                return nodemap;
            }

            Queue<FillNode> nodes = new Queue<FillNode>();
            nodes.Enqueue(new FillNode(a));
            nodemap[a.x, a.y] = 1;

            while (nodes.Count > 0)
            {
                FillNode cur = nodes.Dequeue();            

                List<FillNode> adj = GetAdjacent4(cur);
                for (int i = 0; i < adj.Count; i++)
                {
                    if (!ItemId.IsSolid(data.ForeGroundTiles[adj[i].P.x][adj[i].P.y]))
                    {
                        if (nodemap[adj[i].P.x, adj[i].P.y] == 0)
                        {
                            nodes.Enqueue(adj[i]);
                            nodemap[adj[i].P.x, adj[i].P.y] = cur.HitValue;
                        }
                    }
                }          
            }

            return nodemap;
        }

        public FloodFill(WorldData data)
        {
            this.data = data;
        }
    }
}
