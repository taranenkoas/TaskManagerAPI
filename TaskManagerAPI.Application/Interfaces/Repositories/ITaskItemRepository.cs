namespace TaskManagerAPI.Application.Interfaces.Repositories;

using TaskManagerAPI.Domain.Entities;

public interface ITaskItemRepository : IGenericRepository<TaskItem>
{
    Task<IReadOnlyList<TaskItem>> GetByOwnerAsync(string ownerId, CancellationToken ct = default);
    Task<TaskItem?> GetByIdForOwnerAsync(int id, string ownerId, CancellationToken ct = default);
}
