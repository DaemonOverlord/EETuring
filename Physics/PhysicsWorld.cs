using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using PlayerIOClient;

namespace EETuring.Physics
{
    public class PhysicsWorld
    {
        public const int Size = 16;


        private readonly Input[] combinations = new Input[]
        {
            Input.Nothing,

            Input.HoldLeft, 
            Input.HoldLeft | Input.HoldUp,
            Input.HoldLeft | Input.HoldDown,
            Input.HoldLeft | Input.HoldSpace,

            Input.HoldRight,
            Input.HoldRight | Input.HoldDown,
            Input.HoldRight | Input.HoldUp,
            Input.HoldRight | Input.HoldSpace,

            Input.HoldDown,
            Input.HoldDown | Input.HoldSpace,

            Input.HoldUp,
            Input.HoldUp | Input.HoldSpace,

            Input.HoldSpace,
            Input.HoldSpace | Input.HoldLeft | Input.HoldUp,
            Input.HoldSpace | Input.HoldLeft | Input.HoldDown,
            Input.HoldSpace | Input.HoldRight | Input.HoldUp,
            Input.HoldSpace | Input.HoldRight | Input.HoldDown

        };
                              
        private int[][] foregroundTiles;
        private int[][][] tileData;

        public double WorldGravity = 1;
        public int WorldWidth { get; private set; }
        public int WorldHeight { get; private set; }

        public Point GetPortalById(int id)
        {
            for (int i = 0; i < foregroundTiles.Length; i++)
            {
                for (int ii = 0; ii < foregroundTiles[i].Length; ii++)
                {
                    if (foregroundTiles[i][ii] == 242 || foregroundTiles[i][ii] == 381)
                    {
                        if (tileData[i][ii][1] == id)
                        {
                            return new Point(i, ii);
                        }
                    }
                }
            }
            return null;
        }

        public int GetNodeCount(PlayerState state)
        {
            return state.onDot ? combinations.Length - 1 : combinations.Length;
        }

        public PlayerNode GetNode(PhysicsPlayer p, PlayerState state, Input input, int steps)
        {
            if (state.onDot && (input & Input.HoldSpace) != 0)
            {
                return null;
            }

            PlayerState newState = new PlayerState(state);

            newState.Horizontal = ((input & Input.HoldLeft) != 0 ? -1 : 0) + ((input & Input.HoldRight) != 0 ? 1 : 0);
            newState.Vertical = ((input & Input.HoldUp) != 0 ? -1 : 0) + ((input & Input.HoldDown) != 0 ? 1 : 0);

            return new PlayerNode(input, p.Tick(newState, input, steps));
        }

        public List<PlayerNode> GetNodes(PhysicsPlayer p, PlayerState state, int steps)
        {
            List<PlayerNode> nodes = new List<PlayerNode>();
            for (int i = 0; i < combinations.Length; i++)
            {
                PlayerNode n = GetNode(p, state, combinations[i], steps);
                if (n != null)
                {
                    nodes.Add(n);
                }
            }

            return nodes;
        }
        public List<PlayerNode> GetNodes(PhysicsPlayer p, Point pt, int steps)
        {
            return GetNodes(p, new PlayerState(pt), steps);
        }

        public int GetBlock(int xx, int yy)
        {
            if (xx < 0 || xx >= foregroundTiles.Length || yy < 0 || yy >= foregroundTiles[0].Length)
            {
                return 0;
            }
            return foregroundTiles[xx][yy];
        }

        public int[] GetBlockData(int xx, int yy)
        {
            if (xx < 0 || xx >= foregroundTiles.Length || yy < 0 || yy >= foregroundTiles[0].Length)
            {
                return null;
            }
            return tileData[xx][yy];
        }      

        public bool Overlaps(PlayerState state)
        {
            if ((state.X < 0 || state.Y < 0) || ((state.X > (WorldWidth * Size) - Size) || (state.Y > (WorldHeight * Size) - Size)))
            {
                return true;
            }
            if (state.InGodMode)
            {
                return false;
            }
            int tileId;
            var firstX = ((int)state.X >> 4);
            var firstY = ((int)state.Y >> 4);
            double lastX = ((state.X + PhysicsPlayer.Height) / Size);
            double lastY = ((state.Y + PhysicsPlayer.Width) / Size);
            bool _local7 = false;

            int x;
            int y = firstY;
            while (y < lastY)
            {
                x = firstX;
                for (; x < lastX; x++)
                {
                    tileId = foregroundTiles[x][y];
                    if (ItemId.IsSolid(tileId))
                    {
                        switch (tileId)
                        {
                            case 23:
                                break;
                            case 24:
                                break;
                            case 25:
                                break;
                            case 26:
                                break;
                            case 27:
                                break;
                            case 28:
                                break;
                            case 156:
                                break;
                            case 157:
                                break;
                            case ItemId.DoorPurple:
                                if (state.Purple)
                                {
                                    continue;
                                }
                                break;
                            case ItemId.GatePurple:
                                if (!state.Purple)
                                {
                                    continue;
                                }
                                break;
                            case ItemId.DoorClub:
                                if (state.IsClubMember)
                                {
                                    continue;
                                }
                                break;
                            case ItemId.GateClub:
                                if (!state.IsClubMember)
                                {
                                    continue;
                                }
                                break;
                            case ItemId.Coindoor:
                            case ItemId.BlueCoindoor:
                                if (tileData[x][y][0] <= state.Coins)
                                {
                                    continue;
                                }
                                break;
                            case ItemId.Coingate:
                            case ItemId.BlueCoingate:
                                if (tileData[x][y][0] > state.Coins)
                                {
                                    continue;
                                }
                                break;
                            case ItemId.ZombieGate:
                                break;
                            case ItemId.ZombieDoor:
                                continue;
                            case 61:
                            case 62:
                            case 63:
                            case 64:
                            case 89:
                            case 90:
                            case 91:
                            case 96:
                            case 97:
                            case 122:
                            case 123:
                            case 124:
                            case 125:
                            case 126:
                            case 127:
                            case 146:
                            case 154:
                            case 158:
                            case 194:
                            case 211:
                                if (state.speedY < 0 || y <= state.overlapy)
                                {
                                    if (y != firstY || state.overlapy == -1)
                                    {
                                        state.overlapy = y;
                                    }
                                    _local7 = true;
                                    continue;
                                }
                                break;
                            case 83:
                            case 77:
                                continue;
                        }
                        return true;
                    }
                }
                y++;
            }
            if (!_local7)
            {
                state.overlapy = -1;
            }
            return false;
        }

        public void CreateWorld(WorldData worldData)
        {
            WorldWidth = worldData.Width;
            WorldHeight = worldData.Height;

            this.foregroundTiles = worldData.ForeGroundTiles;
            this.tileData = worldData.TileData;      
        }

        public PhysicsWorld(WorldData worldData)
        {
            CreateWorld(worldData);
        }
    }
}
