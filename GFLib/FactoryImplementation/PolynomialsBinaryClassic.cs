using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GFLib.Interface;
using GFLib.AbstractFactory;
namespace GFLib.FactoryImplementation
{
    public class PolynomialsBinaryClassic : PolynomialsBinaryFactory
    {
        private long[] LengthDividers;
        public override int Power
        {
            get { return base.Power; }
            set
            {
                if (value > MaxPower)
                    throw new ArgumentException(nameof(base.Power), $"Max power { MaxPower }");
                if ((Math.Pow(ToBase, value) + 1) < 1)
                    throw new ArgumentException(nameof(base.Power), "Min power 1");
                base.Power = value;
            }
        }
        public PolynomialsBinaryClassic(int toBase) : base(toBase)
        {
            MaxPower = Enumerable.Range(0, 63).Select(x => Max(x)).Count(x => x < long.MaxValue) - 1;
        }
        private async Task GenerateDividers()
        {
            List<long> tempList = Dividers.ToList();
            for (int i = Dividers.Length + 1; i <= Power / 2; i++)
            {
                tempList.AddRange((await StartAsync(i)).Select(x => Convert.ToInt64(string.Join("", x), 2)).ToArray());
                Progress = 0;
            }
            Dividers = tempList.ToArray();
            LengthDividers = new long[Dividers.Length];
            for (int i = 0; i < Dividers.Length; i++)
            {
                LengthDividers[i] = (long)Math.Pow(2, BitOll(Dividers[i]));
            }
            Dividers = tempList.ToArray();
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
            await Task.WhenAll(Enumerable.Range(0, Environment.ProcessorCount).Select(x => Task.Run(() => GenerateStohas((long)Min(Power), (long)Max(Power)))).ToArray());
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
                if ((min & 1) == 1 && (BitInt(min) & 1) == 1)
                {
                    if (IsPolinom(min) == true)
                        polinoms.Add(IntToArray(min, power));
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
                long testPolinom = randomPolinom.LongRandom(min, max);
                if ((min & 1) == 1 && (BitInt(min) & 1) == 1)
                {
                    coutTest++;
                    if (IsPolinom(testPolinom) == true)
                    {
                        Add(IntToArray(testPolinom, power), coutTest);
                    }
                }
            }
        }
        private protected override bool IsPolinom(dynamic testPolinom)
        {
            for (int j = 0; j < Dividers.Length; j++)
            {
                if (Mod(testPolinom, Dividers[j], LengthDividers[j]) == 0)
                    return false;
            }
            return true;
        }    
    }
}
