using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GFLib;
using GFLib.AbstractFactory;
using GFLib.Interface;

namespace GFСomparison.Model
{
    public class ComparisonPolinom 
    {
        public List<int[]> PolinomComparison = new List<int[]>();
        private long progress;
        public long Progress
        {
            get
            {
                return progress;
            }
            private set
            {
                progress = value;
                ProgressEvent?.Invoke();
            }
        }
        public bool IsCansel {
            get
            {
                return Classic.IsCansel | Liner.IsCansel;
            }
        }
        public event Action ProgressEvent;

        private IPolynomials Classic;
        private IPolynomials Liner;
        public ComparisonPolinom(int toBase)
        {
            Classic = PolynomialGFFactory.GetFactoryClassic(toBase);
            Liner = PolynomialsFactory.GetFactoryLiner(toBase);
        }
        public async Task Generate(int power)
        {
            Liner.ProgressEvent += () => { Progress = Liner.Progress; };
            await Liner.GenerateDetermAsync(power);
            Classic.ProgressEvent += () => { Progress = Classic.Progress; };
            if(Liner.IsCansel == false)
                await Classic.GenerateDetermAsync(power);
            await Task.Run(() => Comparison());
        }
        private void Comparison()
        {
            PolinomComparison = Liner.Items.Except(Classic.Items, new PolinomCompare()).ToList();
        }    
        public void Cansel()
        {
            Classic.Cansel();
            Liner.Cansel();
        }
    }
}
