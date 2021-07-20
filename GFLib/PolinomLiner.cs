using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace GFLib
{
    public class PolinomLiner : Polinom
    {        
        private int[] StartRes;
        private new int[][]Dividers = new int[][]{ };
        private PolinomClassic PolinomClassic;
        private Dictionary<int, int[]> ExclusivePower = new Dictionary<int, int[]>()
        {
            {12, new int[] { 2 } },
            {18, new int[] { 3 } },
            {20, new int[] { 2 } },
            {24, new int[] { 2, 4 } },
            {28, new int[] { 2 } },
            {30, new int[] { 2, 3, 5 } },
        };
        public PolinomLiner(int toBase) : base(toBase)
        {
            Dividers = PolynomialsFirstPower[toBase];
            PolinomClassic = new PolinomClassic(toBase);
        }
        public override async Task GenerateDetermAsync(int power)
        {
            Power = power;
            await GenerateDividers();
            StartRes = FastPow(ToBase - 1).ToString().Select(x => x - '0').ToArray();

            Polynomials = await StartAsync();
        }
        public override async Task GenerateStohasAsync(int power, int countPolinom)
        {
            Power = power;
            CountGenerate = countPolinom;
            await GenerateDividers();

            Polynomials.Clear();
            Persent = PersentProgress(Min(Power), Max(Power));
            cts = new CancellationTokenSource();
            token = cts.Token;
            StartRes = FastPow(ToBase - 1).ToString().Select(x => x - '0').ToArray();

            var taskList = Enumerable.Range(0, Environment.ProcessorCount).Select(x => Task.Run(() => GenerateStohas(Min(Power), Max(Power))));
            await Task.WhenAll(taskList.ToArray());
            Sort();
        }
        private async Task<List<int[]>> StartAsync()
        {
            int countTask = Environment.ProcessorCount;
            long difference = (Max(Power) + 1 - Min(Power)) / countTask;

            var taskList = new List<Task<List<int[]>>>() { };
            cts = new CancellationTokenSource();
            token = cts.Token;
            for (int i = 0; i < countTask; i++)
            {
                long minValue = Min(Power) + difference * i;
                long maxValue = i == countTask - 1 ? Max(Power) : Min(Power) + difference * (i + 1);
                bool progres = i == 0 ? true : false;
                taskList.Add(Task.Run(() => GenerateDeterm(minValue, maxValue, progres)));
            }
            var res = await Task<List<int[]>>.Factory.ContinueWhenAll(taskList.ToArray(), ts => ts.SelectMany(x => x.Result.ToArray()).ToList());
            return res;
        }
        private List<int[]> GenerateDeterm(long min, long max, bool progress = false)
        {
            Persent = PersentProgress(min, max);
            var polinoms = new List<int[]>();
            while (min < max && token.IsCancellationRequested == false)
            {
                int[] testPolinom = IntToArray(min, Power);
                if (testPolinom[Power] > 0 && CheckDivision(testPolinom) == false)
                {
                    if (IsPolinom(testPolinom) == true)
                    {
                        polinoms.Add(CopyArr(testPolinom));
                    }
                }
                if (progress == true && min % Persent == 0)
                {
                    Progress = (int)((double)(min - Min(Power)) / (max - Min(Power)) * 100);
                }
                min++;
            }
            return polinoms;
        }
        private void GenerateStohas(BigInteger min, BigInteger max)
        {
            var randomBigInteger = new RandomBigInteger();
            int CountTestingPolinom = 0;

            while (token.IsCancellationRequested == false)
            {
                int[] testPolinom = IntToArray(randomBigInteger.NextBigInteger(min, max), Power);
                if (testPolinom[Power] > 0 )
                {
                    CountTestingPolinom++;
                    if (IsPolinom(testPolinom) == true)
                    {
                        Add(testPolinom, CountTestingPolinom);
                        CountTestingPolinom = 0;
                    }
                }
            }
        }
        private async Task GenerateDividers()
        {
            if (ExclusivePower.TryGetValue(Power, out int[] value))
            {
                var tmpList = new List<int[]>();
                for (int i = 0; i < value.Length; i++)
                {
                    await PolinomClassic.GenerateDetermAsync(value[i]);
                    tmpList.AddRange(PolinomClassic.Polynomials);
                }
                tmpList.InsertRange(0, Dividers);
                Dividers = tmpList.ToArray();
            }
        }
        private protected override bool IsPolinom(int[] testPolinom)
        {
            int Res = 1;
            int[] ResNew = CopyArr(StartRes);
            while (Res < Power)
            {
                Res += 1;
                ResNew = Pow(ResNew, ToBase - 1);
                int[] arrShift = ArrayShiftLeft(ResNew, ToBase - 1);
                ResNew = DivederByModul(arrShift, testPolinom);
                if (ResNew.Length == 1 && ResNew[0] == 1)
                    break;
            }
            if (ResNew.Length == 1 && ResNew[0] == 1 && Res == Power)
            {
                return true;
            }
            return false;
        }
        private bool CheckDivision(int[] divided)
        {
            for(int i = 0; i < Dividers.Length; i++)
            {
                if(DivederByModul(CopyArr(divided), Dividers[i]).Length == 0)
                {
                    return true;
                }
            }
            return false;
        }    
        private BigInteger FastPow(BigInteger pow)
        {
            BigInteger result = 1;
            while (pow > 0)
            {
                result *= 10;
                pow--;
            }
            return result;
        }
    }
}
