using AutoMapper;
using ScrapSystem.Api.Application.DTOs.ScrapDtos;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Api.Application.DTOs.UserDtos;
using ScrapSystem.Api.Application.DTOs.MaterialName;
using ScrapSystem.Api.Application.DTOs.ScrapDetailDtos;

namespace ScrapSystem.Api.Services.Profiles
{
    public class MappingProfile : Profile
    {

        /// <summary>
        /// Configures AutoMapper mappings for Scrap-related entities and DTOs.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserLoginDto>().ReverseMap();
            CreateMap<User, UserRegisterDto>().ReverseMap();
            CreateMap<Scrap, ScrapDto>().ReverseMap();
            CreateMap<ScrapDetail, ScrapDetailDto>().ReverseMap();
            CreateMap<ScrapDetailDto, ScrapDetail>().ReverseMap();

            CreateMap<MaterialName, MaterialNameDto>().ReverseMap();
        }
    }
}
