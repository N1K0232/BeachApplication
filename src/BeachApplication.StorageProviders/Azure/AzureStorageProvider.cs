using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace BeachApplication.StorageProviders.Azure;

public class AzureStorageProvider(AzureStorageOptions options) : IStorageProvider
{
    private readonly BlobServiceClient blobServiceClient = new(options.ConnectionString);

    public async Task DeleteAsync(string path)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);
        await blobContainerClient.DeleteBlobIfExistsAsync(path);
    }

    public async Task<bool> ExistsAsync(string path)
    {
        var blobClient = await GetBlobClientAsync(path, false);
        return await blobClient.ExistsAsync();
    }

    public async Task<Stream?> ReadAsStreamAsync(string path)
    {
        var blobClient = await GetBlobClientAsync(path);
        var stream = await blobClient.OpenReadAsync();

        if (stream is not null && stream.Position != 0)
        {
            stream.Position = 0;
        }

        return stream;
    }

    public async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        var blobClient = await GetBlobClientAsync(path, true);

        if (!overwrite)
        {
            var exists = await blobClient.ExistsAsync();
            if (exists)
            {
                throw new IOException($"The file {path} already exists");
            }
        }

        var headers = new BlobHttpHeaders
        {
            ContentType = MimeUtility.GetMimeMapping(path)
        };

        stream.Position = 0;
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