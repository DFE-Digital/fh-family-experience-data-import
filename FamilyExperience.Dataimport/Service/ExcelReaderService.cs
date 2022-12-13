using System.Data;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace FamilyExperience.DataImport.Service
{
    public static class ExcelReaderService
    {
        public static T ReadExcel<T>(string path)
        {
            
            using var excelPackage = new ExcelPackage();

            using (var stream = File.OpenRead(path))
            {
                excelPackage.Load(stream);
            }

            var workSheetDetails = excelPackage.Workbook.Worksheets[0];

            Console.WriteLine($"There are {workSheetDetails.Dimension.Rows} rows in excel");

            var excelAsTable = new DataTable();

            foreach (var firstRowCell in workSheetDetails.Cells[1, 1, 1, workSheetDetails.Dimension.End.Column])
            {
                if (!string.IsNullOrEmpty(firstRowCell.Text))
                {
                    excelAsTable.Columns.Add(firstRowCell.Text);
                }
            }

            const int startRow = 6;

            for (var rowNum = startRow; rowNum <= workSheetDetails.Dimension.End.Row; rowNum++)
            {
                
                var wsRow = workSheetDetails.Cells[rowNum, 2, rowNum, excelAsTable.Columns.Count];

                var cellWithValueCount = wsRow.Skip(1).Count(cell => !string.IsNullOrWhiteSpace(cell.Text));

                if (cellWithValueCount == 0) { continue; }

                var row = excelAsTable.Rows.Add();
                Console.WriteLine($"Started reading row:{rowNum} in excel.");

                foreach (var cell in wsRow)
                {
                    row[cell.Start.Column - 2] = cell.Text;
                }
                Console.WriteLine($"Completed reading row:{rowNum} in excel.");
            }

            var generatedTable = JsonConvert.SerializeObject(excelAsTable);
            Console.WriteLine($"Processing excel completed.");


            return JsonConvert.DeserializeObject<T>(generatedTable);
        }
    }
}
