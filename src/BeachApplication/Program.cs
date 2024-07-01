using Serilog;

namespace BeachApplication;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = CreateBuilder(args);
        var host = builder.Build();

        using var cancellationTokenSource = new CancellationTokenSource();
        await host.RunAsync(cancellationTokenSource.Token);
    }

    private static IHostBuilder CreateBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(options =>
            {
                options.AddJsonFile("appsettings.local.json", true, true);
            })
            .ConfigureWebHostDefaults(options =>
            {
                options.UseStartup<Startup>();
            })
            .UseSerilog(ConfigureLogger);

        return builder;
    }

    private static void ConfigureLogger(HostBuilderContext context, LoggerConfiguration configuration)
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    }
}