namespace BeachApplication.BusinessLayer.Providers.Interfaces;

public interface IAzureTokenProvider : IDisposable
{
    string Region { get; set; }

    string SubscriptionKey { get; set; }

    Task<string> GetAccessTokenAsync();
}