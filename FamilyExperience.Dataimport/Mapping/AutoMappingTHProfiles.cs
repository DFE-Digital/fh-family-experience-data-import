using AutoMapper;
using FamilyExperience.Dataimport.Models.API;
using FamilyExperience.Dataimport.Models.Json;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralContacts;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralCostOptions;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralHolidaySchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralOrganisations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhones;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralPhysicalAddresses;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralRegularSchedule;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServiceAtLocations;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.OpenReferralServices;
using FamilyHubs.ServiceDirectory.Shared.Models.Api.ServiceType;

namespace FamilyExperience.Dataimport.Mapping
{
    public class AutoMappingTHProfiles:Profile
    {
        public AutoMappingTHProfiles()
        {
            CreateMap<ContactNumbers, OpenReferralPhoneDto>();
            CreateMap<ContactNumbers, OpenReferralPhoneDto>();


            CreateMap<ContactDetails, OpenReferralContactDto>()
                .ForMember(d => d.Id, opts => opts.MapFrom(s => s.Id))
               .ForMember(d => d.Name, opts => opts.MapFrom(s => s.Name))
               .ForMember(d => d.Phones, opts => opts.MapFrom(s => s.Numbers));

            CreateMap<CostOption, OpenReferralCostOptionDto>()
                .ForMember(d => d.Amount, opts => opts.MapFrom(s => s.Amount))
                .ForMember(d => d.Amount_description, opts => opts.MapFrom(s => s.Amount_description))
                .ForMember(d => d.Id, opts => opts.MapFrom(s => s.Id));

            CreateMap<Location, OpenReferralLocationDto>()
                .ForMember(d => d.Id, opts => opts.MapFrom(s => s.Id))
                .ForMember(d => d.Name, opts => opts.MapFrom(s => s.Name))
                .ForMember(d => d.Physical_addresses, opts => opts.MapFrom(s => s.PhysicalAddresses));

            CreateMap<RegularSchedule, OpenReferralRegularScheduleDto>()
                .ForMember(d => d.Id, opts => opts.MapFrom(s => s.Id))
                .ForMember(d => d.Byday, opts => opts.MapFrom(s => s.Byday))
                .ForMember(d => d.Opens_at, opts => opts.MapFrom(s => s.OpensAt))
                .ForMember(d => d.Closes_at, opts => opts.MapFrom(s => s.ClosesAt));



            CreateMap<HolidaySchedule, OpenReferralHolidayScheduleDto>();

            CreateMap<PhysicalAddress, OpenReferralPhysicalAddressDto>();

            CreateMap<ServiceAtLocation, OpenReferralServiceAtLocationDto>()
                .ForMember(d => d.Location, opts => opts.MapFrom(s => s.LocationDetails))
                .ForMember(d => d.Id, opts => opts.MapFrom(s => s.Id))
                .ForMember(d => d.HolidayScheduleCollection, opts => opts.MapFrom(s => s.HolidaySchedules))
                .ForMember(d => d.Regular_schedule, opts => opts.MapFrom(s => s.RegularSchedules));

            CreateMap<ServiceType, ServiceTypeDto>();

            CreateMap<THService, OpenReferralServiceDto>()
              .ForMember(d => d.Description, opts => opts.MapFrom(s => s.ServiceDescription))
              .ForMember(d => d.OpenReferralOrganisationId, opts => opts.MapFrom(s => s.OrganisationId))
              .ForMember(d => d.Name, opts => opts.MapFrom(s => s.ServiceName))
              .ForMember(d => d.Email, opts => opts.MapFrom(s => s.Email))
              .ForMember(d => d.Contacts, opts => opts.MapFrom(s => s.ContactDetails))
              .ForMember(d => d.Url, opts => opts.MapFrom(s => s.Url))
              .ForMember(d => d.Cost_options, opts => opts.MapFrom(s => s.CostOptions))
              .ForMember(d => d.Service_at_locations, opts => opts.MapFrom(s => s.ServiceAtLocations))
              .ForMember(d => d.CanFamilyChooseDeliveryLocation, opts => opts.MapFrom(s => s.CanFamilyChooseDeliveryLocation));

            CreateMap<THRoot, OpenReferralOrganisationWithServicesDto>()
              .ForMember(d => d.Services, opts => opts.MapFrom(s => s.services));


        }
    }
}
