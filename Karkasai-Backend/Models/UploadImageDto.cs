using FluentValidation;

namespace HabitTribe.Models;

public record UploadImageDto(IFormFile? Image);

public class UploadImageDtoValidator : AbstractValidator<UploadImageDto>
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly string[] _allowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadImageDtoValidator()
    {
        RuleFor(x => x.Image)
            .NotNull().WithMessage("Image is required.")
            .Must(file => file != null && file.Length > 0).WithMessage("Image file cannot be empty.")
            .Must(file => file != null && file.Length <= MaxFileSizeBytes)
            .WithMessage($"Image size must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB.")
            .Must(BeAValidContentType).WithMessage("Invalid file type. Allowed: JPG, PNG, WebP.")
            .Must(HaveValidExtension).WithMessage("Invalid file extension. Allowed: .jpg, .jpeg, .png, .webp");
    }

    private bool BeAValidContentType(IFormFile? file)
    {
        return file != null && _allowedContentTypes.Contains(file.ContentType.ToLower());
    }

    private bool HaveValidExtension(IFormFile? file)
    {
        if (file == null) return false;
        var extension = Path.GetExtension(file.FileName).ToLower();
        return _allowedExtensions.Contains(extension);
    }
}