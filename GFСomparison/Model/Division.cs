using System.Collections.Generic;
using System.Linq;

using GFLib;

namespace GFСomparison.Model
{
    public class Division
    {
        private List<Item> Items = new List<Item>();
        public List<long> Dividers = new List<long>();
        public Division() { }
        public Division(List<long> dividers)
        {
            Dividers = dividers;
        }
        public Item this[int i]
        {
            get
            {
                return Items[i];
            }
        }
        public int Count => Items.Count;
        public int CountZero(int i)
        {
            return Items.Where(x => x.Remainder[i].Length == 0).Count();
        }
        public void Divide()
        {
            foreach(var item in Items)
            {
                item.DividePolinom(Dividers);
            }
        }
        public void Add(Item item) 
        {
            Items.Add(item);
        }
        public void Clear()
        {
            Items.Clear();
        }
        public void RemoveZero()
        {
            for (int i = 0; i < Count; i++)
            {
                Items.RemoveAll(x => x.Remainder.Contains(""));
            }
        }
           
    }
    public class Item: ModularArithmetic
    {
        public List<string> Remainder;
        public int[] Polinom { get; }
        public Item(int[] polinom, int toBase) : base(toBase)
        {
            Polinom = polinom;
        }
        public void DividePolinom(List<long> dividers)
        {
            Remainder = new List<string>();
            for (int j = 0; j < dividers.Count; j++)
            {
                int[] divider = dividers[j].ToString().Select(x => x - '0').ToArray();
                Remainder.Add(string.Join("", DivederByModul(CopyArr(Polinom), divider)));
            }
        }
    }
}
