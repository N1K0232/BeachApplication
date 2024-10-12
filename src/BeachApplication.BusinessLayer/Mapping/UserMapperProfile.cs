using AutoMapper;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;

namespace BeachApplication.BusinessLayer.Mapping;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(user => user.UserName, options => options.MapFrom(request => request.Email));

        CreateMap<ApplicationUser, User>();
    }
}