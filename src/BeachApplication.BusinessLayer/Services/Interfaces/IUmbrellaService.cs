using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IUmbrellaService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Umbrella>> GetAsync(Guid id);

    Task<Result<PaginatedList<Umbrella>>> GetListAsync(char? letter, int pageIndex, int itemsPerPage);

    Task<Result<Umbrella>> InsertAsync(SaveUmbrellaRequest request);

    Task<Result<Umbrella>> UpdateAsync(Guid id, SaveUmbrellaRequest request);

    Task<Result> UpdateStatusAsync(ChangeUmbrellaStatusRequest request);
}