namespace TaskManagerAPI.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Infrastructure.Data;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly DbSet<T> _set = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _set.FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.AsNoTracking().ToListAsync(ct);

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public virtual void Update(T entity) => _set.Update(entity);

    public virtual void Remove(T entity) => _set.Remove(entity);
}
