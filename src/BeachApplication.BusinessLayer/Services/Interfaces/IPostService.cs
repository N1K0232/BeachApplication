using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IPostService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Post>> GetAsync(Guid id);

    Task<Result<IEnumerable<Post>>> GetListAsync();

    Task<Result<Post>> InsertAsync(SavePostRequest request);

    Task<Result<Post>> UpdateAsync(Guid id, SavePostRequest request);
}