using Karkasai.Entities;

namespace Karkasai.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task AddAsync(T entity, CancellationToken token = default);
    
    Task<T?> FindAsync(int id, CancellationToken token = default);
    
    Task<IEnumerable<T>> GetAllAsync(CancellationToken token = default);

    Task RemoveAsync(T entity, CancellationToken token = default);
    
    Task SaveChangesAsync(CancellationToken token = default);
}