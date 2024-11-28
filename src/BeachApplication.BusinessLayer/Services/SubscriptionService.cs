using AutoMapper;
using BeachApplication.BusinessLayer.Resources;
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

public class SubscriptionService(IDataContext dataContext, IUserService userService, IMapper mapper) : ISubscriptionService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var dbSubscription = await dataContext.GetData<Entities.Subscription>(trackingChanges: true).FirstOrDefaultAsync(s => s.Id == id);
        if (dbSubscription is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
        }

        await dataContext.DeleteAsync(dbSubscription);
        await dataContext.SaveAsync();

        return Result.Ok();
    }

    public async Task<Result<Subscription>> GetAsync(Guid id)
    {
        var dbSubscription = await dataContext.GetAsync<Entities.Subscription>(id);
        if (dbSubscription is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
        }

        var subscription = mapper.Map<Subscription>(dbSubscription);
        return subscription;
    }

    public async Task<Result<PaginatedList<Subscription>>> GetListAsync()
    {
        var query = dataContext.GetData<Entities.Subscription>();

        var totalCount = await query.CountAsync();
        var dbSubscriptions = await query.ToListAsync();

        var subscriptions = mapper.Map<IEnumerable<Subscription>>(dbSubscriptions);
        return new PaginatedList<Subscription>(subscriptions, totalCount);
    }

    public async Task<Result<Subscription>> InsertAsync(SaveSubscriptionRequest request)
    {
        var dbSubscription = mapper.Map<Entities.Subscription>(request);
        dbSubscription.UserId = userService.GetUserId();

        await dataContext.InsertAsync(dbSubscription);
        await dataContext.SaveAsync();

        return mapper.Map<Subscription>(dbSubscription);
    }

    public async Task<Result<Subscription>> UpdateAsync(Guid id, SaveSubscriptionRequest request)
    {
        var dbSubscription = await dataContext.GetData<Entities.Subscription>(trackingChanges: true).FirstOrDefaultAsync(s => s.Id == id);
        if (dbSubscription is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
        }

        mapper.Map(request, dbSubscription);
        await dataContext.SaveAsync();

        return mapper.Map<Subscription>(dbSubscription);
    }
}