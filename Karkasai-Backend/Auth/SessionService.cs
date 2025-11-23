using HabitTribe.Entities;
using HabitTribe.Helpers;
using HabitTribe.Repositories;

namespace HabitTribe.Auth;

public interface ISessionService
{
    Task CreateSessionAsync(Guid sessionId, string userId, string refreshToken, DateTime expiresAt);
    Task ExtendSessionAsync(Guid sessionId, string refreshToken, DateTime expiresAt);
    Task InvalidateSessionAsync(Guid sessionId);
    Task<bool> IsSessionValidAsync(Guid sessionId, string refreshToken);
}

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;

    public SessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task CreateSessionAsync(Guid sessionId, string userId, string refreshToken, DateTime expiresAt)
    {
        var session = new Session
        {
            Id = sessionId,
            UserId = userId,
            InitiatedAt = DateTimeOffset.Now,
            ExpiresAt = expiresAt,
            LastRefreshToken = refreshToken.ToSHA256()
        };
        
        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveChangesAsync();
    }

    public async Task ExtendSessionAsync(Guid sessionId, string refreshToken, DateTime expiresAt)
    {
        var session = await _sessionRepository.FindAsync(sessionId);
        if (session == null) return;
        
        session.ExpiresAt = expiresAt;
        session.LastRefreshToken = refreshToken.ToSHA256();
        
        await _sessionRepository.SaveChangesAsync();
    }
    
    public async Task InvalidateSessionAsync(Guid sessionId)
    {
        var session = await _sessionRepository.FindAsync(sessionId);
        if (session is null)
            return;
        
        session.IsRevoked = true;
        
        await _sessionRepository.SaveChangesAsync();
    }

    public async Task<bool> IsSessionValidAsync(Guid sessionId, string refreshToken)
    {
        var session = await _sessionRepository.FindAsync(sessionId);
        return session is not null 
               && session.ExpiresAt > DateTimeOffset.UtcNow 
               && !session.IsRevoked
               && session.LastRefreshToken == refreshToken.ToSHA256();
    }
}
