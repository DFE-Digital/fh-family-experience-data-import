using FamilyExperience.DataImport.Service;

namespace FamilyExperience.DataImport
{
    public class Program
    {
        public static async Task Main()
        {
            var transformData = new TransformDataService();
            await transformData.ProcessDataAsync();
        }
    }
}