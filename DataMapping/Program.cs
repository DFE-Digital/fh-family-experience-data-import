

namespace DataMapping
{
    class Program
    {

        static  void Main()
        {
            TransformSalfordData transformData = new TransformSalfordData();
           transformData.ProcessDataAsync().Wait();
        }
           
    }
        
    } 


   

