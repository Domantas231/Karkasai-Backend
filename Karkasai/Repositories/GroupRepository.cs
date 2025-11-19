using Karkasai.Data;
using Karkasai.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karkasai.Repositories;

public class GroupRepository : IRepository<Group>
{
    private readonly ApplicationDbContext _context;
    
    public GroupRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Group entity, CancellationToken token = default)
    {
        await _context
            .Set<Group>()
            .AddAsync(entity, token);
    }

    public async Task<Group?> FindAsync(int id, CancellationToken token = default)
    {
        return await _context
            .Set<Group>()
            .FindAsync(
                keyValues: [id], 
                cancellationToken: token);
    }

    public async Task<IEnumerable<Group>> GetAllAsync(CancellationToken token = default)
    {
        return await _context
            .Set<Group>()
            .ToListAsync(token);
    }

    public Task RemoveAsync(Group entity, CancellationToken token = default)
    {
        _context
            .Set<Group>()
            .Remove(entity);
        
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        await _context.SaveChangesAsync(token);
    }
}