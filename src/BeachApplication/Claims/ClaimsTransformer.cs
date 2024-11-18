using System.Reflection;
using System.Security.Claims;
using BeachApplication.Extensions;
using Microsoft.AspNetCore.Authentication;

namespace BeachApplication.Claims;

public class ClaimsTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetAttribute<BuildDateTimeAttribute>(a => a.DateTime.ToString("yyyyMMdd.HHmm"));

        var identity = principal.Identity as ClaimsIdentity;
        identity.AddClaim(new Claim(ClaimTypes.Version, version));

        return Task.FromResult(principal);
    }
}