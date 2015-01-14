using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace EETuring.Physics
{
    public class PlayerState
    {
        public int current;
        public double speedX;
        public double speedY;
        public double modifierX;
        public double modifierY;
        public int gravity;
        public bool onDot;
        public double mox, moy;
        public int morx, mory;
        public int pastx, pasty;
        public int overlapy;
        public double mx, my;
        public bool isInvulnerable;
        public bool donex, doney;
        public int[] queue;
        public int delayed;
        public Point lastPortal;
        public List<Point> gotCoins;
        public List<Point> gotBlueCoins;

        public bool Purple { get; set; }
        public bool InGodMode { get; set; } //Also includes moderatormode
        public bool IsDead { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int BlockX { get { return (int)(X / 16); } }
        public int BlockY { get { return (int)(Y / 16); } }
        public int Horizontal { get; set; }
        public int Vertical { get; set; }
        public int LastCheckpointX { get; set; }
        public int LastCheckpointY { get; set; }
        public int Coins { get; set; }
        public int BlueCoins { get { return gotBlueCoins.Count; } }
        public bool IsClubMember { get; set; }

        public double SpeedX { get { return speedX * PhysicsConfig.VariableMultiplier; } internal set { speedX = value / PhysicsConfig.VariableMultiplier; } }
        public double SpeedY { get { return speedY * PhysicsConfig.VariableMultiplier; } internal set { speedY = value / PhysicsConfig.VariableMultiplier; } }
        public double ModifierX { get { return modifierX * PhysicsConfig.VariableMultiplier; } internal set { modifierX = value / PhysicsConfig.VariableMultiplier; } }
        public double ModifierY { get { return modifierY * PhysicsConfig.VariableMultiplier; } internal set { modifierY = value / PhysicsConfig.VariableMultiplier; } }

        private byte[] GetBytes(double d)
        {
            return BitConverter.GetBytes(d);
        }

        private byte[] GetBytes(int i)
        {
            return BitConverter.GetBytes(i);
        }

        private byte[] GetBytes(bool b)
        {
            return BitConverter.GetBytes(b);
        }

        private byte[] AddBytes(byte[] src, byte[] add)
        {
            byte[] dest = new byte[src.Length + add.Length];
            Buffer.BlockCopy(src, 0, dest, 0, src.Length);
            Buffer.BlockCopy(add, 0, dest, src.Length, add.Length);
            return dest;
        }

        public long GetHash(SHA1Managed sha1)
        {
            byte[] bytes = new byte[0];
            bytes = AddBytes(bytes, GetBytes(current));
            bytes = AddBytes(bytes, GetBytes(speedX));
            bytes = AddBytes(bytes, GetBytes(speedX));
            bytes = AddBytes(bytes, GetBytes(speedY));
            bytes = AddBytes(bytes, GetBytes(modifierX));
            bytes = AddBytes(bytes, GetBytes(modifierY));
            bytes = AddBytes(bytes, GetBytes(gravity));
            bytes = AddBytes(bytes, GetBytes(onDot));
            bytes = AddBytes(bytes, GetBytes(morx));
            bytes = AddBytes(bytes, GetBytes(mory));
            bytes = AddBytes(bytes, GetBytes(pastx));
            bytes = AddBytes(bytes, GetBytes(pasty));
            bytes = AddBytes(bytes, GetBytes(overlapy));
            bytes = AddBytes(bytes, GetBytes(mx));
            bytes = AddBytes(bytes, GetBytes(my));
            bytes = AddBytes(bytes, GetBytes(isInvulnerable));
            bytes = AddBytes(bytes, GetBytes(donex));
            bytes = AddBytes(bytes, GetBytes(doney));
            bytes = AddBytes(bytes, GetBytes(queue[0]));
            bytes = AddBytes(bytes, GetBytes(queue[1]));
            bytes = AddBytes(bytes, GetBytes(delayed));
            bytes = AddBytes(bytes, GetBytes(Purple));
            bytes = AddBytes(bytes, GetBytes(InGodMode));
            bytes = AddBytes(bytes, GetBytes(IsDead));
            bytes = AddBytes(bytes, GetBytes(X));
            bytes = AddBytes(bytes, GetBytes(Y));
            bytes = AddBytes(bytes, GetBytes(Horizontal));
            bytes = AddBytes(bytes, GetBytes(Vertical));
            bytes = AddBytes(bytes, GetBytes(LastCheckpointX));
            bytes = AddBytes(bytes, GetBytes(LastCheckpointY));
            bytes = AddBytes(bytes, GetBytes(Coins));
            bytes = AddBytes(bytes, GetBytes(BlueCoins));
            bytes = AddBytes(bytes, GetBytes(IsClubMember));
            return BitConverter.ToInt64(sha1.ComputeHash(bytes), 0);
        }

        public override string ToString()
        {
            return string.Format("x: {0}, y: {1}", BlockX, BlockY);
        }

        public PlayerState()
        {
            this.speedX = 0;
            this.speedY = 0;
            this.modifierX = 0;
            this.modifierY = 0;
            this.isInvulnerable = false;
            this.queue = new int[PhysicsConfig.QueueLength];
            this.gotCoins = new List<Point>();
            this.gotBlueCoins = new List<Point>();
            this.gravity = (int)PhysicsConfig.Gravity;
        }

        public PlayerState(Point p) : this()
        {
            X = p.x * 16;
            Y = p.y * 16;
        }

        public PlayerState(PlayerState state)
        {
            this.queue = new int[PhysicsConfig.QueueLength];
            this.gotCoins = new List<Point>();
            this.gotBlueCoins = new List<Point>();
            this.gravity = (int)PhysicsConfig.Gravity;

            this.current = state.current;
            this.speedX = state.speedX;
            this.speedY = state.speedY;
            this.modifierX = state.modifierX;
            this.modifierY = state.modifierY;
            this.gravity = state.gravity;
            this.onDot = state.onDot;
            this.mox = state.mox;
            this.moy = state.moy;
            this.morx = state.morx;
            this.mory = state.mory;
            this.pastx = state.pastx;
            this.pasty = state.pasty;
            this.overlapy = state.overlapy;
            this.mx = state.mx;
            this.my = state.my;
            this.isInvulnerable = state.isInvulnerable;
            this.donex = state.donex;
            this.doney = state.doney;

            int queueLen = Buffer.ByteLength(state.queue);
            Buffer.BlockCopy(state.queue, 0, this.queue, 0, queueLen);

            this.delayed = state.delayed;
            if (state.lastPortal != null)
            {
                this.lastPortal = new Point(state.lastPortal.x, state.lastPortal.y);
            }

            for (int i = 0; i < state.gotCoins.Count; i++)
            {
                gotCoins.Add(new Point(state.gotCoins[i].x, state.gotCoins[i].y));
            }

            for (int j = 0; j < state.gotBlueCoins.Count; j++)
            {
                gotBlueCoins.Add(new Point(state.gotBlueCoins[j].x, state.gotBlueCoins[j].y));
            }

            this.Purple = state.Purple;
            this.InGodMode = state.InGodMode;
            this.IsDead = state.IsDead;
            this.X = state.X;
            this.Y = state.Y;
            this.Horizontal = state.Horizontal;
            this.Vertical = state.Vertical;
            this.LastCheckpointX = state.LastCheckpointX;
            this.LastCheckpointY = state.LastCheckpointY;
            this.Coins = state.Coins;
            this.IsClubMember = state.IsClubMember;
        }
    }
}
