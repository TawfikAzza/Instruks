using Application.DTOs;
using AutoMapper;
using Domain;

namespace Application.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Keep only what the app actually uses now
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Instruks, InstruksDto>().ReverseMap();
    }
}