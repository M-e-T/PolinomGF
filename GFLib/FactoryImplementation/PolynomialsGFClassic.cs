using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GFLib.AbstractFactory;

namespace GFLib.FactoryImplementation
{
    public class PolynomialsGFClassic : PolynomialGFFactory
    {
        private int countDividers;
        private int CountDividers
        {
            get
            {
                return countDividers;
            }
            set
            {
                if (value <= Power / 2)
                    countDividers = Dividers.Count / 2 + 1;
                else
                    countDividers = Dividers.Count;
            }
        }
        public override int Power
        {
            get { return base.Power; }
            set
            {
                if (value > MaxPower)
                    throw new ArgumentException($"Max power { MaxPower }", nameof(base.Power));
                if ((Math.Pow(ToBase, value) + 1) < 1)
                    throw new ArgumentException(nameof(base.Power), "Min power 1");
                base.Power = value;
            }
        }
        public PolynomialsGFClassic(int toBase) : base(toBase)
        {
            MaxPower = Enumerable.Range(0, 63).Select(x => Max(x)).Count(x => x < long.MaxValue) - 1;
        }
        private async Task GenerateDividers()
        {
            for (int i = Dividers.Count + 1; i <= Power / 2; i++)
            {
                CountDividers = i;
                Dividers.Add((await StartAsync(i)).ToArray());
                Progress = 0;
            }
            CountDividers = Power;
        }
        public override async Task GenerateDetermAsync(int power)
        {
            Power = power;
            await GenerateDividers();
            Items = await StartAsync(Power);
        }
        public override async Task GenerateStohasAsync(int power, int count)
        {
            Power = power;
            CountGenerate = count;
            Cts = new CancellationTokenSource();
            Token = Cts.Token;
            await GenerateDividers();
            await Task.WhenAll(Enumerable.Range(0, Environment.ProcessorCount).Select(x => Task.Run(() =>
            GenerateStohas((long)Min(Power), (long)Max(Power)))).ToArray());
        }
        private async Task<List<int[]>> StartAsync(int power)
        {
            int countTask = Environment.ProcessorCount;
            long difference = (long)(Max(power) + 1 - Min(power)) / countTask;

            var taskList = new List<Task<List<int[]>>>() { };
            Cts = new CancellationTokenSource();
            Token = Cts.Token;
            for (int i = 0; i < countTask; i++)
            {
                long minValue = (long)Min(power) + difference * i;
                long maxValue = i == countTask - 1 ? (long)Max(power) : (long)Min(power) + difference * (i + 1);
                bool progres = i == 0 ? true : false;
                taskList.Add(Task.Run(() => GenerateDeterm(minValue, maxValue, progres)));
            }
            var res = await Task<List<int[]>>.Factory.ContinueWhenAll(taskList.ToArray(), ts => ts.SelectMany(x => x.Result.ToArray()).ToList());
            return res;
        }
        private List<int[]> GenerateDeterm(long min, long max, bool progress = false)
        {
            int power = ToString(min, ToBase).Length - 1;
            Persent = PersentProgress(min, max);
            var polinoms = new List<int[]>();
            while (min < max && IsCansel == false)
            {
                int[] testPolinom = IntToArray(min, power);
                if (testPolinom[power] > 0 && Weight(testPolinom) % ToBase != 0)
                {
                    if (IsPolinom(testPolinom) == true)
                    {
                        polinoms.Add(CopyArr(testPolinom));
                    }
                }
                if (progress == true && min % Persent == 0)
                {
                    Progress = (int)((double)(min - (long)Min(power)) / (max - (long)Min(power)) * 100);
                }
                min++;
            }
            return polinoms;
        }
        private void GenerateStohas(long min, long max)
        {
            int power = ToString(min, ToBase).Length - 1;
            BigInteger coutTest = 0;
            var randomPolinom = new RandomBigInteger();
            while (Token.IsCancellationRequested == false)
            {
                int[] testPolinom = IntToArray(randomPolinom.LongRandom(min, max), power);
                if (testPolinom[power] > 0 && Weight(testPolinom) % ToBase != 0)
                {
                    coutTest++;
                    if (IsPolinom(testPolinom) == true)
                    {
                        Add(CopyArr(testPolinom), coutTest);
                    }
                }
            }
        }
        private protected override bool IsPolinom(int[] testPolinom)
        {
            for (int n = 0; n < CountDividers; n++)
            {
                for (int m = 0; m < Dividers[n].Length; m++)
                    if (DivederByModul(CopyArr(testPolinom), Dividers[n][m]).Length == 0)
                    {
                        return false;
                    }
            }
            return true;
        }
    }
}
