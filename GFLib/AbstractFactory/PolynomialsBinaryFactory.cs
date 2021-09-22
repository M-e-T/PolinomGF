using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GFLib.AbstractFactory
{
    public abstract class PolynomialsBinaryFactory : PolynomialsFactory
    {
        private long[] _dividers = new long[] { };
        protected long[] Dividers
        {
            get
            {
                return _dividers;
            }
            set
            {
                _dividers = value;
            }
        }
        public PolynomialsBinaryFactory(int toBase) : base(toBase)
        {
        }
        private protected abstract bool IsPolinom(dynamic testPolinom);
        private protected int BitInt(long i)
        {
            int count = 0;
            while (i > 0)
            {
                if ((i & 1) == 1)
                    count++;
                i >>= 1;
            }
            return count;
        }
        private protected int BitInt(BigInteger i)
        {
            int count = 0;
            while (i > 0)
            {
                if ((i & 1) == 1)
                    count++;
                i >>= 1;
            }
            return count;
        }
        private protected long Mod(long a, long b, long c)
        {
            while (a > c)
            {
                a ^= -(a & 1) & b;
                a >>= 1;
            }
            return a;
        }
        private protected BigInteger Mod(BigInteger a, BigInteger b, long c)
        {
            while (a > c)
            {
                a ^= -(a & 1) & b;
                a >>= 1;
            }
            return a;
        }
        private protected int BitOll(BigInteger i)
        {
            int count = 0;
            while (i != 0)
            {
                count++;
                i >>= 1;
            }
            return count - 1;
        }
    }
}
