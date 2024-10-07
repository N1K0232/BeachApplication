using AutoMapper;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class OrderMapperProfile : Profile
{
    public OrderMapperProfile()
    {
        CreateMap<Entities.Order, Order>()
            .ForMember(o => o.User, options => options.MapFrom(order => order.User.Email));

        CreateMap<Entities.OrderDetail, OrderDetail>();
        CreateMap<SaveOrderRequest, Entities.OrderDetail>()
            .ForMember(o => o.OrderId, options => options.MapFrom(request => request.Id));
    }
}