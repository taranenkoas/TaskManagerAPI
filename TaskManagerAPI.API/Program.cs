namespace TaskManagerAPI.API;

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TaskManagerAPI.API.Filters;
using TaskManagerAPI.Application.DTO;
using TaskManagerAPI.Application.Mappings;
using TaskManagerAPI.Application.Validators;
using TaskManagerAPI.Infrastructure.Data;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Host.UseSerilog();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connection));
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter<CreateTaskItemDTO>>();
            });

            builder.Services.AddOpenApi();
            builder.Services.AddAutoMapper(cfg => { }, typeof(TaskItemProfile).Assembly);
            builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskItemDTOValidator>();

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
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
