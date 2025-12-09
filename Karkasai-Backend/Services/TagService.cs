using HabitTribe.Auth.Model;
using HabitTribe.Entities;
using HabitTribe.Models;
using HabitTribe.Repositories;

namespace HabitTribe.Services;

public interface ITagService
{
    Task<TagDto> CreateTagAsync(CreateUpdateTagDto tagDto, CancellationToken token = default);
    Task<TagDto?> GetTagAsync(int tagId, CancellationToken token = default);
    Task<IEnumerable<TagDto>> GetAllTagsAsync(CancellationToken token = default);
    Task<TagDto?> UpdateTagAsync(int tagId, CreateUpdateTagDto tagDto, CancellationToken token = default);
    Task<bool> DeleteTagAsync(int tagId, CancellationToken token = default);
    Task<Tag?> GetTagEntityAsync(int tagId, CancellationToken token = default);
}

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<TagDto> CreateTagAsync(CreateUpdateTagDto tagDto, CancellationToken token)
    {
        var tag = new Tag
        {
            Name = tagDto.Name,
            Usable = true,
            Groups = new List<Group>() {}
        };

        await _tagRepository.AddAsync(tag, token);
        await _tagRepository.SaveChangesAsync(token);

        return MapToDto(tag);
    }

    public async Task<TagDto?> GetTagAsync(int tagId, CancellationToken token = default)
    {
        var tag = await _tagRepository.FindAsync(tagId, token);
        return tag == null ? null : MapToDto(tag);
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync(CancellationToken token = default)
    {
        var tags = await _tagRepository.GetAllAsync(token);
        return tags.Select(MapToDto);
    }

    public async Task<TagDto?> UpdateTagAsync(int tagId, CreateUpdateTagDto tagDto, CancellationToken token = default)
    {
        var tag = await _tagRepository.FindAsync(tagId, token);
        if (tag == null) return null;

        tag.Name = tagDto.Name;
        tag.Usable = tagDto.Usable;

        await _tagRepository.SaveChangesAsync(token);

        return MapToDto(tag);
    }

    public async Task<bool> DeleteTagAsync(int tagId, CancellationToken token = default)
    {
        var tag = await _tagRepository.FindAsync(tagId, token);
        if (tag == null) return false;

        await _tagRepository.RemoveAsync(tag, token);
        await _tagRepository.SaveChangesAsync(token);

        return true;
    }

    public async Task<Tag?> GetTagEntityAsync(int tagId, CancellationToken token = default)
    {
        return await _tagRepository.FindAsync(tagId, token);
    }

    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto(
            tag.Id,
            tag.Name,
            tag.Usable
        );
    }
}
