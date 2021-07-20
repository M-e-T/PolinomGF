using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace GFLib.Interface
{
    public interface IPolinom
    {
        event Action ProgressEvent;
        int Power { get; }
        int ToBase { get; }
        int Count { get; }
        int CountGenerate { get; }
        BigInteger CountTesting { get; }
        int Progress { get; }
        bool IsCansel { get; }
        int[] this[int index] { get; }
        List<int[]> Polynomials { get; }
        Task GenerateDetermAsync(int power);
        Task GenerateStohasAsync(int power, int countPolinom);
        void Cansel();
    }
}
