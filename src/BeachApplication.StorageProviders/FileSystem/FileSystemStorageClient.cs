namespace BeachApplication.StorageProviders.FileSystem;

public class FileSystemStorageClient : IStorageClient
{
    private readonly FileSystemStorageOptions options;

    public FileSystemStorageClient(FileSystemStorageOptions options)
    {
        this.options = options;
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = CreatePath(path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = CreatePath(path);
        var exists = File.Exists(fullPath);

        return Task.FromResult(exists);
    }

    public Task<Stream?> ReadAsStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = CreatePath(path);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream?>(stream);
    }

    public async Task SaveAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        var fullPath = CreatePath(path);
        var directoryName = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        using var outputStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        stream.Position = 0;

        await stream.CopyToAsync(outputStream, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    private string CreatePath(string path)
    {
        var fullPath = Path.Combine(options.StorageFolder, path);
        return fullPath;
    }
}