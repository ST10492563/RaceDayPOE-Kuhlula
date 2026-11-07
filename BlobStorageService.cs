
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDayMvc.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureStorage:ConnectionString"];

            var containerName =
                configuration["AzureStorage:ContainerName"]
                ?? "event-banners";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            var blobServiceClient =
                new BlobServiceClient(connectionString);

            _container =
                blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("Please select an image.");

            var extension =
                Path.GetExtension(image.FileName).ToLowerInvariant();

            var allowedExtensions =
                new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException(
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");

            if (image.Length > 5 * 1024 * 1024)
                throw new ArgumentException(
                    "The image must be smaller than 5 MB.");

            await _container.CreateIfNotExistsAsync();

            var fileName = $"{Guid.NewGuid()}{extension}";
            var blob = _container.GetBlobClient(fileName);

            using var stream = image.OpenReadStream();

            await blob.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = image.ContentType
                });

            return fileName;
        }

        public async Task<(Stream Content, string ContentType)> GetImageAsync(
            string fileName)
        {
            var blob = _container.GetBlobClient(fileName);

            var download = await blob.DownloadStreamingAsync();

            return (
                download.Value.Content,
                download.Value.Details.ContentType ?? "application/octet-stream"
            );
        }
    }
}
