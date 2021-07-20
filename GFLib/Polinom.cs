using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using GFLib.Interface;

namespace GFLib
{
    public abstract class Polinom : ModularArithmetic, IPolinom
    {
        public event Action ProgressEvent;

        private int power;
        public int Power 
        {
            get { return power; } 
            internal set
            {
                if ((Math.Pow(ToBase, value) + 1) > long.MaxValue)
                    throw new ArgumentException(nameof(power), "Max power 39 ");
                if ((Math.Pow(ToBase, value) + 1) < 1)
                    throw new ArgumentException(nameof(power), "Min power 1");
                power = value;
            } 
        }
        public int ToBase { get; }
        public int Count
        {
            get
            {
                return Polynomials.Count;
            }
        }
        public int CountGenerate { get; protected set; }
        public BigInteger CountTesting { get; private set; }
        public bool IsCansel 
        {
            get
            {
                return token.IsCancellationRequested;
            } 
        } 
        private int progress;
        public int Progress
        {
            get
            {
                return progress;
            }
            protected set
            {
                progress = value;
                ProgressEvent?.Invoke();
            }
        }
        private List<int[]> polynomials = new List<int[]>();
        public List<int[]> Polynomials
        {
            get
            {
                return polynomials;
            }
            private protected set
            {
                polynomials = value;
            }
        }
        public int[] this[int index]
        {
            get
            {
                return Polynomials[index];
            }
        }
        private List<int[][]> dividers = new List<int[][]>();
        protected List<int[][]> Dividers
        {
            get
            {
                return dividers;
            }
            set
            {
                dividers = value;
            }
        }
        protected object balanceLock = new object();
        protected CancellationTokenSource cts;
        protected CancellationToken token;
        protected long Persent { get; set; }
        protected readonly Dictionary<int, int[][]> PolynomialsFirstPower = new Dictionary<int, int[][]>()
        {
            {2, new int[][] { new int[] { 1, 1 } } },
            {3, new int[][] { new int[] { 1, 1 }, new int[] {1, 2 } } } ,
            {5, new int[][] { new int[] { 1, 1 }, new int[] {1, 2 }, new int[] { 1, 3 }, new int[] { 1, 4 } } } ,
            {7, new int[][] { new int[] { 1, 1 }, new int[] {1, 2 }, new int[] { 1, 3 }, new int[] { 1, 4 }, new int[] { 1, 5 }, new int[] { 1, 6 } } } ,
            {11, new int[][] { new int[] { 1, 1 }, new int[] {1, 2 }, new int[] { 1, 3 }, new int[] { 1, 4 }, new int[] { 1, 5 }, new int[] { 1, 6 },
                new int[] { 1, 7 }, new int[] { 1, 8 },new int[] { 1, 9 }, new int[] { 1, 10 }    } } ,
            {13, new int[][] { new int[] { 1, 1 }, new int[] {1, 2 }, new int[] { 1, 3 }, new int[] { 1, 4 }, new int[] { 1, 5 }, new int[] { 1, 6 },
                new int[] { 1, 7 }, new int[] { 1, 8 },new int[] { 1, 9 }, new int[] { 1, 10 }, new int[] { 1, 11 }, new int[] { 1, 12 } } }
        };
        public abstract Task GenerateDetermAsync(int power);
        public abstract Task GenerateStohasAsync(int power, int countPolinom);
        private protected abstract bool IsPolinom(int[] testPolinom);
        public Polinom(int toBase) : base(toBase)
        {
            ToBase = toBase;
            Dividers.Add(PolynomialsFirstPower[toBase]);
        }
        public void Cansel()
        {
            if (cts != null)
                cts.Cancel();
        }
        public void Sort()
        {
            Polynomials.Sort(new PolinomCompare());
        }
        protected void Add(int[] polinom, BigInteger countTest)
        {
            lock (balanceLock)
            {
                CountTesting += countTest;
                if (Polynomials.Count < CountGenerate && Contains(polinom) == false && Dualpolynomial(polinom) == false)
                {
                    Polynomials.Add(polinom);
                    if (Polynomials.Count % Persent == 0)
                    {
                        Progress = (int)((double)(Polynomials.Count) / (CountGenerate) * 100);
                    }
                }
                if (CountGenerate == Polynomials.Count)
                    cts.Cancel();
            }
        }  
        protected long Min(int power)
        {
            return (long)Math.Pow(ToBase, power) + 1;
        }
        protected long Max(int power)
        {
            return Min(power) * 2 - ToBase + (ToBase - 2);
        }
        protected int Weight(int[] arr)
        {
            int res = 0;
            for (int i = 0; i < arr.Length; i++)
                res += arr[i];
            return res;
        }
        protected void Reverse(int[] polinom)
        {
            for (int i = 0; i < polinom.Length / 2; i++)
            {
                int tmp = polinom[i];
                polinom[i] = polinom[polinom.Length - i - 1];
                polinom[polinom.Length - i - 1] = tmp;
            }
        }
        protected bool Contains(int[] polinom)
        {
            for(int i = 0; i < Polynomials.Count; i++)
            {
                if (Enumerable.SequenceEqual(Polynomials[i], polinom) == true)
                    return true;
            }
            return false;
        }
        protected bool Dualpolynomial(int[] polinom)
        {
            Reverse(CopyArr(polinom));
            if (polinom[polinom.Length - 1] == ToBase - 1)
            {
                return Contains(XorEveryHalf(polinom, ToBase - 1));
            }
            return Contains(polinom);
        }
        protected long PersentProgress(long min, long max)
        {
            long Persent = (max - min) / 100;
            if (Persent == 0)
                Persent++;
            return Persent;
        }
    }
}
