using AutoMapper;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class CommentMapperProfile : Profile
{
    public CommentMapperProfile()
    {
        CreateMap<Entities.Comment, Comment>();
        CreateMap<SaveCommentRequest, Entities.Comment>();
    }
}