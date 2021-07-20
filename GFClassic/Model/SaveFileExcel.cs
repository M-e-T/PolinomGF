using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace GFClassic.Model
{
    public class SaveFileExcel
    {
        private CancellationTokenSource cts;
        private CancellationToken token;

        public event Action Progres;
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
                Progres?.Invoke();

            }
        }
        public async Task Save(string filename, List<int[]> saveList)
        {
            cts = new CancellationTokenSource();
            token = cts.Token;
            Task task = new Task(() =>
            {
                long maxLengthPage = 1048577; //1048576
                int p = (saveList.Count) / 100;
                if ((p & 1) == 0)
                    p++;
                filename = filename.TrimEnd(".xlsx".ToCharArray());
                int item = 0;
                int countFile = 0;
                while (item < saveList.Count)
                {
                    if (token.IsCancellationRequested)
                        break;
                    countFile++;
                    using (ExcelPackage excelPackage = new ExcelPackage())
                    {
                        excelPackage.Workbook.Properties.Created = DateTime.Now;
                        int countPage = 0;
                        while (countPage < 12 && item < saveList.Count)
                        {
                            if (token.IsCancellationRequested)
                                break;
                            countPage++;
                            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Page" + countPage);
                            int iterator = 1;
                            while (item < saveList.Count && iterator < maxLengthPage)
                            {
                                if (token.IsCancellationRequested)
                                    break;
                                worksheet.Cells[iterator, 1].Value = string.Join("",saveList[item]);
                                worksheet.Cells[iterator, 1].Style.Numberformat.Format = "@";
                                if (item % p == 0)
                                {
                                    long pr = (long)((double)(item) / (saveList.Count) * 100);
                                    Progress = pr;
                                }
                                iterator++;
                                item++;
                            }
                        }
                        //Save your file
                        FileInfo fi = new FileInfo(filename + ".xlsx");
                        excelPackage.SaveAs(fi);
                    }
                }
            });
            task.Start();
            await task;
        }
    }
}
