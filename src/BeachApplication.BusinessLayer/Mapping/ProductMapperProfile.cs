using AutoMapper;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class ProductMapperProfile : Profile
{
    public ProductMapperProfile()
    {
        CreateMap<Entities.Product, Product>()
            .ForMember(p => p.Category, options => options.MapFrom(p => p.Category.Name));

        CreateMap<SaveProductRequest, Entities.Product>()
            .ForMember(p => p.Category, options => options.Ignore());
    }
}