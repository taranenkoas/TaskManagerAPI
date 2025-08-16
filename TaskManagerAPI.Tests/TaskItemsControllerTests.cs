namespace TaskManagerAPI.Tests;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using TaskManagerAPI.API.Controllers;
using TaskManagerAPI.API.Settings;
using TaskManagerAPI.Application.DTO;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Domain.Entities;
using Xunit;

public class TaskItemsControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly IOptions<RabbitMQSettings> _rabbitMQSettings;
    private readonly TaskItemsController _controller;

    public TaskItemsControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockCache = new Mock<IDistributedCache>();

        _rabbitMQSettings = Options.Create(new RabbitMQSettings
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        });

        _controller = new TaskItemsController(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockCache.Object,
            _rabbitMQSettings);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithTaskItems()
    {
        // Arrange
        var userId = "test-user-id";
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Test Task", OwnerId = userId }
        };

        var taskDtos = new List<TaskItemDTO>
        {
            new() { Id = 1, Title = "Test Task" }
        };

        var mockTaskRepository = new Mock<ITaskItemRepository>();
        mockTaskRepository.Setup(x => x.GetByOwnerAsync(userId))
            .ReturnsAsync(tasks);

        _mockUnitOfWork.Setup(x => x.TaskItems)
            .Returns(mockTaskRepository.Object);

        _mockMapper.Setup(x => x.Map<IEnumerable<TaskItemDTO>>(tasks))
            .Returns(taskDtos);

        // Исправлено: мокируем GetAsync вместо GetStringAsync
        _mockCache.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        SetUserContext(userId);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var returnValue = Assert.IsType<List<TaskItemDTO>>(okResult.Value);
        Assert.Single(returnValue);
        Assert.Equal("Test Task", returnValue[0].Title);
    }

    [Fact]
    public async Task Get_ReturnsOk_WithTaskItem()
    {
        // Arrange
        var userId = "test-user-id";
        var taskId = 1;
        var task = new TaskItem { Id = taskId, Title = "Test Task", OwnerId = userId };
        var taskDto = new TaskItemDTO { Id = taskId, Title = "Test Task" };

        var mockTaskRepository = new Mock<ITaskItemRepository>();
        mockTaskRepository.Setup(x => x.GetByIdForOwnerAsync(taskId, userId))
            .ReturnsAsync(task);

        _mockUnitOfWork.Setup(x => x.TaskItems)
            .Returns(mockTaskRepository.Object);

        _mockMapper.Setup(x => x.Map<TaskItemDTO>(task))
            .Returns(taskDto);

        SetUserContext(userId);

        // Act
        var result = await _controller.Get(taskId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.NotNull(okResult);
        var returnValue = Assert.IsType<TaskItemDTO>(okResult.Value);
        Assert.Equal(taskId, returnValue.Id);
        Assert.Equal("Test Task", returnValue.Title);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenTaskIsCreated()
    {
        // Arrange
        var userId = "test-user-id";
        var createDto = new CreateTaskItemDTO { Title = "New Task" };
        var task = new TaskItem { Id = 1, Title = "New Task", OwnerId = userId };
        var taskDto = new TaskItemDTO { Id = 1, Title = "New Task" };

        var mockTaskRepository = new Mock<ITaskItemRepository>();
        _mockUnitOfWork.Setup(x => x.TaskItems)
            .Returns(mockTaskRepository.Object);

        _mockMapper.Setup(x => x.Map<TaskItem>(createDto))
            .Returns(task);
        _mockMapper.Setup(x => x.Map<TaskItemDTO>(task))
            .Returns(taskDto);

        SetUserContext(userId);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.NotNull(createdResult);
        var returnValue = Assert.IsType<TaskItemDTO>(createdResult.Value);
        Assert.Equal(1, returnValue.Id);
        Assert.Equal("New Task", returnValue.Title);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenTaskIsUpdated()
    {
        // Arrange
        var userId = "test-user-id";
        var taskId = 1;
        var taskDto = new TaskItemDTO { Id = taskId, Title = "Updated Task" };
        var existingTask = new TaskItem { Id = taskId, Title = "Original Task", OwnerId = userId };

        var mockTaskRepository = new Mock<ITaskItemRepository>();
        mockTaskRepository.Setup(x => x.GetByIdForOwnerAsync(taskId, userId))
            .ReturnsAsync(existingTask);

        _mockUnitOfWork.Setup(x => x.TaskItems)
            .Returns(mockTaskRepository.Object);

        SetUserContext(userId);

        // Act
        var result = await _controller.Update(taskId, taskDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockMapper.Verify(x => x.Map(taskDto, existingTask), Times.Once);
        mockTaskRepository.Verify(x => x.Update(existingTask), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenTaskIsDeleted()
    {
        // Arrange
        var userId = "test-user-id";
        var taskId = 1;
        var task = new TaskItem { Id = taskId, Title = "Task to delete", OwnerId = userId };

        var mockTaskRepository = new Mock<ITaskItemRepository>();
        mockTaskRepository.Setup(x => x.GetByIdForOwnerAsync(taskId, userId))
            .ReturnsAsync(task);

        _mockUnitOfWork.Setup(x => x.TaskItems)
            .Returns(mockTaskRepository.Object);

        SetUserContext(userId);

        // Act
        var result = await _controller.Delete(taskId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockTaskRepository.Verify(x => x.Remove(task), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetUserContext(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}
