using Microsoft.EntityFrameworkCore;
using HabitTribe.Data;
using HabitTribe.Entities;

namespace HabitTribe.Repositories;

public interface ICommentRepository : IRepository<Comment>
{
    Task<Comment?> FindWithUserAsync(int groupId, int postId, int commentId, CancellationToken token = default);
    Task<IEnumerable<Comment>> GetAllByPostIdAsync(int groupId, int postId, CancellationToken token = default);
}

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Comment?> FindWithUserAsync(int groupId, int postId, int commentId, CancellationToken token = default)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Post)
            .ThenInclude(p => p.Group)
            .FirstOrDefaultAsync(c => c.Post.GroupId == groupId && c.PostId == postId && c.Id == commentId, token);
    }

    public async Task<IEnumerable<Comment>> GetAllByPostIdAsync(int groupId, int postId, CancellationToken token = default)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.Post.GroupId == groupId && c.PostId == postId)
            .ToListAsync(token);
    }
}
