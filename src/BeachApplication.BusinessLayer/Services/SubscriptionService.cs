using AutoMapper;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Caching;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using TinyHelpers.Extensions;
using Entities = BeachApplication.DataAccessLayer.Entities;

namespace BeachApplication.BusinessLayer.Services;

public class SubscriptionService(IApplicationDbContext db, ISqlClientCache cache, IUserService userService, IMapper mapper) : ISubscriptionService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var dbSubscription = await db.GetData<Entities.Subscription>(trackingChanges: true).FirstOrDefaultAsync(s => s.Id == id);
        if (dbSubscription is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
        }

        await db.DeleteAsync(dbSubscription);
        await db.SaveAsync();

        await cache.RemoveAsync(id);
        return Result.Ok();
    }

    public async Task<Result<Subscription>> GetAsync(Guid id)
    {
        var cachedSubscription = await cache.GetAsync<Entities.Subscription>(id);
        if (cachedSubscription is not null)
        {
            return mapper.Map<Subscription>(cachedSubscription);
        }

        var dbSubscription = await db.GetAsync<Entities.Subscription>(id);
        if (dbSubscription is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
        }

        var subscription = mapper.Map<Subscription>(dbSubscription);
        return subscription;
    }

    public async Task<Result<PaginatedList<Subscription>>> GetListAsync(string userName)
    {
        var query = db.GetData<Entities.Subscription>().Include(s => s.User).AsQueryable();

        if (userName.HasValue())
        {
            query = query.Where(s => s.User.NormalizedUserName.Equals(userName.ToUpperInvariant()));
        }

        var totalCount = await query.CountAsync();
        var dbSubscriptions = await query.ToListAsync();

        var subscriptions = mapper.Map<IEnumerable<Subscription>>(dbSubscriptions);
        return new PaginatedList<Subscription>(subscriptions, totalCount);
    }

    public async Task<Result<Subscription>> InsertAsync(SaveSubscriptionRequest request)
    {
        var dbSubscription = mapper.Map<Entities.Subscription>(request);
        dbSubscription.UserId = await userService.GetIdAsync();

        await db.InsertAsync(dbSubscription);
        await db.SaveAsync();

        await cache.SetAsync(dbSubscription);
        return mapper.Map<Subscription>(dbSubscription);
    }

    public async Task<Result<Subscription>> UpdateAsync(Guid id, SaveSubscriptionRequest request)
    {
        var dbSubscription = await db.GetData<Entities.Subscription>(trackingChanges: true).FirstOrDefaultAsync(s => s.Id == id);
        if (dbSubscription is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
        }

        mapper.Map(request, dbSubscription);
        await db.SaveAsync();

        return mapper.Map<Subscription>(dbSubscription);
    }
}