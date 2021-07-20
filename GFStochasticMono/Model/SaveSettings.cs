using System;

namespace GFStochasticMono.Model
{
    [Serializable]
    public class SaveSettings
    {
        public string TextFont = "Times New Rouman";
        public string TextInterval = "1";
        public int CountColum = 4;
        public int CountRow = 40;
        public int TextSize = 12;
        public double ColumnWidth = 3.17;
        public double RowHeight = 0.55;
        public bool AutoNumbering = true;
        public int Power;
        public int Weight;
    }
}
