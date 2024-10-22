using BeachApplication.Shared.Models;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IOrderService
{
    Task<Result> CancelAsync(Guid id);

    Task<Result<Order>> GetAsync();

    Task<Result<PaginatedList<Order>>> GetListAsync(int pageIndex, int itemsPerPage, string orderBy);
}