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
[Route("api/groups/{groupId}/posts/{postId}/comments")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IGroupService _groupService;
    private readonly IPostService _postService;
    private readonly INotificationService _notificationService;
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ISessionService _sessionService;
    private readonly IValidator<UploadImageDto> _uploadImageValidator;

    public CommentController(
        ICommentService commentService,
        IGroupService groupService,
        IPostService postService,
        INotificationService notificationService,
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        ISessionService sessionService,
        IValidator<UploadImageDto> uploadImageValidator)
    {
        _commentService = commentService;
        _groupService = groupService;
        _postService = postService;
        _notificationService = notificationService;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _sessionService = sessionService;
        _uploadImageValidator = uploadImageValidator;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(int groupId, int postId, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!await IsUserGroupMemberOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();

        var comments = await _commentService.GetAllCommentsAsync(groupId, postId, token);
        if (!comments.Any()) return NotFound();

        return Ok(comments);
    }

    [HttpGet("{commentId}")]
    [Authorize]
    public async Task<IActionResult> Get(int groupId, int postId, int commentId, CancellationToken token)
    {
        var comment = await _commentService.GetCommentAsync(groupId, postId, commentId, token);
        if (comment == null) return NotFound();

        return Ok(comment);
    }

    [HttpPost]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Create(int groupId, int postId, [FromBody] CreateCommentDto dto, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null) return Unauthorized();

        try
        {
            var comment = await _commentService.CreateCommentAsync(groupId, postId, dto, user, token);
            
            // Get the post to find the author's name
            var post = await _postService.GetPostAsync(groupId, postId, token);
            var postAuthorName = post?.User?.UserName ?? "";
            
            await _notificationService.NotifyNewCommentAsync(groupId, postId, postAuthorName, comment);
            
            return Created($"api/groups/{groupId}/posts/{postId}/comments/{comment.Id}", comment);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
    
    [HttpPut("{commentId}/image")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Image(int groupId, int postId, int commentId, [FromForm] UploadImageDto dto, 
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
        
        var group = await _commentService.AddCommentImage(groupId, postId, commentId, dto.Image, token);
        if(group == null) return NotFound();
        
        return Ok();
    }
    
    [HttpDelete("{commentId}/image")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Image(int groupId, int postId, int commentId, CancellationToken token)
    {
        if (!await IsSessionValid())
            return Unauthorized();

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);
        
        // if (!await _groupService.IsUserOwnerOrAdmin(groupId, userId!, isAdmin, token))
        //     return Forbid();
        
        var result = await _commentService.DeleteCommentImage(groupId, postId, commentId, token);
        if (!result) return NotFound();
        
        return Ok();
    }

    [HttpPut("{commentId}")]
    [Authorize]
    public async Task<IActionResult> Update(int groupId, int postId, int commentId, [FromBody] UpdateCommentDto dto, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        var existingComment = await _commentService.GetCommentEntityAsync(groupId, postId, commentId, token);
        if (existingComment == null) return NotFound();

        if (existingComment.UserId != userId && !isAdmin)
            return Forbid();

        var comment = await _commentService.UpdateCommentAsync(groupId, postId, commentId, dto, token);
        if (comment == null) return NotFound();

        return Ok(comment);
    }

    [HttpDelete("{commentId}")]
    [Authorize]
    public async Task<IActionResult> Delete(int groupId, int postId, int commentId, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        var existingComment = await _commentService.GetCommentEntityAsync(groupId, postId, commentId, token);
        if (existingComment == null) return NotFound();

        if (existingComment.UserId != userId && !isAdmin)
            return Forbid();

        var deleted = await _commentService.DeleteCommentAsync(groupId, postId, commentId, token);
        if (!deleted) return NotFound();

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