namespace TaskManagerAPI.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Infrastructure.Data;

public class TaskItemRepository(ApplicationDbContext context) : GenericRepository<TaskItem>(context), ITaskItemRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetByOwnerAsync(string ownerId)
        => await _set.AsNoTracking()
            .Where(t => t.OwnerId == ownerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<TaskItem?> GetByIdForOwnerAsync(int id, string ownerId)
        => await _set.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId);
}
