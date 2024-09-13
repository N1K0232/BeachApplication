using Microsoft.AspNetCore.Identity;

namespace BeachApplication.DataAccessLayer.Extensions;

public static class IdentityExtensions
{
    public static string GetErrors(this IdentityResult result)
        => string.Join(",", result.Errors.Select(e => e.Description));
}