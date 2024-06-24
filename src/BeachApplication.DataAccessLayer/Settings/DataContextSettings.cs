namespace BeachApplication.DataAccessLayer.Settings;

public class DataContextSettings
{
    public int CommandTimeout { get; init; }

    public int MaxRetryCount { get; init; }

    public TimeSpan MaxRetryDelay { get; init; }
}