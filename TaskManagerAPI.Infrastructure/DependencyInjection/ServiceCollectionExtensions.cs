namespace TaskManagerAPI.Infrastructure.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Infrastructure.Repositories;
using TaskManagerAPI.Infrastructure.UnitOfWork;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services)
    {
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
