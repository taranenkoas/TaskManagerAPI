namespace TaskManagerAPI.API;

using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using TaskManagerAPI.API.Filters;
using TaskManagerAPI.Application.DTO;
using TaskManagerAPI.Application.Mappings;
using TaskManagerAPI.Application.Validators;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Infrastructure.Data;
using TaskManagerAPI.Infrastructure.DependencyInjection;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_key_123";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskManagerAPI";

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
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

            builder.Services.AddDataAccess();

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
