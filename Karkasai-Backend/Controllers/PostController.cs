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
[Route("api/groups/{groupId}/posts")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IGroupService _groupService;
    private readonly UserManager<User> _userManager;

    public PostController(
        IPostService postService,
        IGroupService groupService,
        UserManager<User> userManager)
    {
        _postService = postService;
        _groupService = groupService;
        _userManager = userManager;
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
        if (!posts.Any()) return NotFound();

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
            return Created($"api/groups/{groupId}/posts/{post.Id}", post);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
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

        var deleted = await _postService.DeletePostAsync(groupId, postId, token);
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
