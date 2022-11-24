using AutoMapper;
using DataMapping.Mapping;
using System.Data;
using ExcelApp = Microsoft.Office.Interop.Excel;

namespace DataMapping
{
    public class TransformTHData
    {

        public async Task ProcessDataAsync()
        {
            try
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new AutoMappingProfiles());

                });
                var mapper = new Mapper(config);


                ExcelApp.Application excelApp = new();
                DataRow myNewRow;
                DataTable myTable;

                if (excelApp == null)
                {
                    Console.WriteLine("Excel is not installed!!");
                    return;
                }

                ExcelApp.Workbook excelBook = excelApp.Workbooks.Open(@"D:\DFE\AP_MainSet.xlsx");
                ExcelApp._Worksheet excelSheet = excelBook.Sheets[1];
                ExcelApp.Range excelRange = excelSheet.UsedRange;

                for (int i = 2; i <= excelRange.Rows.Count; i++)
                {

                }


            }
            catch (Exception ex) { }
        }
    }
}
