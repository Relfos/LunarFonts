using System;
using System.Collections.Generic;

namespace LunarLabs.Fonts
{
    internal struct Edge
    {
        public float x0;
        public float y0;
        public float x1;
        public float y1;
        public bool invert;
    }

    internal class EdgeList
    {
        private List<Edge> edges = new List<Edge>(16);

        public int Count => edges.Count;

        public int Add(Edge item)
        {
            var result = edges.Count;
            edges.Add(item);
            return result;
        }


        public Edge Get(int index)
        {
            if (index < 0 || index >= edges.Count)
            {
                throw new Exception("Invalid edge index");
            }

            return edges[index];
        }

        private int EdgeCompare(Edge pa, Edge pb)
        {
            if (pa.y0 < pb.y0)
                return -1;

            if (pa.y0 > pb.y0)
                return 1;

            return 0;
        }

        private void QuickSort(int left, int right)
        {
            int i, j;
            do
            {
                i = left;
                j = right;
                var pivot = edges[(left + right) / 2];
                do
                {

                    while (EdgeCompare(edges[i], pivot) < 0)
                    {
                        i++;
                    }

                    while (EdgeCompare(edges[j], pivot) > 0)
                    {
                        j--;
                    }

                    if (i <= j)
                    {
                        var temp = edges[i];
                        edges[i] = edges[j];
                        edges[j] = temp;
                        i++;
                        j--;
                    }

                } while (i <= j);

                if (left < j)
                {
                    QuickSort(left, j);

                }

                left = i;
            } while (i < right);
        }

        public void Sort()
        {
            if (edges.Count > 1)
            {
                QuickSort(0, edges.Count - 1);
            }
        }

        public void Fix(int index, float Y0)
        {
            var temp = edges[index];
            temp.y0 = Y0;
            edges[index] = temp;
        }
    }
}