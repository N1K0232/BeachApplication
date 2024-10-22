using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IReservationService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Reservation>> GetAsync(Guid id);

    Task<Result<PaginatedList<Reservation>>> GetListAsync(DateOnly? reservationDate, int pageIndex, int itemsPerPage, string orderBy);

    Task<Result<Reservation>> InsertAsync(SaveReservationRequest request);

    Task<Result<Reservation>> UpdateAsync(Guid id, SaveReservationRequest request);
}