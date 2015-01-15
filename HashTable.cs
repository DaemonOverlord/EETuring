using EETuring.Physics;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace EETuring
{
    public class HashTable : IDisposable
    {
        private SHA1Managed sha1;
        private Dictionary<long, PlayerState> hashTable;

        /// <summary>
        /// Adds a playerstate to the hashtable
        /// </summary>
        /// <param name="state">PlayerState to add</param>
        public void Add(PlayerState state)
        {
            long hash = state.GetHash(sha1);
            if (hashTable.ContainsKey(hash))
            {
                throw new InvalidOperationException();
            }

            hashTable.Add(hash, state);
        }

        /// <summary>
        /// Clears the hashtable
        /// </summary>
        public void Release()
        {
            hashTable.Clear();
        }

        /// <summary>
        /// Calculates a hash using the SHA1 algorithm
        /// </summary>
        /// <param name="node">PlayerNode</param>
        public long GetHash(PlayerNode node)
        {
            return node.State.GetHash(sha1);
        }

        /// <summary>
        /// Checks if the state is within the hashtable
        /// </summary>
        /// <param name="state">PlayerState</param>
        public bool Lookup(PlayerState state)
        {
            long hash = state.GetHash(sha1);
            return hashTable.ContainsKey(hash);
        }

        /// <summary>
        /// Disposes the hashtables hashing algorithm
        /// </summary>
        public void Dispose()
        {
            sha1.Dispose();
        }

        /// <summary>
        /// Creates a new empty hashtable
        /// </summary>
        public HashTable()
        {
            sha1 = new SHA1Managed();
            hashTable = new Dictionary<long, PlayerState>();
        }
    }
}
