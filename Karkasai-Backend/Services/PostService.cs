using HabitTribe.Auth.Model;
using HabitTribe.Entities;
using HabitTribe.Models;
using HabitTribe.Repositories;

namespace HabitTribe.Services;

public interface IPostService
{
    Task<PostDto> CreatePostAsync(int groupId, CreatePostDto dto, User user, CancellationToken token = default);
    Task<PostDto?> GetPostAsync(int groupId, int postId, CancellationToken token = default);
    Task<IEnumerable<PostDto>> GetAllPostsAsync(int groupId, CancellationToken token = default);
    Task<PostDto?> UpdatePostAsync(int groupId, int postId, UpdatePostDto dto, CancellationToken token = default);
    Task<bool> DeletePostAsync(int groupId, int postId, CancellationToken token = default);
    Task<Post?> GetPostEntityAsync(int groupId, int postId, CancellationToken token = default);
}

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IGroupRepository _groupRepository;

    public PostService(IPostRepository postRepository, IGroupRepository groupRepository)
    {
        _postRepository = postRepository;
        _groupRepository = groupRepository;
    }

    public async Task<PostDto> CreatePostAsync(int groupId, CreatePostDto dto, User user, CancellationToken token = default)
    {
        var group = await _groupRepository.FindAsync(groupId, token);
        if (group == null)
            throw new InvalidOperationException("Group not found");

        var post = new Post
        {
            Title = dto.Title,
            DateCreated = DateTimeOffset.UtcNow,
            GroupId = groupId,
            Group = group,
            UserId = user.Id,
            User = user
        };

        await _postRepository.AddAsync(post, token);
        await _postRepository.SaveChangesAsync(token);

        return MapToDto(post);
    }

    public async Task<PostDto?> GetPostAsync(int groupId, int postId, CancellationToken token = default)
    {
        var post = await _postRepository.FindWithUserAsync(groupId, postId, token);
        return post == null ? null : MapToDto(post);
    }

    public async Task<IEnumerable<PostDto>> GetAllPostsAsync(int groupId, CancellationToken token = default)
    {
        var posts = await _postRepository.GetAllByGroupIdAsync(groupId, token);
        return posts.Select(MapToDto);
    }

    public async Task<PostDto?> UpdatePostAsync(int groupId, int postId, UpdatePostDto dto, CancellationToken token = default)
    {
        var post = await _postRepository.FindWithUserAsync(groupId, postId, token);
        if (post == null) return null;

        post.Title = dto.Title;

        await _postRepository.SaveChangesAsync(token);

        return MapToDto(post);
    }

    public async Task<bool> DeletePostAsync(int groupId, int postId, CancellationToken token = default)
    {
        var post = await _postRepository.FindWithUserAsync(groupId, postId, token);
        if (post == null) return false;

        await _postRepository.RemoveAsync(post, token);
        await _postRepository.SaveChangesAsync(token);

        return true;
    }

    public async Task<Post?> GetPostEntityAsync(int groupId, int postId, CancellationToken token = default)
    {
        return await _postRepository.FindWithUserAsync(groupId, postId, token);
    }

    private static PostDto MapToDto(Post post)
    {
        return new PostDto(
            post.Id,
            post.Title,
            post.DateCreated,
            new UserDto(post.User.UserName!)
        );
    }
}
