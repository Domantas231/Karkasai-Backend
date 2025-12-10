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

public record GroupNotificationDto(
    int GroupId,
    string Title,
    string Description,
    string OwnerName,
    DateTimeOffset CreatedAt
);

public interface INotificationService
{
    Task NotifyNewPostAsync(int groupId, string groupTitle, PostDto dto);
    Task NotifyPostDeletedAsync(int groupId, int postId, string groupTitle);
    Task NotifyPostUpdatedAsync(int groupId, PostDto dto, string groupTitle);
    Task NotifyNewCommentAsync(int groupId, int postId, string postAuthorName, CommentDto dto);
    Task NotifyNewGroupAsync(GroupDto dto);
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

    public async Task NotifyPostDeletedAsync(int groupId, int postId, string groupTitle)
    {
        await _hubContext.Clients.Group($"group-{groupId}")
            .SendAsync("PostDeleted", new { GroupId = groupId, PostId = postId, GroupTitle = groupTitle });
    }

    public async Task NotifyPostUpdatedAsync(int groupId, PostDto dto, string groupTitle)
    {
        await _hubContext.Clients.Group($"group-{groupId}")
            .SendAsync("PostUpdated", new { GroupId = groupId, GroupTitle = groupTitle, Post = dto });
    }

    public async Task NotifyNewCommentAsync(int groupId, int postId, string postAuthorName, CommentDto dto)
    {
        await _hubContext.Clients.Group($"group-{groupId}")
            .SendAsync("NewComment", new { 
                GroupId = groupId, 
                PostId = postId, 
                PostAuthorName = postAuthorName,
                Comment = dto 
            });
    }

    public async Task NotifyNewGroupAsync(GroupDto dto)
    {
        var notification = new GroupNotificationDto(
            dto.Id,
            dto.Title,
            dto.Description,
            dto.OwnerUser.UserName,
            dto.DateCreated
        );
        
        // Broadcast to all connected users
        await _hubContext.Clients.All.SendAsync("NewGroup", notification);
    }
}