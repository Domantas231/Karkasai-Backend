using Microsoft.EntityFrameworkCore;
using HabitTribe.Data;
using HabitTribe.Entities;

namespace HabitTribe.Repositories;

public interface IGroupRepository : IRepository<Group>
{
    Task<Group?> FindWithDetailsAsync(int id, CancellationToken token = default);
    Task<IEnumerable<Group>> GetAllWithDetailsAsync(CancellationToken token = default);
}

public class GroupRepository : Repository<Group>, IGroupRepository
{
    public GroupRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Group?> FindWithDetailsAsync(int id, CancellationToken token = default)
    {
        return await _context.Groups
            .Include(g => g.OwnerUser)
            .Include(g => g.Tags)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, token);
    }

    public async Task<IEnumerable<Group>> GetAllWithDetailsAsync(CancellationToken token = default)
    {
        return await _context.Groups
            .Include(g => g.OwnerUser)
            .Include(g => g.Tags)
            .Include(g => g.Members)
            .ToListAsync(token);
    }
}
