namespace BeachApplication.StorageProviders.FileSystem;

public class FileSystemStorageProvider(FileSystemStorageOptions options) : IStorageProvider
{
    public Task DeleteAsync(string path)
    {
        var fullPath = CreatePath(path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string path)
    {
        var fullPath = CreatePath(path);
        var exists = File.Exists(fullPath);

        return Task.FromResult(exists);
    }

    public Task<Stream?> ReadAsStreamAsync(string path)
    {
        var fullPath = CreatePath(path);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream?>(stream);
    }

    public async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        await CreateDirectoryAsync(path);
        var outputStream = await CreateFileStreamAsync(path);

        stream.Position = 0;

        await stream.CopyToAsync(outputStream);
        await outputStream.DisposeAsync();
    }

    private Task<Stream> CreateFileStreamAsync(string path)
    {
        var stream = File.OpenWrite(CreatePath(path));
        return Task.FromResult<Stream>(stream);
    }

    private Task CreateDirectoryAsync(string path)
    {
        var fullPath = CreatePath(path);
        var directoryName = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        return Task.CompletedTask;
    }

    private string CreatePath(string path)
        => Path.Combine(options.StorageFolder, path);
}