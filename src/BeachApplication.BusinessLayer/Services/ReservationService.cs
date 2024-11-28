using System.Linq.Dynamic.Core;
using AutoMapper;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class ReservationService(IDataContext dataContext, IUserService userService, IMapper mapper) : IReservationService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var dbReservation = await dataContext.GetData<Entities.Reservation>(trackingChanges: true).FirstOrDefaultAsync(u => u.Id == id);
        if (dbReservation is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No reservation found with id {id}");
        }

        await dataContext.DeleteAsync(dbReservation);
        await dataContext.SaveAsync();

        return Result.Ok();
    }

    public async Task<Result<Reservation>> GetAsync(Guid id)
    {
        var dbReservation = await dataContext.GetAsync<Entities.Reservation>(id);
        if (dbReservation is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No reservation found with id {id}");
        }

        var reservation = mapper.Map<Reservation>(dbReservation);
        return reservation;
    }

    public async Task<Result<PaginatedList<Reservation>>> GetListAsync(DateOnly? reservationDate, int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = dataContext.GetData<Entities.Reservation>()
            .Include(r => r.Umbrella)
            .WhereIf(reservationDate.HasValue, r => r.StartOn == reservationDate || r.EndsOn == reservationDate);

        var totalCount = await query.CountAsync();
        var dbReservations = await query.OrderBy(orderBy).ToListAsync(pageIndex, itemsPerPage);

        var hasNextPage = await query.HasNextPageAsync(pageIndex, itemsPerPage);
        var reservations = mapper.Map<IEnumerable<Reservation>>(dbReservations).Take(itemsPerPage);

        return new PaginatedList<Reservation>(reservations, totalCount, hasNextPage);
    }

    public async Task<Result<Reservation>> InsertAsync(SaveReservationRequest request)
    {
        var exists = await ReservationExistsAsync(request);
        if (!exists)
        {
            return Result.Fail(FailureReasons.Conflict, "This reservation already exists");
        }

        var umbrella = await GetUmbrellaAsync(request.Letter, request.Number);
        if (umbrella is null)
        {
            return Result.Fail(FailureReasons.ClientError, "No umbrella found");
        }

        if (umbrella.IsBusy)
        {
            return Result.Fail(FailureReasons.ClientError, "Sorry but this umbrella is already taken");
        }

        var dbReservation = mapper.Map<Entities.Reservation>(request);
        dbReservation.UserId = userService.GetUserId();

        umbrella.IsBusy = true;
        await dataContext.InsertAsync(dbReservation);

        await dataContext.SaveAsync();
        return mapper.Map<Reservation>(dbReservation);
    }

    public async Task<Result<Reservation>> UpdateAsync(Guid id, SaveReservationRequest request)
    {
        var umbrella = await GetUmbrellaAsync(request.Letter, request.Number);
        if (umbrella is null)
        {
            return Result.Fail(FailureReasons.ClientError, "No umbrella found");
        }

        if (umbrella.IsBusy)
        {
            return Result.Fail(FailureReasons.ClientError, "Sorry but this umbrella is already taken");
        }

        var dbReservation = await dataContext.GetData<Entities.Reservation>(trackingChanges: true).FirstOrDefaultAsync(r => r.Id == id);
        if (dbReservation is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No reservation found with id {id}");
        }

        dbReservation.UserId = userService.GetUserId();
        mapper.Map(request, dbReservation);

        await dataContext.SaveAsync();
        return mapper.Map<Reservation>(dbReservation);
    }

    private async Task<Entities.Umbrella> GetUmbrellaAsync(char letter, int number)
    {
        var umbrella = await dataContext.GetData<Entities.Umbrella>(trackingChanges: true)
            .FirstOrDefaultAsync(u => u.Letter == letter.ToString() && u.Number == number);

        return umbrella;
    }

    private async Task<bool> ReservationExistsAsync(SaveReservationRequest request)
    {
        var query = dataContext.GetData<Entities.Reservation>();
        var userId = userService.GetUserId();

        var exists = await query.AnyAsync(r => r.UserId == userId && r.StartAt == request.StartAt &&
            r.EndsOn == request.EndsOn && r.EndsAt == request.EndsAt);

        return exists;
    }
}