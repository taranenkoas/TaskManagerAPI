namespace TaskManagerAPI.Application.Interfaces;

using System.Threading;
using System.Threading.Tasks;
using TaskManagerAPI.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    ITaskItemRepository TaskItems { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
