using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace MOAI.API.Services;

public class AzureBlobService
{
    private readonly BlobContainerClient _container;

    public AzureBlobService(IConfiguration config)
    {
        var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        var containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Missing AZURE_STORAGE_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Missing AZURE_STORAGE_CONTAINER_NAME");

        _container = new BlobContainerClient(connectionString, containerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }


    public async Task<string> UploadFileAsync(string blobName, Stream stream, string contentType)
    {
        var blob = _container.GetBlobClient(blobName);
        await blob.UploadAsync(stream, overwrite: true);
        await blob.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });
        return blob.Uri.ToString(); // Return public or internal blob URI
    }

    public async Task<Stream?> DownloadFileAsync(string blobName)
    {
        var blob = _container.GetBlobClient(blobName);
        if (await blob.ExistsAsync())
        {
            var result = await blob.DownloadAsync();
            return result.Value.Content;
        }

        return null;
    }

    public async Task<bool> DeleteFileAsync(string blobName)
    {
        var blob = _container.GetBlobClient(blobName);
        return await blob.DeleteIfExistsAsync();
    }
}
