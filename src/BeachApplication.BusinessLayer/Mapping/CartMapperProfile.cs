using AutoMapper;
using BeachApplication.Shared.Models;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class CartMapperProfile : Profile
{
    public CartMapperProfile()
    {
        CreateMap<Entities.Cart, Cart>();
        CreateMap<Entities.CartItem, CartItem>();
    }
}