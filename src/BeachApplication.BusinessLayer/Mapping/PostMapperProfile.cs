using AutoMapper;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class PostMapperProfile : Profile
{
    public PostMapperProfile()
    {
        CreateMap<Entities.Post, Post>();
        CreateMap<SavePostRequest, Entities.Post>();
    }
}