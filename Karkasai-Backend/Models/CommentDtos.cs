using FluentValidation;

namespace HabitTribe.Models;

public record CommentDto(int Id, string Content, DateTimeOffset DateCreated, UserDto User);

public record CreateCommentDto(string Content);

public record UpdateCommentDto(string Content);

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.Content).NotNull().NotEmpty().Length(1, 300);
    }
}

public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
{
    public UpdateCommentDtoValidator()
    {
        RuleFor(x => x.Content).NotNull().NotEmpty().Length(1, 300);
    }
}
