using Azure.Storage.Blobs;
using BeachApplication.StorageProviders.Azure;
using BeachApplication.StorageProviders.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace BeachApplication.StorageProviders.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureStorage(this IServiceCollection services, Action<AzureStorageOptions> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var options = new AzureStorageOptions();
        configuration.Invoke(options);

        services.AddSingleton(options);
        return AddAzureStorageCore(services);
    }

    public static IServiceCollection AddFileSystemStorage(this IServiceCollection services, Action<FileSystemStorageOptions> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var options = new FileSystemStorageOptions();
        configuration.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<IStorageProvider, FileSystemStorageProvider>();

        return services;
    }

    private static IServiceCollection AddAzureStorageCore(IServiceCollection services)
    {
        services.AddScoped(services =>
        {
            var options = services.GetRequiredService<AzureStorageOptions>();
            return new BlobServiceClient(options.ConnectionString);
        });

        return services;
    }
}