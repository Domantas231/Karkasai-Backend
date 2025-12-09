using FluentValidation;

namespace HabitTribe.Models;

public record TagDto(int Id, string Name, bool Usable);
public record CreateUpdateTagDto(int Id, string Name, bool Usable);

public class TagDtoValidator : AbstractValidator<CreateUpdateTagDto>
{
    public TagDtoValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty().Length(5, 100);
    }
}