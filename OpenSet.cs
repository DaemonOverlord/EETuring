using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EETuring
{
    public class OpenSet<T>
    {
        private List<T> list;
        private Func<T, double> selector;

        public int Count { get { return list.Count; } }

        /// <summary>
        /// Add a node to the set
        /// </summary>
        public void Add(T node)
        {
            list.Add(node);
        }

        /// <summary>
        /// Sorts the nodes then removes and returns the best node
        /// </summary>
        public T Dequeue()
        {
            QuickSort(list, 0, list.Count);

            T top = list[0];
            list.RemoveAt(0);
            return top;
        }

        /// <summary>
        /// Quicksort partition of a list pivoting at the left and right indicies
        /// </summary>
        private int QuicksortPartition(List<T> list, int left, int right)
        {
            int start = left;
            T pivot = list[start];
            left++;
            right--;

            while (true)
            {
                while (left <= right && selector(list[left]) <= selector(pivot))
                    left++;

                while (left <= right && selector(list[right]) > selector(pivot))
                    right--;

                if (left > right)
                {
                    list[start] = list[left - 1];
                    list[left - 1] = pivot;

                    return left;
                }


                T temp = list[left];
                list[left] = list[right];
                list[right] = temp;

            }
        }

        /// <summary>
        /// Recursive quicksort algorithm
        /// </summary>
        private void QuickSort(List<T> list, int left, int right)
        {
            if (list == null || list.Count <= 1)
                return;

            if (left < right)
            {
                int pivotIdx = QuicksortPartition(list, left, right);
                QuickSort(list, left, pivotIdx - 1);
                QuickSort(list, pivotIdx, right);
            }
        }

        public OpenSet(Func<T, double> selector)
        {
            list = new List<T>();
            this.selector = selector;
        }
    }
}
