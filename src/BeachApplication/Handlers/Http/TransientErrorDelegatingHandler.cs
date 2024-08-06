using Polly.Registry;

namespace BeachApplication.Handlers.Http;

public class TransientErrorDelegatingHandler(ResiliencePipelineProvider<string> provider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var pipeline = provider.GetPipeline<HttpResponseMessage>("http");
        return await pipeline.ExecuteAsync(async token => await base.SendAsync(request, token), cancellationToken);
    }
}