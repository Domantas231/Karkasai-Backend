using Microsoft.AspNetCore.SignalR;
using HabitTribe.Hubs;
using HabitTribe.Models;

namespace HabitTribe.Services;

public record PostNotificationDto(
    int PostId,
    int GroupId,
    string GroupTitle,
    string PostTitle,
    string AuthorName,
    DateTimeOffset CreatedAt
);

public interface INotificationService
{
    Task NotifyNewPostAsync(int groupId, string groupTitle, PostDto dto);
    Task NotifyPostDeletedAsync(int groupId, int postId);
    Task NotifyPostUpdatedAsync(int groupId, PostDto dto);
    Task NotifyNewCommentAsync(int groupId, int postId, CommentDto dto);
}

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewPostAsync(int groupId, string groupTitle, PostDto dto)
    {
        var notification = new PostNotificationDto(
            dto.Id,
            groupId,
            groupTitle,
            dto.Title,
            dto.User.UserName,
            dto.DateCreated
        );
        
        await _hubContext.Clients.Group($"group-{groupId}").SendAsync("NewPost", notification);
    }

    public async Task NotifyPostDeletedAsync(int groupId, int postId)
    {
        await _hubContext.Clients.Group($"group-{groupId}")
            .SendAsync("PostDeleted", new { GroupId = groupId, PostId = postId });
    }

    public async Task NotifyPostUpdatedAsync(int groupId, PostDto dto)
    {
        await _hubContext.Clients.Group($"group-{groupId}")
            .SendAsync("PostUpdated", new { GroupId = groupId, Post = dto });
    }

    public async Task NotifyNewCommentAsync(int groupId, int postId, CommentDto dto)
    {
        await _hubContext.Clients.Group($"group-{groupId}")
            .SendAsync("NewComment", new { GroupId = groupId, PostId = postId, Comment = dto });
    }
}