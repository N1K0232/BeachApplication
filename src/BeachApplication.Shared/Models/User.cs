using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class User : BaseObject
{
    public string FirstName { get; init; } = string.Empty;

    public string? LastName { get; init; }

    public string Email { get; init; } = string.Empty;
}