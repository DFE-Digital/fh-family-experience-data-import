using System.Data;
using OfficeOpenXml;
using Newtonsoft.Json;

namespace FamilyExperience.Dataimport.Helper
{
    public class ExcelReader
    {        public string ReadExcel<T>(string path) 
        {
            using (var excelPack = new ExcelPackage())
            {
                //Load excel stream
                using (var stream = File.OpenRead(path))
                {
                    excelPack.Load(stream);
                }

                //Lets Deal with first worksheet.(You may iterate here if dealing with multiple sheets)
                var ws = excelPack.Workbook.Worksheets[0];

                //Get all details as DataTable -because Datatable make life easy :)
                DataTable excelasTable = new DataTable();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    //Get colummn details
                    if (!string.IsNullOrEmpty(firstRowCell.Text))
                    {
                        string firstColumn = string.Format("Column {0}", firstRowCell.Start.Column);
                        excelasTable.Columns.Add(true ? firstRowCell.Text : firstColumn);
                    }
                }
                var startRow = 2;
                //Get row details
                for (int rowNum = startRow; rowNum <= 4; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, excelasTable.Columns.Count];
                    DataRow row = excelasTable.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;
                    }
                }
                
                //Get everything as generics and let end user decides on casting to required type
                var generatedType = JsonConvert.SerializeObject(excelasTable);
                return generatedType;
            }
        }

    }
}
