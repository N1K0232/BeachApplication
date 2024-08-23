using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface ISubscriptionService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Subscription>> GetAsync(Guid id);

    Task<Result<PaginatedList<Subscription>>> GetListAsync(string? userName);

    Task<Result<Subscription>> InsertAsync(SaveSubscriptionRequest request);

    Task<Result<Subscription>> UpdateAsync(Guid id, SaveSubscriptionRequest request);
}