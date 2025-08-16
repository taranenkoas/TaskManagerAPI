namespace TaskManagerAPI.Tests;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using TaskManagerAPI.API.Controllers;
using TaskManagerAPI.Application.DTO.Auth;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Infrastructure.Data;
using Xunit;

public class AuthControllerTests
{
    private readonly IConfiguration _config;

    public AuthControllerTests()
    {
        // Настраиваем JWT-конфиг
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Key", "super_secret_key_1234567890_super_secret_key"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenUserCreatedSuccessfully()
    {
        // Arrange
        var userManager = GetUserManager();
        var controller = new AuthController(userManager, _config);

        var dto = new RegisterDTO
        {
            Email = "new@example.com",
            Password = "Password123!",
            FullName = "New User"
        };

        // Act
        var result = await controller.Register(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Contains("User registered successfully", okResult.Value!.ToString()!);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithValidCredentials()
    {
        // Arrange
        var userManager = GetUserManager();
        var controller = new AuthController(userManager, _config);

        var user = new User { UserName = "testuser", Email = "test@example.com" };
        await userManager.CreateAsync(user, "Password123!");

        var dto = new LoginDTO { Email = "test@example.com", Password = "Password123!" };

        // Act
        var result = await controller.Login(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("token", okResult.Value!.ToString()!);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WithInvalidCredentials()
    {
        // Arrange
        var userManager = GetUserManager();
        var controller = new AuthController(userManager, _config);

        var dto = new LoginDTO { Email = "wrong@example.com", Password = "wrongpass" };

        // Act
        var result = await controller.Login(dto);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(401, unauthorized.StatusCode);
    }

    private static UserManager<User> GetUserManager()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // уникальная база для каждого теста
            .Options;

        var context = new ApplicationDbContext(options);

        var store = new UserStore<User>(context);
        var userManager = new UserManager<User>(
            store,
            null!,
            new PasswordHasher<User>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            null!
        );

        return userManager;
    }
}
