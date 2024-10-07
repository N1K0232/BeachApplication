namespace BeachApplication.StorageProviders;

public interface IStorageProvider
{
    Task DeleteAsync(string path);

    Task<Stream> ReadAsStreamAsync(string path);

    Task SaveAsync(Stream stream, string fileName);
}