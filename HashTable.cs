using EETuring.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EETuring
{
    public class HashTable : IDisposable
    {
        private SHA1Managed sha1;
        private Dictionary<long, PlayerState> hashTable;

        public void Add(PlayerState state)
        {
            long hash = state.GetHash(sha1);
            if (hashTable.ContainsKey(hash))
            {
                throw new InvalidOperationException();
            }

            hashTable.Add(hash, state);
        }

        public void Release()
        {
            hashTable.Clear();
        }

        public long GetHash(PlayerNode node)
        {
            return node.State.GetHash(sha1);
        }

        public bool Lookup(PlayerState state)
        {
            long hash = state.GetHash(sha1);
            return hashTable.ContainsKey(hash);
        }

        public void Dispose()
        {
            sha1.Dispose();
        }

        public HashTable()
        {
            sha1 = new SHA1Managed();
            hashTable = new Dictionary<long, PlayerState>();
        }
    }
}
