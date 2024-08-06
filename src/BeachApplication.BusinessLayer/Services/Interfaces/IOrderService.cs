using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IOrderService
{
    Task<Result<Order>> AddOrderDetailAsync(SaveOrderRequest request);

    Task<Result<Order>> CreateAsync();

    Task<Result> DeleteAsync(Guid id);

    Task<Result<Order>> GetAsync(Guid id);

    Task<Result<PaginatedList<Order>>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy);
}