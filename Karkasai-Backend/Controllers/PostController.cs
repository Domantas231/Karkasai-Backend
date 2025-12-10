using System.Security.Claims;
using FluentValidation;
using HabitTribe.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using HabitTribe.Auth.Model;
using HabitTribe.Models;
using HabitTribe.Services;

namespace HabitTribe.Controllers;

[ApiController]
[Route("api/groups/{groupId}/posts")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IGroupService _groupService;
    private readonly INotificationService _notificationService;
    private readonly UserManager<User> _userManager;
    private readonly IValidator<UploadImageDto> _uploadImageValidator;
    private readonly ISessionService _sessionService;
    private readonly IJwtTokenService _jwtTokenService;

    public PostController(
        IPostService postService,
        IGroupService groupService,
        INotificationService notificationService,
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        ISessionService sessionService,
        IValidator<UploadImageDto> uploadImageValidator)
    {
        _postService = postService;
        _groupService = groupService;
        _notificationService = notificationService;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _sessionService = sessionService;
        _uploadImageValidator = uploadImageValidator;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(int groupId, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!await IsUserGroupMemberOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();

        var posts = await _postService.GetAllPostsAsync(groupId, token);
        // if (!posts.Any()) return NotFound();

        return Ok(posts);
    }

    [HttpGet("{postId}")]
    [Authorize]
    public async Task<IActionResult> Get(int groupId, int postId, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!await IsUserGroupMemberOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();

        var post = await _postService.GetPostAsync(groupId, postId, token);
        if (post == null) return NotFound();

        return Ok(post);
    }

    [HttpPost]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Create(int groupId, [FromBody] CreatePostDto dto, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!await IsUserGroupMemberOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();

        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null) return Unauthorized();

        try
        {
            var post = await _postService.CreatePostAsync(groupId, dto, user, token);
            
            // Notification
            var group = await _groupService.GetGroupAsync(groupId);
            await _notificationService.NotifyNewPostAsync(groupId, group.Title, post);
            
            return Created($"api/groups/{groupId}/posts/{post.Id}", post);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
    
    [HttpPut("{postId}/image")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Image(int groupId, int postId, [FromForm] UploadImageDto dto, 
        [FromServices] IValidator<UploadImageDto> validator, CancellationToken token)
    {
        var validationResult = await validator.ValidateAsync(dto, token);
        if (!validationResult.IsValid)
        {
            return UnprocessableEntity(validationResult.Errors);
        }
        
        if (!await IsSessionValid())
            return Unauthorized();

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        //if (!await _postService.IsUserOwnerOrAdmin(groupId, userId!, isAdmin, token))
        //    return Forbid();
        
        var group = await _postService.AddPostImage(groupId, postId, dto.Image, token);
        if(group == null) return NotFound();
        
        return Ok();
    }
    
    [HttpDelete("{postId}/image")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Image(int groupId, int postId, CancellationToken token)
    {
        if (!await IsSessionValid())
            return Unauthorized();

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);
        
        if (!await _groupService.IsUserOwnerOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();
        
        var result = await _postService.DeletePostImage(groupId, postId, token);
        if (!result) return NotFound();
        
        return Ok();
    }

    [HttpPut("{postId}")]
    [Authorize]
    public async Task<IActionResult> Update(int groupId, int postId, [FromBody] UpdatePostDto dto, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!await IsUserGroupMemberOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();

        var existingPost = await _postService.GetPostEntityAsync(groupId, postId, token);
        if (existingPost == null) return NotFound();

        if (existingPost.UserId != userId && !isAdmin)
            return Forbid();

        var post = await _postService.UpdatePostAsync(groupId, postId, dto, token);
        if (post == null) return NotFound();
        
        var group = await _groupService.GetGroupAsync(groupId);
        await _notificationService.NotifyPostUpdatedAsync(groupId, post, group.Title);

        return Ok(post);
    }

    [HttpDelete("{postId}")]
    [Authorize]
    public async Task<IActionResult> Delete(int groupId, int postId, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!await IsUserGroupMemberOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();

        var existingPost = await _postService.GetPostEntityAsync(groupId, postId, token);
        if (existingPost == null) return NotFound();

        if (existingPost.UserId != userId && !isAdmin)
            return Forbid();

        var group = await _groupService.GetGroupAsync(groupId);
        
        var deleted = await _postService.DeletePostAsync(groupId, postId, token);
        if (!deleted) return NotFound();
        
        await _notificationService.NotifyPostDeletedAsync(groupId, postId, group.Title);

        return NoContent();
    }

    private async Task<bool> IsUserGroupMemberOrAdmin(int groupId, string userId, bool isAdmin, CancellationToken token)
    {
        if (isAdmin) return true;

        var group = await _groupService.GetGroupEntityAsync(groupId, token);
        if (group == null) return false;

        return group.Members.Any(m => m.Id == userId);
    }
    
    private async Task<bool> IsSessionValid()
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            return false;

        if (!_jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
            return false;

        var sessionId = claims?.FindFirstValue("SessionId");
        if (string.IsNullOrWhiteSpace(sessionId)) return false;

        return await _sessionService.IsSessionValidAsync(Guid.Parse(sessionId), refreshToken);
    }
}