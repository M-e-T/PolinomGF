using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GFLib
{
    public class PolinomCompare : IComparer<int[]>, IEqualityComparer<int[]>
    {
        public int Compare(int[] p1, int[] p2)
        {
            for (int i = 0; i < p1.Length; i++)
            {
                if (p1[i] > p2[i])
                    return 1;
                if (p1[i] < p2[i])
                    return -1;
            }
            return 0;
        }
        public bool Equals(int[] x, int[] y)
        {
            return Enumerable.SequenceEqual(x, y);
        }
        public int GetHashCode(int[] obj)
        {
            int hc = obj.Length;
            for(int i = 0; i < obj.Length; i++)
            {
                hc = unchecked(hc * 314159 + obj[i]);
            }
            return hc;
        }
    }
}
