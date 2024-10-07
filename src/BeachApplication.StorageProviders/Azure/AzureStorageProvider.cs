using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace BeachApplication.StorageProviders.Azure;

public class AzureStorageProvider(BlobServiceClient blobServiceClient, AzureStorageOptions options) : IStorageProvider
{
    public async Task DeleteAsync(string path)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);
        await blobContainerClient.DeleteBlobIfExistsAsync(path);
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

        var stream = await blobClient.OpenReadAsync();
        return stream;
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
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);

        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        }

        return blobContainerClient.GetBlobClient(path);
    }
}