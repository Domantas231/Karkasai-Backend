using HabitTribe.Auth.Model;
using HabitTribe.Entities;
using HabitTribe.Models;
using HabitTribe.Repositories;

namespace HabitTribe.Services;

public interface IGroupService
{
    Task<GroupDto> CreateGroupAsync(CreateGroupDto dto, User ownerUser, CancellationToken token = default);
    Task<GroupDto?> GetGroupAsync(int id, CancellationToken token = default);
    Task<IEnumerable<GroupDto>> GetAllGroupsAsync(CancellationToken token = default);
    Task<GroupDto?> UpdateGroupAsync(int id, UpdateGroupDto dto, CancellationToken token = default);
    Task<bool> DeleteGroupAsync(int id, CancellationToken token = default);
    Task<GroupDto?> JoinGroupAsync(int groupId, User newMember, CancellationToken token = default);
    Task<Group?> GetGroupEntityAsync(int id, CancellationToken token = default);
    Task<bool> IsUserOwnerOrAdmin(int groupId, string userId, bool isAdmin, CancellationToken token = default);
}

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly ITagService _tagService;

    public GroupService(IGroupRepository groupRepository, ITagService tagService)
    {
        _groupRepository = groupRepository;
        _tagService = tagService;
    }

    public async Task<GroupDto> CreateGroupAsync(CreateGroupDto dto, User ownerUser, CancellationToken token = default)
    {
        var tags = new List<Tag>();
        foreach (var id in dto.TagIds) tags.Add(await _tagService.GetTagEntityAsync(id, token));
        
        var group = new Group
        {
            Title = dto.Title,
            Description = dto.Description,
            MaxMembers = dto.MaxMembers,
            CurrentMembers = 1,
            DateCreated = DateTimeOffset.UtcNow,
            OwnerUserId = ownerUser.Id,
            OwnerUser = ownerUser,
            Members = new List<User> { ownerUser },
            Tags = tags
        };
        
        await _groupRepository.AddAsync(group, token);
        await _groupRepository.SaveChangesAsync(token);
        
        return MapToDto(group);
    }

    public async Task<GroupDto?> GetGroupAsync(int id, CancellationToken token = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(id, token);
        return group == null ? null : MapToDto(group);
    }

    public async Task<IEnumerable<GroupDto>> GetAllGroupsAsync(CancellationToken token = default)
    {
        var groups = await _groupRepository.GetAllWithDetailsAsync(token);
        return groups.Select(MapToDto);
    }

    public async Task<GroupDto?> UpdateGroupAsync(int id, UpdateGroupDto dto, CancellationToken token = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(id, token);
        if (group == null) return null;

        var tags = await Task.WhenAll(
            dto.TagIds.Select(i => _tagService.GetTagEntityAsync(i, token)));
        
        group.Title = dto.Title;
        group.Description = dto.Description;
        group.MaxMembers = dto.MaxMembers;
        group.Tags = tags;

        await _groupRepository.SaveChangesAsync(token);

        return MapToDto(group);
    }

    public async Task<bool> DeleteGroupAsync(int id, CancellationToken token = default)
    {
        var group = await _groupRepository.FindAsync(id, token);
        if (group == null) return false;

        await _groupRepository.RemoveAsync(group, token);
        await _groupRepository.SaveChangesAsync(token);

        return true;
    }

    public async Task<GroupDto?> JoinGroupAsync(int groupId, User newMember, CancellationToken token = default)
    {
        var group = await _groupRepository.FindWithDetailsAsync(groupId, token);
        if (group == null) return null;

        if (group.CurrentMembers >= group.MaxMembers)
            return null;

        group.Members.Add(newMember);
        group.CurrentMembers = group.Members.Count;

        await _groupRepository.SaveChangesAsync(token);

        return MapToDto(group);
    }

    public async Task<Group?> GetGroupEntityAsync(int id, CancellationToken token = default)
    {
        return await _groupRepository.FindWithDetailsAsync(id, token);
    }

    public async Task<bool> IsUserOwnerOrAdmin(int groupId, string userId, bool isAdmin, CancellationToken token = default)
    {
        if (isAdmin) return true;
        
        var group = await _groupRepository.FindAsync(groupId, token);
        return group?.OwnerUserId == userId;
    }

    private static GroupDto MapToDto(Group group)
    {
        return new GroupDto(
            group.Id,
            group.Title,
            group.Description,
            group.CurrentMembers,
            group.MaxMembers,
            group.DateCreated,
            new UserDto(group.OwnerUser.UserName!),
            group.Members.Select(m => new UserDto(m.UserName!)).ToList(),
            group.Tags.Select(t => new TagDto(t.Id, t.Name!)).ToList()
        );
    }
}
