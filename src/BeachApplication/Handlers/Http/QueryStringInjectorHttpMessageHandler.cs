using System.Web;

namespace BeachApplication.Handlers.Http;

public class QueryStringInjectorHttpMessageHandler : DelegatingHandler
{
    public QueryStringInjectorHttpMessageHandler(IDictionary<string, string> parameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, string>();
    }

    public IDictionary<string, string> Parameters { get; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var queryStringParameters = HttpUtility.ParseQueryString(request.RequestUri.Query);
        foreach (var parameter in Parameters)
        {
            queryStringParameters.Add(parameter.Key, parameter.Value);
        }

        var uriBuilder = new UriBuilder(request.RequestUri)
        {
            Query = queryStringParameters.ToString()
        };

        request.RequestUri = new Uri(uriBuilder.ToString());
        return base.SendAsync(request, cancellationToken);
    }
}