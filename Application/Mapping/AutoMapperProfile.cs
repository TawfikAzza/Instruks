using Application.DTOs;
using AutoMapper;
using Domain;

namespace Application.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Product Mappings
        CreateMap<PostProductDTO, Product>();
        CreateMap<UpdateProductDTO, Product>();
        CreateMap<Product, ProductResultDTO>();

        // Order Mappings
        CreateMap<PostOrderDTO, Order>();
        CreateMap<UpdateOrderDTO, Order>();
        CreateMap<Order, OrderResultDTO>();
    }
}