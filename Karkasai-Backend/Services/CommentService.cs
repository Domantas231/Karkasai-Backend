using HabitTribe.Auth.Model;
using HabitTribe.Entities;
using HabitTribe.Models;
using HabitTribe.Repositories;

namespace HabitTribe.Services;

public interface ICommentService
{
    Task<CommentDto> CreateCommentAsync(int groupId, int postId, CreateCommentDto dto, User user, CancellationToken token = default);
    Task<CommentDto?> GetCommentAsync(int groupId, int postId, int commentId, CancellationToken token = default);
    Task<IEnumerable<CommentDto>> GetAllCommentsAsync(int groupId, int postId, CancellationToken token = default);
    Task<CommentDto?> UpdateCommentAsync(int groupId, int postId, int commentId, UpdateCommentDto dto, CancellationToken token = default);
    Task<bool> DeleteCommentAsync(int groupId, int postId, int commentId, CancellationToken token = default);
    Task<Comment?> GetCommentEntityAsync(int groupId, int postId, int commentId, CancellationToken token = default);
}

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;

    public CommentService(ICommentRepository commentRepository, IPostRepository postRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
    }

    public async Task<CommentDto> CreateCommentAsync(int groupId, int postId, CreateCommentDto dto, User user, CancellationToken token = default)
    {
        var post = await _postRepository.FindWithUserAsync(groupId, postId, token);
        if (post == null)
            throw new InvalidOperationException("Post not found");

        var comment = new Comment
        {
            Content = dto.Content,
            DateCreated = DateTimeOffset.UtcNow,
            PostId = postId,
            Post = post,
            UserId = user.Id,
            User = user
        };

        await _commentRepository.AddAsync(comment, token);
        await _commentRepository.SaveChangesAsync(token);

        return MapToDto(comment);
    }

    public async Task<CommentDto?> GetCommentAsync(int groupId, int postId, int commentId, CancellationToken token = default)
    {
        var comment = await _commentRepository.FindWithUserAsync(groupId, postId, commentId, token);
        return comment == null ? null : MapToDto(comment);
    }

    public async Task<IEnumerable<CommentDto>> GetAllCommentsAsync(int groupId, int postId, CancellationToken token = default)
    {
        var comments = await _commentRepository.GetAllByPostIdAsync(groupId, postId, token);
        return comments.Select(MapToDto);
    }

    public async Task<CommentDto?> UpdateCommentAsync(int groupId, int postId, int commentId, UpdateCommentDto dto, CancellationToken token = default)
    {
        var comment = await _commentRepository.FindWithUserAsync(groupId, postId, commentId, token);
        if (comment == null) return null;

        comment.Content = dto.Content;

        await _commentRepository.SaveChangesAsync(token);

        return MapToDto(comment);
    }

    public async Task<bool> DeleteCommentAsync(int groupId, int postId, int commentId, CancellationToken token = default)
    {
        var comment = await _commentRepository.FindWithUserAsync(groupId, postId, commentId, token);
        if (comment == null) return false;

        await _commentRepository.RemoveAsync(comment, token);
        await _commentRepository.SaveChangesAsync(token);

        return true;
    }

    public async Task<Comment?> GetCommentEntityAsync(int groupId, int postId, int commentId, CancellationToken token = default)
    {
        return await _commentRepository.FindWithUserAsync(groupId, postId, commentId, token);
    }

    private static CommentDto MapToDto(Comment comment)
    {
        return new CommentDto(
            comment.Id,
            comment.Content,
            comment.DateCreated,
            new UserDto(comment.User.UserName!)
        );
    }
}
