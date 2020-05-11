using AutoMapper;
using app_api.V1.DTO.InputDTOs.UserDTOs;
using app_api.Data.Models;

namespace app_api
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<NewUserDTO, User>()
                .ForMember(dest => dest.Username, origin => origin.MapFrom(ev => ev.NewUserUsername));
            CreateMap<NewServiceDTO, User>()
                .ForMember(dest => dest.Username, origin => origin.MapFrom(ev => ev.NewServiceName + "/" + ev.NewServiceHost));
        }
    }
}