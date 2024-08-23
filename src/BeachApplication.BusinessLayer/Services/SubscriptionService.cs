using AutoMapper;
using AutoMapper.QueryableExtensions;
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

public class SubscriptionService(IApplicationDbContext context, ISqlClientCache cache, IUserService userService, IMapper mapper) : ISubscriptionService
{
    public async Task<Result> DeleteAsync(Guid id)
    {
        var query = context.GetData<Entities.Subscription>(trackingChanges: true);
        var subscription = await query.FirstOrDefaultAsync(s => s.Id == id);

        if (subscription is not null)
        {
            await context.DeleteAsync(subscription);
            await context.SaveAsync();

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
    }

    public async Task<Result<Subscription>> GetAsync(Guid id)
    {
        var dbSubscription = await context.GetAsync<Entities.Subscription>(id);
        if (dbSubscription is not null)
        {
            var subscription = mapper.Map<Subscription>(dbSubscription);
            return subscription;
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
    }

    public async Task<Result<PaginatedList<Subscription>>> GetListAsync(string? userName)
    {
        var query = context.GetData<Entities.Subscription>().Include(s => s.User).AsQueryable();

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

        await context.InsertAsync(subscription);
        await context.SaveAsync();

        await cache.SetAsync(subscription, TimeSpan.FromHours(1));
        return mapper.Map<Subscription>(subscription);
    }

    public async Task<Result<Subscription>> UpdateAsync(Guid id, SaveSubscriptionRequest request)
    {
        var query = context.GetData<Entities.Subscription>(trackingChanges: true);
        var subscription = await query.FirstOrDefaultAsync(s => s.Id == id);

        if (subscription is not null)
        {
            mapper.Map(request, subscription);
            await context.SaveAsync();

            return mapper.Map<Subscription>(subscription);
        }

        return Result.Fail(FailureReasons.ItemNotFound, string.Format(ErrorMessages.ItemNotFound, "Subscription", id));
    }
}