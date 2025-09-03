using AutoMapper;
using Shared.DTOs;
using Shared.Models;

namespace SalesService.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        
        CreateMap<OrderItem, OrderItemDto>();
        
        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());
            
        CreateMap<CreateOrderItemDto, OrderItem>()
            .ForMember(dest => dest.ProductName, opt => opt.Ignore())
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());
    }
}
