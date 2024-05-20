using AutoMapper;
using Models.Entities;
using TelephoneServices.Models;

namespace TelephoneServices
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ServiceModel, Service>();
            CreateMap<Service, ServiceModel>();
        }
    }
}
