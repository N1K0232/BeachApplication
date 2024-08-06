using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface ICategoryService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Category>> GetAsync(Guid id);

    Task<Result<IEnumerable<Category>>> GetListAsync(string? name);

    Task<Result<Category>> InsertAsync(SaveCategoryRequest request);

    Task<Result<Category>> UpdateAsync(Guid id, SaveCategoryRequest request);
}