using AutoMapper;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class UmbrellaMapperProfile : Profile
{
    public UmbrellaMapperProfile()
    {
        CreateMap<Entities.Umbrella, Umbrella>();
        CreateMap<SaveUmbrellaRequest, Entities.Umbrella>()
            .ForMember(u => u.Letter, options => options.MapFrom(request => request.Letter.ToString()));
    }
}