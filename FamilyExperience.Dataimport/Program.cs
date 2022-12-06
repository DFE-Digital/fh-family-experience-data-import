

namespace FamilyExperience.Dataimport
{
    class Program
    {

        static  void Main()
        {
            //TransformSalfordData transformData = new TransformSalfordData();
            //transformData.ProcessDataAsync().Wait();

            //TransformTHData transformTHData = new TransformTHData();
            //transformTHData.ProcessDataAsync().Wait();


            var transformData = new TransformData();
            transformData.ProcessDataAsync().Wait();
        }
           
    }
        
    } 


   

