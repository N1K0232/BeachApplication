using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace BeachApplication.StorageProviders.Azure;

public class AzureStorageProvider(BlobServiceClient blobServiceClient, AzureStorageOptions options) : IStorageProvider
{
    public async Task DeleteAsync(string path)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await blobContainerClient.DeleteBlobIfExistsAsync(blobName);
    }

    public async Task<bool> ExistsAsync(string path)
    {
        var blobClient = await GetBlobClientAsync(path);
        return await blobClient.ExistsAsync();
    }

    public async Task<Stream> ReadAsStreamAsync(string path)
    {
        var blobClient = await GetBlobClientAsync(path);
        var exists = await blobClient.ExistsAsync();

        if (!exists)
        {
            return null;
        }

        return await blobClient.OpenReadAsync();
    }

    public async Task SaveAsync(Stream stream, string path)
    {
        stream.Position = 0;
        var headers = new BlobHttpHeaders
        {
            ContentType = MimeUtility.GetMimeMapping(path)
        };

        var blobClient = await GetBlobClientAsync(path, true);
        await blobClient.UploadAsync(stream, headers);
    }

    private async Task<BlobClient> GetBlobClientAsync(string path, bool createIfNotExists = false)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        }

        return blobContainerClient.GetBlobClient(blobName);
    }

    private (string ContainerName, string BlobName) ExtractContainerBlobName(string path)
    {
        var relativePath = path?.Replace(@"\", "/") ?? string.Empty;
        if (!relativePath.StartsWith('/') && !string.IsNullOrWhiteSpace(options.ContainerName))
        {
            return (options.ContainerName, path);
        }

        var root = Path.GetPathRoot(relativePath);
        var fileName = relativePath[(root ?? string.Empty).Length..];

        var parts = fileName.Split('/');
        return (parts.First().ToLowerInvariant(), string.Join('/', parts.Skip(1)));
    }
}