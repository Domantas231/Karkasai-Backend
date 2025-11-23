using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using HabitTribe.Auth;
using HabitTribe.Auth.Model;
using HabitTribe.Models;

namespace HabitTribe.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ISessionService _sessionService;

    public AuthController(
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        ISessionService sessionService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _sessionService = sessionService;
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user != null)
            return UnprocessableEntity("Username already taken");

        var newUser = new User
        {
            UserName = dto.UserName,
            Email = dto.Email
        };

        var createUserResult = await _userManager.CreateAsync(newUser, dto.Password);
        if (!createUserResult.Succeeded)
            return UnprocessableEntity(createUserResult.Errors);

        await _userManager.AddToRoleAsync(newUser, Roles.User);

        return Created();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user == null)
            return UnprocessableEntity("User does not exist");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            return UnprocessableEntity("Username or password is incorrect");

        var roles = await _userManager.GetRolesAsync(user);

        var sessionId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(3);
        var accessToken = _jwtTokenService.CreateAccessToken(user.UserName!, user.Id, roles);
        var refreshToken = _jwtTokenService.CreateRefreshToken(sessionId, user.Id, expiresAt);

        await _sessionService.CreateSessionAsync(sessionId, user.Id, refreshToken, expiresAt);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt
        };

        Response.Cookies.Append("RefreshToken", refreshToken, cookieOptions);

        return Ok(new SuccessfulLoginDto(accessToken));
    }

    [HttpPost("accessToken")]
    public async Task<IActionResult> RefreshAccessToken()
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            return UnprocessableEntity("Refresh token not found");

        if (!_jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
            return UnprocessableEntity("Invalid refresh token");

        var sessionId = claims?.FindFirstValue("SessionId");
        if (string.IsNullOrWhiteSpace(sessionId))
            return UnprocessableEntity("Invalid session");

        var sessionIdAsGuid = Guid.Parse(sessionId);
        if (!await _sessionService.IsSessionValidAsync(sessionIdAsGuid, refreshToken))
            return UnprocessableEntity("Session is invalid");

        var userId = claims?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null)
            return UnprocessableEntity("User not found");

        var roles = await _userManager.GetRolesAsync(user);

        var expiresAt = DateTime.UtcNow.AddDays(3);
        var accessToken = _jwtTokenService.CreateAccessToken(user.UserName!, user.Id, roles);
        var newRefreshToken = _jwtTokenService.CreateRefreshToken(sessionIdAsGuid, user.Id, expiresAt);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = expiresAt
        };

        Response.Cookies.Append("RefreshToken", newRefreshToken, cookieOptions);

        await _sessionService.ExtendSessionAsync(sessionIdAsGuid, newRefreshToken, expiresAt);

        return Ok(new SuccessfulLoginDto(accessToken));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            return UnprocessableEntity("Not logged in");

        if (!_jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
            return UnprocessableEntity("Invalid token");

        var sessionId = claims?.FindFirstValue("SessionId");
        if (string.IsNullOrWhiteSpace(sessionId))
            return UnprocessableEntity("Invalid session");

        await _sessionService.InvalidateSessionAsync(Guid.Parse(sessionId));
        Response.Cookies.Delete("RefreshToken");

        return Ok();
    }
}
