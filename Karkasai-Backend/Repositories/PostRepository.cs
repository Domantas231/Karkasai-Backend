using Microsoft.EntityFrameworkCore;
using HabitTribe.Data;
using HabitTribe.Entities;

namespace HabitTribe.Repositories;

public interface IPostRepository : IRepository<Post>
{
    Task<Post?> FindWithUserAsync(int groupId, int postId, CancellationToken token = default);
    Task<IEnumerable<Post>> GetAllByGroupIdAsync(int groupId, CancellationToken token = default);
}

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Post?> FindWithUserAsync(int groupId, int postId, CancellationToken token = default)
    {
        return await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.GroupId == groupId && p.Id == postId, token);
    }

    public async Task<IEnumerable<Post>> GetAllByGroupIdAsync(int groupId, CancellationToken token = default)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Where(p => p.GroupId == groupId)
            .ToListAsync(token);
    }
}
