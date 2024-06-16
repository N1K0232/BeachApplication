using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace BeachApplication.StorageProviders.Azure;

public class AzureStorageClient : IStorageClient
{
    private readonly BlobServiceClient client;
    private readonly AzureStorageOptions options;

    public AzureStorageClient(BlobServiceClient client, AzureStorageOptions options)
    {
        this.client = client;
        this.options = options;
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = client.GetBlobContainerClient(options.ContainerName);
        await blobContainerClient.DeleteBlobIfExistsAsync(path, cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(path, false, cancellationToken);
        return await blobClient.ExistsAsync(cancellationToken);
    }

    public async Task<Stream?> ReadAsStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(path, false, cancellationToken);
        var exists = await blobClient.ExistsAsync(cancellationToken);

        if (!exists)
        {
            return null;
        }

        var stream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
        return stream;
    }

    public async Task SaveAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(path, true, cancellationToken);
        var headers = new BlobHttpHeaders
        {
            ContentType = MimeUtility.GetMimeMapping(path)
        };

        stream.Position = 0;
        await blobClient.UploadAsync(stream, headers, cancellationToken: cancellationToken);
    }

    private async Task<BlobClient> GetBlobClientAsync(string path, bool createIfNotExists = false, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = client.GetBlobContainerClient(options.ContainerName);

        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        }

        return blobContainerClient.GetBlobClient(path);
    }
}