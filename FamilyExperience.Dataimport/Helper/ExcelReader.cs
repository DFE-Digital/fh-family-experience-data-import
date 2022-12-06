using System.Data;
using OfficeOpenXml;
using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Helper
{
    public static class ExcelReader
    {        public static string ReadExcel(string path)
        {
            using (var excelPackage = new ExcelPackage())
            {
                using (var stream = File.OpenRead(path))
                {
                    excelPackage.Load(stream);
                }

                var workSheetDetails = excelPackage.Workbook.Worksheets[0];                
                var excelasTable = new DataTable();
                foreach (var firstRowCell in workSheetDetails.Cells[1, 1, 1, workSheetDetails.Dimension.End.Column])
                {                    
                    if (!string.IsNullOrEmpty(firstRowCell.Text))
                    {
                        excelasTable.Columns.Add(firstRowCell.Text);
                    }
                }
                var startRow = 4;                
                for (var rowNum = startRow; rowNum <= workSheetDetails.Dimension.End.Row; rowNum++)
                {
                    var wsRow = workSheetDetails.Cells[rowNum, 2, rowNum, excelasTable.Columns.Count];
                    var row = excelasTable.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column-2] = cell.Text;
                       
                    }
                }

                var generatedTable = JsonConvert.SerializeObject(excelasTable);
                return generatedTable;
            }
        }

    }
}
