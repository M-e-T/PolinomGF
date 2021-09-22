using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GFLib.Interface;
namespace GFLib.AbstractFactory
{
    public abstract class PolynomialGFFactory : PolynomialsFactory
    {
        private List<int[][]> _dividers = new List<int[][]>();
        protected List<int[][]> Dividers
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
        private protected abstract bool IsPolinom(int[] testPolinom);
        public PolynomialGFFactory(int toBase) : base(toBase)
        {
            Dividers.Add(PolynomialsFirstPower[toBase]);
        }
    }
}
