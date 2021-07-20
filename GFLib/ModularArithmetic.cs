using System;
using System.Linq;
using System.Numerics;


namespace GFLib
{
    public class ModularArithmetic
    {
        private int Module { get; }
        public ModularArithmetic(int module)
        {
            Module = module;
        }
        protected int[] CopyArr(int[] arr)
        {
            int[] result = new int[arr.Length];
            for (int i = 0; i < arr.Length; i++)
                result[i] = arr[i];
            return result;
        }
        protected int[] IntToArray(BigInteger value, int power)
        {
            int[] result = new int[power + 1];
            for (int i = result.Length - 1; i > -1; i--)
            {
                result[i] = (int)(value % Module);
                value = value / Module;
            }
            return result;
        }
        protected int[] IntToArray(long value, int power)
        {
            int[] result = new int[power + 1];
            for (int i = result.Length - 1; i > -1; i--)
            {
                result[i] = (int)(value % Module);
                value = value / Module;
            }
            return result;
        }

        protected int[] XorEveryHalf(int[] leftArr, int multiplier)
        {
            int[] result = new int[leftArr.Length];
            for (int i = 0; i < leftArr.Length; i++)
            {
                result[i] = (leftArr[i] * multiplier) % Module;
            }
            return result;
        }
        protected int[] XorLeftMinus(int[] leftArr, int[] rightArr)
        {
            if (leftArr.Length >= rightArr.Length)
            {
                for (int i = 0; i < rightArr.Length; i++)
                {
                    leftArr[i] = mod(leftArr[i] - rightArr[i]);
                }
                return leftArr;
            }
            else
            {
                throw new ArgumentNullException("Division error");
            }
        }
        protected int[] Xor(int[] left, int[] right)
        {
            if (left.Length >= right.Length)
            {
                for (int i = 0; i < right.Length; i++)
                {
                    left[left.Length - 1 - i] = (left[left.Length - 1 - i] + right[right.Length - 1 - i]) % Module;
                }
                return left;
            }
            else
            {
                for (int i = 0; i < left.Length; i++)
                {
                    right[i] = (left[i] + right[i]) % Module;
                }
                return right.Reverse().ToArray();
            }
        }
        protected int[] MultiplicationXor(int[] left, int[] right)
        {
            int[] result = new int[] { };
            for (int i = 0; i < right.Length; i++)
            {
                if (right[right.Length-1-i] == 0)
                    continue;
                else
                {
                    int[] _txor = XorEveryHalf(left, right[right.Length - 1 - i]);
                    int[] _tLeft = ArrayShiftLeft(_txor, i);
                    result = Xor(_tLeft, result);
                }
            }
            return result;
        }
        protected int[] DivederByModul(int[] value, int[] mod)
        {
            while (value.Length >= mod.Length)
            {
                while (value[0] > 0)
                {
                    value = XorLeftMinus(value, mod);
                }
                ArrayShiftLeft(ref value);
            }
            return value;
        }
        protected int[] Pow(int[] val, int i)
        {
            int[] result = val;
            while (i > 0)
            {
                result = MultiplicationXor(result, val);
                i--;
            }
            return result;
        }
        protected int[] ArrayShiftLeft(int[] arr, int count)
        {
            if (count <= 0)
                return arr;
            int[] newArr = new int[arr.Length + count];
            for (int i = 0; i < arr.Length; i++)
            {
                newArr[i] = arr[i];
            }
            return newArr;
        }
        protected void ArrayShiftLeft(ref int[] arr)
        {
            int size = CountZeroArray(arr);
            int[] newArr = new int[arr.Length - size];
            for (int i = 0; i < newArr.Length; i++)
            {
                newArr[i] = arr[i + size];
            }
            arr = newArr;
        }
        protected int mod(int value)
        {
            return (value % Module + Module) % Module;
        }
        protected int CountZeroArray(int[] arr)
        {
            int count = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == 0)
                    count++;
                else
                    break;
            }
            return count;
        }
    }
}
