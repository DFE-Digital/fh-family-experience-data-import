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
                DataTable excelasTable = new DataTable();
                foreach (var firstRowCell in workSheetDetails.Cells[1, 1, 1, workSheetDetails.Dimension.End.Column])
                {                    
                    if (!string.IsNullOrEmpty(firstRowCell.Text))
                    {
                        string firstColumn = string.Format("Column {0}", firstRowCell.Start.Column);
                        excelasTable.Columns.Add(true ? firstRowCell.Text : firstColumn);
                    }
                }
                var startRow = 2;                
                for (int rowNum = startRow; rowNum <= workSheetDetails.Dimension.End.Row; rowNum++)
                {
                    var wsRow = workSheetDetails.Cells[rowNum, 2, rowNum, excelasTable.Columns.Count];
                    DataRow row = excelasTable.Rows.Add();
                    int count = 0;
                    foreach (var cell in wsRow)
                    {
                        
                        row[cell.Start.Column-1] = cell.Text;
                       
                    }
                }

                var generatedTable = JsonConvert.SerializeObject(excelasTable);
                return generatedTable;
            }
        }

    }
}
