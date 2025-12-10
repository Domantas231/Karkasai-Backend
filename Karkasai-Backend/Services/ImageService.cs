namespace HabitTribe.Services;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public interface IImageService
{
    Task<string?> UploadImageAsync(IFormFile file, string folder);
    Task<bool> DeleteImageAsync(string publicId);
    Task<bool> DeleteImageByUrlAsync(string imageUrl);
}

public class ImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public ImageService(IConfiguration config)
    {
        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]);
        
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string?> UploadImageAsync(IFormFile file, string folder)
    {
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream)
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl?.ToString();
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        return result.Result == "ok";
    }
    
    public async Task<bool> DeleteImageByUrlAsync(string imageUrl)
    {
        var publicId = ExtractPublicIdFromUrl(imageUrl);
        if (string.IsNullOrEmpty(publicId))
            return false;
            
        return await DeleteImageAsync(publicId);
    }

    private string? ExtractPublicIdFromUrl(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return null;

        try
        {
            // Cloudinary URL format: https://res.cloudinary.com/{cloud}/image/upload/v{version}/{public_id}.{ext}
            var uri = new Uri(imageUrl);
            var path = uri.AbsolutePath; // /dg17lpxbx/image/upload/v1234567890/folder/image.jpg
            
            var uploadIndex = path.IndexOf("/upload/");
            if (uploadIndex == -1)
                return null;

            var afterUpload = path.Substring(uploadIndex + 8); // v1234567890/folder/image.jpg
            
            // Remove version if present (starts with 'v' followed by numbers)
            if (afterUpload.StartsWith("v") && afterUpload.Contains("/"))
            {
                var versionEnd = afterUpload.IndexOf('/');
                afterUpload = afterUpload.Substring(versionEnd + 1); // folder/image.jpg
            }

            // Remove file extension
            var lastDot = afterUpload.LastIndexOf('.');
            if (lastDot > 0)
                afterUpload = afterUpload.Substring(0, lastDot); // folder/image

            return afterUpload;
        }
        catch
        {
            return null;
        }
    }
}