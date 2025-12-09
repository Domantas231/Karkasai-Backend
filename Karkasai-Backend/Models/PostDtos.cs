using FluentValidation;

namespace HabitTribe.Models;

public record PostDto(int Id, string Title, DateTimeOffset DateCreated, string ImageUrl, UserDto User);

public record CreatePostDto(string Title);

public record UpdatePostDto(string Title);

public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostDtoValidator()
    {
        RuleFor(x => x.Title).NotNull().NotEmpty().Length(2, 100);
    }
}

public class UpdatePostDtoValidator : AbstractValidator<UpdatePostDto>
{
    public UpdatePostDtoValidator()
    {
        RuleFor(x => x.Title).NotNull().NotEmpty().Length(2, 100);
    }
}
