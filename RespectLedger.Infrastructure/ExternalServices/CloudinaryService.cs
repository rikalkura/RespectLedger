using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using RespectLedger.Application.Common.Interfaces;

namespace RespectLedger.Infrastructure.ExternalServices;

public class CloudinaryService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        string cloudName = configuration["Cloudinary:CloudName"] ?? throw new InvalidOperationException("Cloudinary CloudName not configured");
        string apiKey = configuration["Cloudinary:ApiKey"] ?? throw new InvalidOperationException("Cloudinary ApiKey not configured");
        string apiSecret = configuration["Cloudinary:ApiSecret"] ?? throw new InvalidOperationException("Cloudinary ApiSecret not configured");

        Account account = new(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
    {
        ImageUploadParams uploadParams = new()
        {
            File = new FileDescription(fileName, imageStream),
            Folder = "respectledger",
            Overwrite = true,
            ResourceType = ResourceType.Image
        };

        ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Failed to upload image: {uploadResult.Error?.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<bool> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract public ID from URL
            Uri uri = new(imageUrl);
            string publicId = uri.Segments.Last().Split('.')[0]; // Get filename without extension
            publicId = $"respectledger/{publicId}";

            DeletionParams deletionParams = new(publicId)
            {
                ResourceType = ResourceType.Image
            };

            DeletionResult deletionResult = await _cloudinary.DestroyAsync(deletionParams);
            return deletionResult.Result == "ok";
        }
        catch
        {
            return false;
        }
    }
}
