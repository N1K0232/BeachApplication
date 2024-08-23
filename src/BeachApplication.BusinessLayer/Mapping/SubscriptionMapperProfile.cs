using AutoMapper;
using BeachApplication.Shared.Enums;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class SubscriptionMapperProfile : Profile
{
    public SubscriptionMapperProfile()
    {
        CreateMap<Entities.Subscription, Subscription>();
        CreateMap<SaveSubscriptionRequest, Entities.Subscription>()
            .ForMember(s => s.Status, options => options.MapFrom(s => s.Status.GetValueOrDefault(SubscriptionStatus.Active)));
    }
}