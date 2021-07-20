using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GFStochasticMono.Model;

namespace GFStochasticMono.Interface
{
    public interface ISerialize
    {
        void SaveSettings(SaveSettings saveSettings);
        SaveSettings OpenSettings();
    }
}
