namespace TaskManagerAPI.Infrastructure.UnitOfWork;

using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Infrastructure.Data;

public class UnitOfWork(ApplicationDbContext context, ITaskItemRepository taskItemRepository) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;

    public ITaskItemRepository TaskItems { get; } = taskItemRepository;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
