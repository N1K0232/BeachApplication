using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface ICartService
{
    Task<Result<Order>> ConfirmAsync(Guid id);

    Task<Result> DeleteAsync(Guid id);

    Task<Result<Cart>> GetAsync();

    Task<Result> RemoveItemAsync(Guid id, Guid itemId);

    Task<Result<Cart>> SaveAsync(SaveCartRequest request);
}