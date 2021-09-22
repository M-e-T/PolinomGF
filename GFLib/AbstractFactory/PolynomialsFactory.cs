using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using GFLib.FactoryImplementation;
using GFLib.Interface;

namespace GFLib.AbstractFactory
{
    public abstract class PolynomialsFactory : ModularArithmetic, IPolynomials
    {
        public event Action ProgressEvent;
        private int _power;
        public virtual int Power
        {
            get { return _power; }
            set
            {
                _power = value;
            }
        }
        public virtual int MaxPower { get; internal set; }
        public int ToBase { get; }
        public int CountGenerate { get; protected set; }
        public BigInteger CountTesting { get; private set; }
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }
        public bool IsCansel
        {
            get
            {
                return Token.IsCancellationRequested;
            }
        }
        private int _progress;
        public int Progress
        {
            get
            {
                return _progress;
            }
            protected set
            {
                _progress = value;
                ProgressEvent?.Invoke();
            }
        }
        private List<int[]> _item = new List<int[]>();
        public List<int[]> Items
        {
            get
            {
                return _item;
            }
            private protected set
            {
                _item = value;
            }
        }
        public int[] this[int index]
        {
            get
            {
                return Items[index];
            }
        }
        protected CancellationTokenSource Cts;
        protected CancellationToken Token;
        protected object balanceLock = new object();
        protected long Persent { get; set; }

        private static readonly char[] BaseChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C' };
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
        public PolynomialsFactory(int toBase) : base(toBase)
        {
            ToBase = toBase;
        }
        public void Cansel()
        {
            if (Cts != null)
                Cts.Cancel();
        }
        public void Sort()
        {
            Items.Sort(new PolinomCompare());
        }
        protected void Add(int[] polinom, BigInteger countTest)
        {
            lock (balanceLock)
            {
                CountTesting += countTest;
                if (Count < CountGenerate && Contains(polinom) == false && Dualpolynomial(polinom) == false)
                {
                    Items.Add(polinom);
                    if (Items.Count % Persent == 0)
                    {
                        Progress = (int)((double)(Items.Count) / (CountGenerate) * 100);
                    }
                }
                if (CountGenerate == Items.Count)
                    Cts.Cancel();
            }
        }
        protected BigInteger Min(int power)
        {
            return BigInteger.Pow(ToBase, power) + 1;
        }
        protected BigInteger Max(int power)
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
            for (int i = 0; i < Items.Count; i++)
            {
                if (Enumerable.SequenceEqual(Items[i], polinom) == true)
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
        protected long PersentProgress(BigInteger min, BigInteger max)
        {
            long Persent = (long)((max - min) / 100);
            if (Persent == 0)
                Persent++;
            return Persent;
        }
        public string ToString(int[] polinom)
        {
            string res = "";
            for (int i = 0; i < polinom.Length; i++)
            {
                res += BaseChars[polinom[i]];
            }
            return res;
        }
        private protected static string ToString(BigInteger value, int tobase)
        {
            if (tobase > BaseChars.Length)
                throw new ArgumentException(nameof(tobase) + $"Max Base {BaseChars.Length}");
            string result = string.Empty;
            int targetBase = tobase;

            do
            {
                result = BaseChars[(int)(value % targetBase)] + result;
                value = value / targetBase;
            }
            while (value > 0);

            return result;
        }
        public static PolynomialsFactory GetFactoryClassic(int toBase)
        {
            if (toBase == 2)
                return new PolynomialsBinaryClassic(toBase);
            return new PolynomialsGFClassic(toBase);
        }
        public static PolynomialsFactory GetFactoryLiner(int toBase)
        {
            if (toBase == 2)
                return new PolynomialsBinaryLiner(toBase);
            return new PolynomialsGFLiner(toBase);
        }
    }
}
