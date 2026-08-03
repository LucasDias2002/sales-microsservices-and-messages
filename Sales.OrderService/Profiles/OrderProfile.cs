using AutoMapper;
using Sales.OrderService.DTOs;
using Sales.OrderService.Entities;

namespace Sales.OrderService.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDTO>().ReverseMap();
        }
    }
}
