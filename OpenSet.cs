using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EETuring
{
    public class OpenSet
    {
        private HashTable hashTable;
        private List<PlayerNode> list;

        public int Count { get { return list.Count; } }

        public void Add(PlayerNode node)
        {
            list.Add(node);
        }

        public PlayerNode Dequeue()
        {
            QuickSort(list, 0, list.Count);

            PlayerNode top = list[0];
            list.RemoveAt(0);
            return top;
        }

        private int MyPartition(List<PlayerNode> list, int left, int right)
        {
            int start = left;
            PlayerNode pivot = list[start];
            left++;
            right--;

            while (true)
            {
                while (left <= right && list[left].F <= pivot.F)
                    left++;

                while (left <= right && list[right].F > pivot.F)
                    right--;

                if (left > right)
                {
                    list[start] = list[left - 1];
                    list[left - 1] = pivot;

                    return left;
                }


                PlayerNode temp = list[left];
                list[left] = list[right];
                list[right] = temp;

            }
        }

        private void QuickSort(List<PlayerNode> list, int left, int right)
        {
            if (list == null || list.Count <= 1)
                return;

            if (left < right)
            {
                int pivotIdx = MyPartition(list, left, right);
                QuickSort(list, left, pivotIdx - 1);
                QuickSort(list, pivotIdx, right);
            }
        }

        public OpenSet(HashTable hashTable)
        {
            this.hashTable = hashTable;
            hashTable.Release();
            list = new List<PlayerNode>();
        }
    }
}
