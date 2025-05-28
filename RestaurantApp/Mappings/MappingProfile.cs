using AutoMapper;
using RestaurantApp.DTOs;
using RestaurantApp.Models;

namespace RestaurantApp.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDTO>();

            CreateMap<RegisterDTO, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.IsAccountActivated, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => 0));

            CreateMap<Dish, CreateDishDTO>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name)).ReverseMap();    
        }
    }
}
