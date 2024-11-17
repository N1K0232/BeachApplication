namespace BeachApplication.BusinessLayer.Settings;

public class AppSettings
{
    public string ApplicationId { get; init; }

    public string ApplicationName { get; init; }

    public string ApplicationDescription { get; init; }

    public string ClientId { get; init; }

    public string StorageFolder { get; init; }

    public string[] SupportedCultures { get; init; }
}