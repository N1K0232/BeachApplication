namespace BeachApplication.BusinessLayer.Settings;

public class AppSettings
{
    public string ApplicationName { get; init; } = null!;

    public string ApplicationDescription { get; init; } = null!;

    public string? ContainerName { get; init; }

    public string StorageFolder { get; init; } = null!;

    public string[] SupportedCultures { get; init; } = null!;
}