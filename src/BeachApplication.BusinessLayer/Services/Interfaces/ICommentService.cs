using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface ICommentService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Comment>> GetAsync();

    Task<Result<Comment>> GetAsync(Guid id);

    Task<Result<PaginatedList<Comment>>> GetListAsync(int pageIndex, int itemsPerPage);

    Task<Result<Comment>> InsertAsync(SaveCommentRequest request);

    Task<Result<Comment>> UpdateAsync(Guid id, SaveCommentRequest request);
}