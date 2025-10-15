using Application.DTOs;
using AutoMapper;
using Domain;

namespace Application.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Instruks, InstruksDto>().ReverseMap();
    }
}