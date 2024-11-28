using AutoMapper;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.DataAccessLayer;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class UmbrellaService(IDataContext dataContext, IMapper mapper) : IUmbrellaService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var dbUmbrella = await dataContext.GetData<Entities.Umbrella>().Include(u => u.Reservations).FirstOrDefaultAsync(u => u.Id == id);
        if (dbUmbrella is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No umbrella found with id {id}");
        }

        if (dbUmbrella.Reservations.Count > 0)
        {
            return Result.Fail(FailureReasons.ClientError, "Cannot delete this umbrella because it just belongs to a reservation");
        }

        await dataContext.DeleteAsync(dbUmbrella);
        await dataContext.SaveAsync();

        return Result.Ok();
    }

    public async Task<Result<Umbrella>> GetAsync(Guid id)
    {
        var dbUmbrella = await dataContext.GetAsync<Entities.Umbrella>(id);
        if (dbUmbrella is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No umbrella found with id {id}");
        }

        var umbrella = mapper.Map<Umbrella>(dbUmbrella);
        return umbrella;
    }

    public async Task<Result<PaginatedList<Umbrella>>> GetListAsync(char? letter, int pageIndex, int itemsPerPage)
    {
        var query = dataContext.GetData<Entities.Umbrella>().WhereIf(letter.HasValue, u => u.Letter == letter.Value.ToString());
        var totalCount = await query.CountAsync();

        var dbUmbrellas = await query.OrderBy(u => u.Number).ToListAsync(pageIndex, itemsPerPage);
        var hasNextPage = await query.HasNextPageAsync(pageIndex, itemsPerPage);

        var umbrellas = mapper.Map<IEnumerable<Umbrella>>(dbUmbrellas).Take(itemsPerPage);
        return new PaginatedList<Umbrella>(umbrellas, totalCount, hasNextPage);
    }

    public async Task<Result<Umbrella>> InsertAsync(SaveUmbrellaRequest request)
    {
        var exists = await dataContext.GetData<Entities.Umbrella>().AnyAsync(u => u.Letter == request.Letter.ToString() && u.Number == request.Number);
        if (exists)
        {
            return Result.Fail(FailureReasons.Conflict, "This umbrella already exists");
        }

        var dbUmbrella = mapper.Map<Entities.Umbrella>(request);
        await dataContext.InsertAsync(dbUmbrella);

        await dataContext.SaveAsync();
        return mapper.Map<Umbrella>(dbUmbrella);
    }

    public async Task<Result<Umbrella>> UpdateAsync(Guid id, SaveUmbrellaRequest request)
    {
        var dbUmbrella = await dataContext.GetData<Entities.Umbrella>(trackingChanges: true).FirstOrDefaultAsync(u => u.Id == id);
        if (dbUmbrella is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No umbrella found with id {id}");
        }

        mapper.Map(request, dbUmbrella);
        await dataContext.SaveAsync();

        return mapper.Map<Umbrella>(dbUmbrella);
    }

    public async Task<Result> UpdateStatusAsync(ChangeUmbrellaStatusRequest request)
    {
        var dbUmbrella = await dataContext.GetData<Entities.Umbrella>(trackingChanges: true).FirstOrDefaultAsync(u => u.Id == request.Id);
        if (dbUmbrella is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, $"No umbrella found with id {request.Id}");
        }

        if (dbUmbrella.IsBusy != request.IsBusy)
        {
            dbUmbrella.IsBusy = request.IsBusy;
            await dataContext.SaveAsync();
        }

        return Result.Ok();
    }
}