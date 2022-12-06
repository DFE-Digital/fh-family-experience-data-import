using FamilyExperience.Dataimport.Service;

namespace FamilyExperience.Dataimport
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