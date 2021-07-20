using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace GFLib
{
    public class PolinomClassic : Polinom
    {
        private int lenghtDividers;
        private int LenghtDividers 
        {
            get
            {
                return lenghtDividers;
            }
            set
            {
                if (value <= Power / 2)
                    lenghtDividers =  Dividers.Count / 2 + 1;
                else
                    lenghtDividers =  Dividers.Count;
            } 
        }
        public PolinomClassic(int toBase) : base(toBase)
        {
        }
        private async Task GenerateDividers()
        {
            for (int i = Dividers.Count + 1; i <= Power / 2; i++)
            {
                LenghtDividers = i;
                Dividers.Add((await StartAsync(i)).ToArray());
                Progress = 0;
            }
            LenghtDividers = Power;
        }
        public override async Task GenerateDetermAsync(int power)
        {
            Power = power;
            await GenerateDividers();
            Polynomials = await StartAsync(Power);
        }
        public override async Task GenerateStohasAsync(int power, int count)
        {
            Power = power;
            CountGenerate = count;
            cts = new CancellationTokenSource();
            token = cts.Token;
            await GenerateDividers();
            await Task.WhenAll(Enumerable.Range(0, Environment.ProcessorCount).Select(x => Task.Run(() => GenerateStohas(Min(Power), Max(Power)))).ToArray());
        }
        private async Task<List<int[]>> StartAsync(int power)
        {
            int countTask = Environment.ProcessorCount;
            long difference = (Max(power) + 1 - Min(power)) / countTask;

            var taskList = new List<Task<List<int[]>>>() { };
            cts = new CancellationTokenSource();
            token = cts.Token;
            for (int i = 0; i < countTask; i++)
            {
                long minValue = Min(power) + difference * i;
                long maxValue = i == countTask - 1 ? Max(power) : Min(power) + difference * (i + 1);
                bool progres = i == 0 ? true : false;
                taskList.Add(Task.Run(() => GenerateDeterm(minValue, maxValue, progres)));
            }
            var res = await Task<List<int[]>>.Factory.ContinueWhenAll(taskList.ToArray(), ts => ts.SelectMany(x => x.Result.ToArray()).ToList());
            return res;    
        }
        private List<int[]> GenerateDeterm(long min, long max, bool progress = false)
        {
            int power = BigIntegerHelper.IntToString(min, ToBase).Length - 1;
            Persent = PersentProgress(min, max);
            var polinoms = new List<int[]>();
            while (min < max && token.IsCancellationRequested == false)
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
                    Progress = (int)((double)(min - Min(power)) / (max - Min(power)) * 100);
                }
                min++;
            }
            return polinoms;
        }
        private void GenerateStohas(long min, long max)
        {
            int power = BigIntegerHelper.IntToString(min, ToBase).Length - 1;
            BigInteger coutTest = 0;
            var randomPolinom = new RandomBigInteger();
            while (token.IsCancellationRequested == false)
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
            for (int n = 0; n < LenghtDividers; n++)
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
