using AutoMapper;
using DataMapping.Mapping;
using FamilyExperience.Dataimport.Helper;


namespace DataMapping
{
    public class TransformTHData
    {

        public void ProcessDataAsync()
        {
            try
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new AutoMappingProfiles());

                });
                var mapper = new Mapper(config);

            }
            catch (Exception ex) { }
        }
    }
}
