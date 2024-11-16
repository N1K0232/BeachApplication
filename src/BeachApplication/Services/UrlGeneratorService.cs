using BeachApplication.Contracts;

namespace BeachApplication.Services;

public class UrlGeneratorService(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor) : IUrlGeneratorService
{
    public Task<string> GetPageUrlAsync(string page, object values = null)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var url = linkGenerator.GetUriByPage(httpContext, page, null, values);

        return Task.FromResult(url);
    }
}