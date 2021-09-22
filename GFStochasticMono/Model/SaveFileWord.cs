using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Linq;

using Xceed.Words.NET;
using Xceed.Document.NET;

using GFLib.Interface;

namespace GFStochasticMono.Model
{
    public class SaveFileWord
    {
        public event Action ProgressEvent;

        private SaveSettings saveSettings;
        private long progress;
        public long Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                ProgressEvent?.Invoke();

            }
        }
        public SaveFileWord(SaveSettings settings)
        {
            saveSettings = settings;
        }
        //public async Task Save(string filename, List<int[]> saveList, int tobase, int countSymbol)
        public async Task Save(string filename, IPolynomials polinom, int countSymbol)
        {
            Task task = new Task(() =>
            {
                int p = (polinom.Count) / 100;
                if ((p & 1) == 0)
                    p++;
                int item = 0;
                int CountTable = 1;
                DocX document = DocX.Create(filename);
                while (item < polinom.Count)
                {
                    // создаём таблицу с строками и столбцами
                    Table table = document.AddTable(saveSettings.CountRow + 1, saveSettings.CountColum + 1);
                    // располагаем таблицу по центру
                    table.Alignment = Alignment.center;
                    // меняем стандартный дизайн таблицы
                    table.Design = TableDesign.None;
                    table.SetColumnWidth(0, 26);

                    if (saveSettings.AutoNumbering == true && saveSettings.Weight == 0)
                    {
                        document.InsertParagraph("").Font("Times New Rouman").FontSize(12);
                        document.InsertParagraph($"GF ({polinom.ToBase})-" + saveSettings.Power + "-" + CountTable).Font(saveSettings.TextFont).FontSize(12).Bold().
                        Alignment = Alignment.center;
                    }
                    CountTable++;
                    for (int i = 1; i <= saveSettings.CountColum; i++)
                    {
                        table.SetColumnWidth(i, saveSettings.ColumnWidth * 28.3464567);
                    }
                    for (int i = 1; i <= saveSettings.CountColum; i++)
                    {
                        string s = Char.ToString((char)(i + 64));
                        table.Rows[0].Cells[i].Paragraphs[0].Append(s).Font(saveSettings.TextFont).FontSize(saveSettings.TextSize).Alignment = Alignment.center;
                        table.Rows[0].Cells[i].SetBorder(TableCellBorderType.Left, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                        table.Rows[0].Cells[i].SetBorder(TableCellBorderType.Right, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                        table.Rows[0].Cells[i].SetBorder(TableCellBorderType.Bottom, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                        table.Rows[0].Cells[i].SetBorder(TableCellBorderType.Top, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                    }
                    int countRow = 0;
                    for (int i = 1; i <= saveSettings.CountRow; i++)
                    {
                        int tmp = (int)Math.Ceiling((double)saveSettings.Power / countSymbol);
                        if ((i-1) % tmp == 0)
                        {
                            countRow += 1;
                            table.Rows[i].Cells[0].Paragraphs[0].Append(countRow.ToString()).Font(saveSettings.TextFont).FontSize(saveSettings.TextSize).Alignment = Alignment.center;
                            table.Rows[i].Cells[0].SetBorder(TableCellBorderType.Top, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                            table.Rows[i].Cells[0].SetBorder(TableCellBorderType.Bottom, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                            table.Rows[i].Cells[0].SetBorder(TableCellBorderType.Left, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                            table.Rows[i].Cells[0].SetBorder(TableCellBorderType.Right, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                        }
                        table.Rows[i].Cells[0].SetBorder(TableCellBorderType.Left, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                        table.Rows[i].Cells[0].SetBorder(TableCellBorderType.Right, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                    }
                    table.Rows[saveSettings.CountRow].Cells[0].SetBorder(TableCellBorderType.Bottom, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));

                    table.Rows[0].Cells[0].SetBorder(TableCellBorderType.Left, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                    table.Rows[0].Cells[0].SetBorder(TableCellBorderType.Right, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                    table.Rows[0].Cells[0].SetBorder(TableCellBorderType.Bottom, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                    table.Rows[0].Cells[0].SetBorder(TableCellBorderType.Top, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                    for (int j = 1; j <= saveSettings.CountColum; j++)
                    {
                        for (int i = 1; item < polinom.Count && i <= saveSettings.CountRow;)
                        {
                            List<string> str = Split(string.Join("",polinom[item]), countSymbol);
                            for (int t = 0; t < str.Count; t++)
                            {
                                table.Rows[t + i].Cells[j].Paragraphs[0].Append(str[t]).Font(saveSettings.TextFont).FontSize(saveSettings.TextSize);
                                table.Rows[t + i].Height = saveSettings.RowHeight * 28.3464567;
                                table.Rows[t + i].Cells[j].VerticalAlignment = VerticalAlignment.Center;
                                table.Rows[t + i].Cells[j].SetBorder(TableCellBorderType.Left, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                                table.Rows[t + i].Cells[j].SetBorder(TableCellBorderType.Right, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                            }
                            i += str.Count;
                            if (item % p == 0)
                            {
                                Progress = (int)((double)(item) / (polinom.Count) * 100);
                            }
                            item++;
                        }
                        table.Rows[saveSettings.CountRow].Cells[j].SetBorder(TableCellBorderType.Bottom, new Border(BorderStyle.Tcbs_single, BorderSize.one, 0, Color.Black));
                    }
                    //table.Alignment = Alignment.center;
                    document.InsertParagraph().InsertTableAfterSelf(table);
                    //Save your file
                }
                // сохраняем документ
                document.Save();
            });
            task.Start();
            await task;
        }
        public List<string> Split(string str, int chunkSize)
        {
            if(str.Length - 1 < chunkSize)
                return new List<string> { str };
            List<string> Result = new List<string>();
            int groupingLength = str.Length % chunkSize;
            Result.Add(str.Substring(0, chunkSize));
            for (int i = chunkSize; i < str.Length - groupingLength; i += chunkSize)
            {
                Result.Add(str.Substring(i, chunkSize));
            }
            if (groupingLength > 0)
                Result.Add(str.Substring(str.Length - groupingLength, groupingLength));
            if(Result.Last().Length == 1)
            {
                Result[Result.Count - 2] = Result[Result.Count - 2] + Result[Result.Count - 1];
                Result.RemoveAt(Result.Count - 1);
            }
            return Result;
        }
    }
}
