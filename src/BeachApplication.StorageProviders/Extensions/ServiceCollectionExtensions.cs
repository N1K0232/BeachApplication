using Azure.Storage.Blobs;
using BeachApplication.StorageProviders.Azure;
using BeachApplication.StorageProviders.Caching;
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

        services.AddScoped(_ => options);
        services.AddScoped(_ => new BlobServiceClient(options.ConnectionString));

        services.AddScoped<IStorageClient, AzureStorageClient>();
        return AddStorageProvider(services);
    }

    public static IServiceCollection AddFileSystemStorage(this IServiceCollection services, Action<FileSystemStorageOptions> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var options = new FileSystemStorageOptions();
        configuration.Invoke(options);

        services.AddScoped(_ => options);
        services.AddScoped<IStorageClient, FileSystemStorageClient>();

        return AddStorageProvider(services);
    }

    private static IServiceCollection AddStorageProvider(IServiceCollection services)
    {
        services.AddScoped<IStorageProvider, StorageProvider>();
        services.AddSingleton<IStorageCache, StorageCache>();

        return services;
    }
}