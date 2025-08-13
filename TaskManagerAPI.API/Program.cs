namespace TaskManagerAPI.API;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerAPI.Application.Mappings;
using TaskManagerAPI.Infrastructure.Data;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connection = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connection));

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        builder.Services.AddAutoMapper(cfg => { }, typeof(TaskItemProfile).Assembly);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
