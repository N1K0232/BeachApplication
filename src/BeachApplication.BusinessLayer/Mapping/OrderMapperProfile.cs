using AutoMapper;
using BeachApplication.Shared.Models;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class OrderMapperProfile : Profile
{
    public OrderMapperProfile()
    {
        CreateMap<Entities.Order, Order>()
            .ForMember(o => o.User, options => options.MapFrom(order => order.User.Email));

        CreateMap<Entities.OrderDetail, OrderDetail>();
    }
}