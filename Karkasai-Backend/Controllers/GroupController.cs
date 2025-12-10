using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using HabitTribe.Auth;
using HabitTribe.Auth.Model;
using HabitTribe.Models;
using HabitTribe.Services;

namespace HabitTribe.Controllers;

[ApiController]
[Route("api/groups")]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly INotificationService _notificationService;
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ISessionService _sessionService;
    private readonly IValidator<UploadImageDto> _uploadImageValidator;

    public GroupController(
        IGroupService groupService,
        INotificationService notificationService,
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        ISessionService sessionService,
        IValidator<UploadImageDto> uploadImageValidator)
    {
        _groupService = groupService;
        _notificationService = notificationService;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _sessionService = sessionService;
        _uploadImageValidator = uploadImageValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        var groups = await _groupService.GetAllGroupsAsync(token);
        return Ok(groups);
    }

    [HttpGet("{groupId}")]
    public async Task<IActionResult> Get(int groupId, CancellationToken token)
    {
        var group = await _groupService.GetGroupAsync(groupId, token);
        if (group == null) return NotFound();

        return Ok(group);
    }
    
    [HttpPost]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Create([FromBody] CreateGroupDto dto, CancellationToken token)
    {
        if (!await IsSessionValid())
            return Unauthorized();

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var ownerUser = await _userManager.FindByIdAsync(userId!);
        if (ownerUser == null) return Unauthorized();

        var group = await _groupService.CreateGroupAsync(dto, ownerUser, token);

        // Notify all users about the new group
        await _notificationService.NotifyNewGroupAsync(group);

        return Created($"api/groups/{group.Id}", group);
    }

    [HttpPut("{groupId}")]
    [Authorize]
    public async Task<IActionResult> Update(int groupId, [FromBody] UpdateGroupDto dto, CancellationToken token)
    {
        if (!await IsSessionValid())
            return Unauthorized();

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!await _groupService.IsUserOwnerOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();

        var group = await _groupService.UpdateGroupAsync(groupId, dto, token);
        if (group == null) return NotFound();

        return Ok(group);
    }
    
    [HttpPut("{groupId}/image")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Image(int groupId, [FromForm] UploadImageDto dto, 
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

        if (!await _groupService.IsUserOwnerOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();
        
        var group = await _groupService.AddGroupImage(groupId, dto.Image, token);
        if(group == null) return NotFound();
        
        return Ok();
    }

    [HttpDelete("{groupId}/image")]
    [Authorize(Roles = Roles.User)]
    public async Task<IActionResult> Image(int groupId, CancellationToken token)
    {
        if (!await IsSessionValid())
            return Unauthorized();

        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);
        
        if (!await _groupService.IsUserOwnerOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();
        
        var result = await _groupService.DeleteGroupImage(groupId, token);
        if (!result) return NotFound();
        
        return Ok();
    }
    
    [HttpPut("{groupId}/join")]
    [Authorize]
    public async Task<IActionResult> Join(int groupId, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var newMember = await _userManager.FindByIdAsync(userId!);
        if (newMember == null) return Unauthorized();

        var group = await _groupService.JoinGroupAsync(groupId, newMember, token);
        if (group == null) return NotFound();

        return Ok(group);
    }

    [HttpDelete("{groupId}")]
    [Authorize]
    public async Task<IActionResult> Delete(int groupId, CancellationToken token)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var isAdmin = User.IsInRole(Roles.Admin);

        if (!await _groupService.IsUserOwnerOrAdmin(groupId, userId!, isAdmin, token))
            return Forbid();

        var deleted = await _groupService.DeleteGroupAsync(groupId, token);
        if (!deleted) return NotFound();

        return NoContent();
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