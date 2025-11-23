using Microsoft.EntityFrameworkCore;
using HabitTribe.Data;
using HabitTribe.Entities;

namespace HabitTribe.Repositories;

public interface ISessionRepository
{
    Task AddAsync(Session session, CancellationToken token = default);
    Task<Session?> FindAsync(Guid id, CancellationToken token = default);
    Task SaveChangesAsync(CancellationToken token = default);
}

public class SessionRepository : ISessionRepository
{
    private readonly ApplicationDbContext _context;
    
    public SessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Session session, CancellationToken token = default)
    {
        await _context.Sessions.AddAsync(session, token);
    }

    public async Task<Session?> FindAsync(Guid id, CancellationToken token = default)
    {
        return await _context.Sessions.FindAsync(keyValues: [id], cancellationToken: token);
    }

    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        await _context.SaveChangesAsync(token);
    }
}
