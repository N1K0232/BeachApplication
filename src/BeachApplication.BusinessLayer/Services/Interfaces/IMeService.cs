using BeachApplication.Shared.Models;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IMeService
{
    Task<Result<User>> GetAsync();
}