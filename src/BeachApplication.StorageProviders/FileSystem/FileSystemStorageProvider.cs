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

    public Task<Stream> ReadAsStreamAsync(string path)
    {
        var fullPath = CreatePath(path);
        var exists = File.Exists(fullPath);

        if (!exists)
        {
            return Task.FromResult<Stream>(null);
        }

        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream>(stream);
    }

    public async Task SaveAsync(Stream stream, string path)
    {
        var fileStream = await OpenAsync(path);

        if (stream.Position != 0)
        {
            stream.Position = 0;
        }

        await stream.CopyToAsync(fileStream);
        fileStream.Close();
    }

    private Task<Stream> OpenAsync(string path)
    {
        var fullPath = CreatePath(path);
        var directoryName = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        var stream = File.OpenWrite(fullPath);
        return Task.FromResult<Stream>(stream);
    }

    private string CreatePath(string path) => Path.Combine(options.StorageFolder, path);
}