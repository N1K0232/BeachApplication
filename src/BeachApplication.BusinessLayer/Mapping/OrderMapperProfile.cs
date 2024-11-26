using AutoMapper;
using BeachApplication.Shared.Models;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class OrderMapperProfile : Profile
{
    public OrderMapperProfile()
    {
        CreateMap<Entities.Order, Order>();
        CreateMap<Entities.OrderDetail, OrderDetail>();
    }
}