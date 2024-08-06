using System.Web;

namespace BeachApplication.Handlers.Http;

public class QueryStringInjectorHttpMessageHandler(IDictionary<string, string>? parameters = null) : DelegatingHandler
{
    public IDictionary<string, string> Parameters
    {
        get
        {
            return parameters ?? new Dictionary<string, string>();
        }
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var queryStringParameters = HttpUtility.ParseQueryString(request.RequestUri!.Query);
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