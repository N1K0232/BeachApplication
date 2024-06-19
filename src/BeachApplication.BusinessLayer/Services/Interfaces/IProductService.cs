using BeachApplication.Shared.Collections;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services;

public interface IProductService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Product>> GetAsync(Guid id);

    Task<Result<ListResult<Product>>> GetListAsync(string name, string category, int pageIndex, int itemsPerPage, string orderBy);

    Task<Result<Product>> InsertAsync(SaveProductRequest request);

    Task<Result<Product>> UpdateAsync(Guid id, SaveProductRequest request);
}