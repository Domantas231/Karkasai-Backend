using Microsoft.EntityFrameworkCore;
using HabitTribe.Data;
using HabitTribe.Entities;

namespace HabitTribe.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task AddAsync(T entity, CancellationToken token = default);
    Task<T?> FindAsync(int id, CancellationToken token = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken token = default);
    IQueryable<T> Query();
    Task RemoveAsync(T entity, CancellationToken token = default);
    Task SaveChangesAsync(CancellationToken token = default);
}

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(T entity, CancellationToken token = default)
    {
        await _context.Set<T>().AddAsync(entity, token);
    }

    public async Task<T?> FindAsync(int id, CancellationToken token = default)
    {
        return await _context.Set<T>().FindAsync(keyValues: [id], cancellationToken: token);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken token = default)
    {
        return await _context.Set<T>().ToListAsync(token);
    }

    public IQueryable<T> Query()
    {
        return _context.Set<T>().AsQueryable();
    }

    public Task RemoveAsync(T entity, CancellationToken token = default)
    {
        _context.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken token = default)
    {
        await _context.SaveChangesAsync(token);
    }
}
