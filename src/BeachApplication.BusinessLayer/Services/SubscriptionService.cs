using AutoMapper;
using AutoMapper.QueryableExtensions;
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

public class SubscriptionService(IApplicationDbContext db, IUserService userService, IMapper mapper) : ISubscriptionService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var query = db.GetData<Entities.Subscription>(trackingChanges: true);
        var subscription = await query.FirstOrDefaultAsync(s => s.Id == id);

        if (subscription is not null)
        {
            await db.DeleteAsync(subscription);
            await db.SaveAsync();

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
    }

    public async Task<Result<Subscription>> GetAsync(Guid id)
    {
        var dbSubscription = await db.GetAsync<Entities.Subscription>(id);
        if (dbSubscription is not null)
        {
            var subscription = mapper.Map<Subscription>(dbSubscription);
            return subscription;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
    }

    public async Task<Result<PaginatedList<Subscription>>> GetListAsync(string userName)
    {
        var query = db.GetData<Entities.Subscription>().Include(s => s.User).AsQueryable();

        if (userName.HasValue())
        {
            query = query.Where(s => s.User.NormalizedUserName == userName.ToUpperInvariant());
        }

        var totalCount = await query.CountAsync();
        var subscriptions = await query.ProjectTo<Subscription>(mapper.ConfigurationProvider).ToListAsync();

        return new PaginatedList<Subscription>(subscriptions, totalCount);
    }

    public async Task<Result<Subscription>> InsertAsync(SaveSubscriptionRequest request)
    {
        var subscription = mapper.Map<Entities.Subscription>(request);
        subscription.UserId = await userService.GetIdAsync();

        await db.InsertAsync(subscription);
        await db.SaveAsync();

        return mapper.Map<Subscription>(subscription);
    }

    public async Task<Result<Subscription>> UpdateAsync(Guid id, SaveSubscriptionRequest request)
    {
        var query = db.GetData<Entities.Subscription>(trackingChanges: true);
        var subscription = await query.FirstOrDefaultAsync(s => s.Id == id);

        if (subscription is not null)
        {
            mapper.Map(request, subscription);
            await db.SaveAsync();

            return mapper.Map<Subscription>(subscription);
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
    }
}