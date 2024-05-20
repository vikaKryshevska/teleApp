using AutoMapper;
using UserService.Models;
using Models.Entities;

namespace UserService
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterRequestModel, Subscriber>().ReverseMap();
        }
    }
}
