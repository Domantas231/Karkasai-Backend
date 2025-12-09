using FluentValidation;

namespace HabitTribe.Models;

public record GroupDto(
    int Id, 
    string Title, 
    string Description, 
    int CurrentMembers, 
    int MaxMembers, 
    DateTimeOffset DateCreated, 
    string ImageUrl,
    UserDto OwnerUser, 
    ICollection<UserDto> Members,
    ICollection<TagDto> Tags);

public record CreateGroupDto(string Title, string Description, int MaxMembers, ICollection<int> TagIds);

public record UpdateGroupDto(string Title, string Description, int MaxMembers, ICollection<int> TagIds);

public class CreateGroupDtoValidator : AbstractValidator<CreateGroupDto>
{
    public CreateGroupDtoValidator()
    {
        RuleFor(x => x.Title).NotNull().NotEmpty().Length(2, 100);
        RuleFor(x => x.Description).NotNull().NotEmpty().Length(3, 500);
        RuleFor(x => x.MaxMembers).NotNull().NotEmpty().InclusiveBetween(4, 7);
    }
}

public class UpdateGroupDtoValidator : AbstractValidator<UpdateGroupDto>
{
    public UpdateGroupDtoValidator()
    {
        RuleFor(x => x.Title).NotNull().NotEmpty().Length(2, 100);
        RuleFor(x => x.Description).NotNull().NotEmpty().Length(3, 500);
        RuleFor(x => x.MaxMembers).NotNull().NotEmpty().InclusiveBetween(4, 7);
    }
}
