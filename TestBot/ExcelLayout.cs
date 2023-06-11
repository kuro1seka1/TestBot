using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot
{
    class ExcelLayout
    {
        public async void Excel(string pathToAnalisis)
        {
            List<string> table = new();
            Workbook wb = new Workbook(pathToAnalisis);

            string dest = "testKursk.txt";
            WorksheetCollection collections = wb.Worksheets;
            Random random = new Random();
            for (int worksheetIndex = 0; worksheetIndex < collections.Count; worksheetIndex++)
            {

                // Получить рабочий лист, используя его индекс
                Worksheet worksheet = collections[worksheetIndex];

                // Печать имени рабочего листа
                Console.WriteLine("Worksheet: " + worksheet.Name);

                // Получить количество строк и столбцов
                int rows = worksheet.Cells.MaxDataRow;
                int cols = worksheet.Cells.MaxDataColumn + 1;
                int[] ranges = new int[] { 1, 30, 60, 90, 120, 150, 180 };
                int tes = random.Next(ranges.Length);

                int firstRange = ranges[tes - 1];
                //добавить проверку воторого значения
                    if(firstRange != 0 )
                    {
                        using (StreamWriter writer = new StreamWriter(dest, true, Encoding.UTF8))
                        {
                            for (int i = ranges[tes - 1]; i < ranges[tes]; i++)
                            {

                                // Перебрать каждый столбец в выбранной строке
                                for (int j = 0; j < cols; j++)
                                {
                                    // Значение чейки Pring
                                    var test = "|" + worksheet.Cells[i, j].Value + "|";


                                    await writer.WriteAsync(test.TrimStart('|'));

                                }
                                // Распечатать разрыв строки
                                await writer.WriteAsync('\n');

                            }

                        }
                    }
                    else
                    {
                        using (StreamWriter writer = new StreamWriter(dest, true, Encoding.UTF8))
                        {
                            for (int i = ranges[tes]; i < ranges[tes+1]; i++)
                            {

                                // Перебрать каждый столбец в выбранной строке
                                for (int j = 0; j < cols; j++)
                                {
                                    // Значение чейки Pring
                                    var test = "|" + worksheet.Cells[i, j].Value + "|";


                                    await writer.WriteAsync(test.TrimStart('|'));

                                }
                                // Распечатать разрыв строки
                                await writer.WriteAsync('\n');

                            }

                        }

                    }
                   
                
               

            }
        }
    }
}
