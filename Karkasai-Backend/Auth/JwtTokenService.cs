using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace HabitTribe.Auth;

public interface IJwtTokenService
{
    string CreateAccessToken(string username, string userId, IEnumerable<string> roles);
    string CreateRefreshToken(Guid sessionId, string userId, DateTime expires);
    bool TryParseRefreshToken(string refreshToken, out ClaimsPrincipal? claims);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly SymmetricSecurityKey _authSigningKey;
    private readonly string? _issuer;
    private readonly string? _audience;

    public JwtTokenService(IConfiguration configuration)
    {
        _authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
        _issuer = configuration["Jwt:ValidIssuer"];
        _audience = configuration["Jwt:ValidAudience"];
    }

    public string CreateAccessToken(string username, string userId, IEnumerable<string> roles)
    {
        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, userId)
        };
        
        authClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            expires: DateTime.Now.AddMinutes(10),
            claims: authClaims,
            signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken(Guid sessionId, string userId, DateTime expires)
    {
        var authClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, userId),
            new("SessionId", sessionId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            expires: expires,
            claims: authClaims,
            signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryParseRefreshToken(string refreshToken, out ClaimsPrincipal? claims)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = _authSigningKey,
                ValidateLifetime = true
            };
            
            claims = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
            return true;
        }
        catch
        {
            claims = null;
            return false;
        }
    }
}
