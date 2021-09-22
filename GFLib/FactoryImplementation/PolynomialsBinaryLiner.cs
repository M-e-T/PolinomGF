using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using GFLib.AbstractFactory;

namespace GFLib.FactoryImplementation
{
    public class PolynomialsBinaryLiner : PolynomialsBinaryFactory
    {
        private PolynomialsBinaryClassic PolinomClassic;

        private int Bits;
        private long[] LengthDividers;
        private Dictionary<int, int[]> ExclusivePower = new Dictionary<int, int[]>()
        {
            { 12, new int[] { 2 } },
            { 18, new int[] { 3 } },
            { 20, new int[] { 2 } },
            { 24, new int[] { 2, 4 } },
            { 28, new int[] { 2 } },
            { 30, new int[] { 2, 3, 5 } },
            { 36, new int[] { 2, 3, 6 } },
            { 40, new int[] { 2, 4} },
            { 42, new int[] { 2, 3, 7} },
            { 44, new int[] { 2 } },
            { 45, new int[] { 3 } },
            { 48, new int[] { 2, 4, 8} },
            { 50, new int[] { 5 } },
            { 52, new int[] { 2 } },
            { 54, new int[] { 3, 9 } },
            { 56, new int[] { 2, 4 } },
            { 60, new int[] { 2, 3, 4, 5} },
        };
        public PolynomialsBinaryLiner(int toBase) : base(toBase)
        {
            //Dividers = PolynomialsFirstPower[toBase];
            PolinomClassic = new PolynomialsBinaryClassic(toBase);
        }
        public override async Task GenerateDetermAsync(int power)
        {
            Power = power;
            await GenerateDividers();
            Bits = BitOll(BigInteger.Pow(2, BitOll(Min(power)) - 1));
            Items = await StartAsync();
        }
        public override async Task GenerateStohasAsync(int power, int countPolinom)
        {
            Power = power;
            CountGenerate = countPolinom;
            await GenerateDividers();

            Items.Clear();
            Persent = PersentProgress(countPolinom, 0);
            Cts = new CancellationTokenSource();
            Token = Cts.Token;
            Bits = BitOll(BigInteger.Pow(2, BitOll(Min(power)) - 1));

            var taskList = Enumerable.Range(0, Environment.ProcessorCount).Select(x => Task.Run(() => GenerateStohas(Min(Power), Max(Power))));
            await Task.WhenAll(taskList.ToArray());
            Sort();
        }
        private async Task<List<int[]>> StartAsync()
        {
            int countTask = Environment.ProcessorCount;
            long difference = (long)(Max(Power) + 1 - Min(Power)) / countTask;

            var taskList = new List<Task<List<int[]>>>() { };
            Cts = new CancellationTokenSource();
            Token = Cts.Token;
            for (int i = 0; i < countTask; i++)
            {
                long minValue = (long)Min(Power) + difference * i;
                long maxValue = i == countTask - 1 ? (long)Max(Power) : (long)Min(Power) + difference * (i + 1);
                bool progres = i == 0 ? true : false;
                taskList.Add(Task.Run(() => GenerateDeterm(minValue, maxValue, progres)));
            }
            var res = await Task<List<int[]>>.Factory.ContinueWhenAll(taskList.ToArray(), ts => ts.SelectMany(x => x.Result.ToArray()).ToList());
            return res;
        }
        private List<int[]> GenerateDeterm(long min, long max, bool progress = false)
        {
            var polinoms = new List<int[]>();
            int power = ToString(min, ToBase).Length - 1;
            Persent = PersentProgress(min, max);
            while (min < max && Token.IsCancellationRequested == false)
            {
                if ((min & 1) == 1 && (BitInt(min) & 1) == 1)
                {
                    if (IsPolinom(min) == true)
                    {
                        polinoms.Add(IntToArray(min, power));
                    }
                }
                if (progress == true && min % Persent == 0)
                {
                    Progress = (int)((double)(min - (long)Min(Power)) / (max - (long)Min(Power)) * 100);
                }
                min++;
            }
            return polinoms;
        }
        private void GenerateStohas(BigInteger min, BigInteger max)
        {
            var randomBigInteger = new RandomBigInteger();
            int power = ToString(min, ToBase).Length - 1;
            int CountTestingPolinom = 0;

            while (IsCansel == false)
            {
                BigInteger testPolinom = randomBigInteger.NextBigInteger(min, max);
                if ((testPolinom & 1) == 1 && (BitInt(testPolinom) & 1) == 1)
                {
                    CountTestingPolinom++;
                    if (IsPolinom(testPolinom) == true)
                    {
                        Add(IntToArray(testPolinom, power), CountTestingPolinom);
                        CountTestingPolinom = 0;
                    }
                }
            }
        }
        private async Task GenerateDividers()
        {
            if (ExclusivePower.TryGetValue(Power, out int[] value))
            {
                var tmpList = new List<long>();
                for (int i = 0; i < value.Length; i++)
                {
                    await PolinomClassic.GenerateDetermAsync(value[i]);
                    tmpList.AddRange(PolinomClassic.Items.Select(x => Convert.ToInt64(string.Join("", x), 2)));
                }
                Dividers = tmpList.ToArray();
                LengthDividers = new long[Dividers.Length];
                for(int i = 0; i < Dividers.Length; i++ )
                {
                    LengthDividers[i] = (long)Math.Pow(2, BitOll(Dividers[i]));
                }
            }
        }
        private protected override bool IsPolinom(dynamic testPolinom)
        {
            if (CheckDivision(testPolinom) == true)
            {
                int Res = 2;
                long _Res = Mult(Res, Res << 1, testPolinom);
                while (_Res > 1 && Res < Power)
                {
                    Res += 1;
                    _Res = Mult(_Res, _Res << 1, testPolinom);
                }
                if (_Res == 1 && Res == Power)
                {
                    return true;
                }
            }
            return false;
        }
        private protected bool IsPolinom(BigInteger testPolinom)
        {
            if (CheckDivision(testPolinom) == true)
            {
                int Res = 2;
                BigInteger _Res = Mult(Res, Res << 1, testPolinom);
                while (_Res > 1 && Res < Power)
                {
                    Res += 1;
                    _Res = Mult(_Res, _Res << 1, testPolinom);
                }
                if (_Res == 1 && Res == Power)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CheckDivision(long testPolinom)
        {
            for (int j = 0; j < Dividers.Length; j++)
            {
                if (Mod(testPolinom, Dividers[j], LengthDividers[j]) == 0)
                    return false;
            }
            return true;
        }
        private bool CheckDivision(BigInteger testPolinom)
        {
            for (int j = 0; j < Dividers.Length; j++)
            {
                if (Mod(testPolinom, Dividers[j], LengthDividers[j]) == 0)
                    return false;
            }
            return true;
        }
        public long Mult(long a, long b, long i)
        {
            long p = 0;
            while (b > 0)
            {
                p ^= -(b & 1) & a;
                a = (a << 1) ^ (i & -((a >> Bits) & 1));
                b >>= 1;
            }
            return p;
        }
        public BigInteger Mult(BigInteger a, BigInteger b, BigInteger i)
        {
            BigInteger p = 0;
            while (b > 0)
            {
                p ^= -(b & 1) & a;
                a = (a << 1) ^ (i & -((a >> Bits) & 1));
                b >>= 1;
            }
            return p;
        }
    }
}
