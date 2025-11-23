using System.Security.Claims;
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
    private readonly UserManager<User> _userManager;

    public CommentController(
        ICommentService commentService,
        IGroupService groupService,
        UserManager<User> userManager)
    {
        _commentService = commentService;
        _groupService = groupService;
        _userManager = userManager;
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
            return Created($"api/groups/{groupId}/posts/{postId}/comments/{comment.Id}", comment);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
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
}
