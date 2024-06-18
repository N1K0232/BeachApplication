using BeachApplication.BusinessLayer.Exceptions;
using BeachApplication.BusinessLayer.Providers.Interfaces;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Settings;
using Microsoft.Extensions.Options;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Providers;

public class AzureTokenProvider : IAzureTokenProvider
{
    private string region;
    private string subscriptionKey;

    private string token;
    private Uri serviceUrl;

    private CancellationTokenSource cancellationTokenSource;
    private bool disposed = false;

    private readonly HttpClient httpClient;

    public AzureTokenProvider(HttpClient httpClient, IOptions<TranslatorSettings> translatorSettingsOptions)
    {
        this.httpClient = httpClient;
        SubscriptionKey = translatorSettingsOptions.Value.SubscriptionKey;
        Region = translatorSettingsOptions.Value.Region;
    }

    public string Region
    {
        get
        {
            return region;
        }
        set
        {
            if (value != region)
            {
                region = value;
                serviceUrl = new Uri(region.HasValue() ? string.Format(ClientValues.RegionAuthorizationUrl, region) : ClientValues.GlobalAuthorizationUrl);
            }
        }
    }

    public string SubscriptionKey
    {
        get
        {
            return subscriptionKey;
        }
        set
        {
            if (value != subscriptionKey)
            {
                subscriptionKey = value;
                token = null;
            }
        }
    }

    public async Task<string> GetAccessTokenAsync()
    {
        ThrowIfDisposed();
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);
            request.Headers.Add(ClientValues.OcpApimSubscriptionKeyHeader, SubscriptionKey);
            request.Headers.Add(ClientValues.OcpApimSubscriptionRegionHeader, Region);

            using var response = await httpClient.SendAsync(request, cancellationTokenSource.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationTokenSource.Token);
                token = $"Bearer {content}";

                return token;
            }

            throw await HttpClientException.ReadFromResponseAsync(response);
        }
        catch (Exception ex)
        {
            throw new HttpClientException(500, ex.Message);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;

            disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, GetType().FullName);
    }
}