namespace BeachApplication.BusinessLayer.Settings;

public class SwaggerSettings
{
    public bool Enabled { get; init; }

    public string Title { get; init; } = null!;

    public string Version { get; init; } = null!;

    public string UserName { get; init; } = null!;

    public string Password { get; init; } = null!;
}