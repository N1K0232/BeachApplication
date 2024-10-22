using AutoMapper;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Mapping;

public class ReservationMapperProfile : Profile
{
    public ReservationMapperProfile()
    {
        CreateMap<Entities.Reservation, Reservation>();
        CreateMap<SaveReservationRequest, Entities.Reservation>();
    }
}