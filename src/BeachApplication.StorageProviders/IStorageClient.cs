namespace BeachApplication.StorageProviders;

public interface IStorageClient
{
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);

    Task<Stream?> ReadAsStreamAsync(string path, CancellationToken cancellationToken = default);

    Task SaveAsync(string path, Stream stream, CancellationToken cancellationToken = default);
}