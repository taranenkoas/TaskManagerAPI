namespace TaskManagerAPI.API.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TaskManagerAPI.API.Settings;
using TaskManagerAPI.Application.DTO;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Entities;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskItemsController(
    IUnitOfWork uow,
    IMapper mapper,
    IDistributedCache cache,
    IOptions<RabbitMQSettings> rabbitMQSettings)
    : ControllerBase
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IMapper _mapper = mapper;
    private readonly IDistributedCache _cache = cache;
    private readonly RabbitMQSettings _rabbitMQSettings = rabbitMQSettings.Value;

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDTO>>> GetAll()
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var cacheKey = $"task_{CurrentUserId}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
            return Ok(JsonSerializer.Deserialize<List<TaskItemDTO>>(cachedData)!);

        var tasks = await _uow.TaskItems.GetByOwnerAsync(CurrentUserId);
        var tasksDto = _mapper.Map<IEnumerable<TaskItemDTO>>(tasks);

        var jsonData = JsonSerializer.Serialize(tasksDto);
        await _cache.SetStringAsync(
            cacheKey,
            jsonData,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return Ok(tasksDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItemDTO>> Get(int id)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var task = await _uow.TaskItems.GetByIdForOwnerAsync(id, CurrentUserId);
        if (task is null)
            return NotFound();

        return Ok(_mapper.Map<TaskItemDTO>(task));
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDTO>> Create(CreateTaskItemDTO dto)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var task = _mapper.Map<TaskItem>(dto);
        task.OwnerId = CurrentUserId;

        await _uow.TaskItems.AddAsync(task);
        await _uow.SaveChangesAsync();

        var factory = new ConnectionFactory()
        {
            HostName = _rabbitMQSettings.HostName,
            UserName = _rabbitMQSettings.UserName,
            Password = _rabbitMQSettings.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "task_events",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var message = $"TaskCreated: {task.Id} - {task.Title}";
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "task_events",
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body);

        var resultDto = _mapper.Map<TaskItemDTO>(task);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TaskItemDTO dto)
    {
        if (CurrentUserId is null)
            return Unauthorized();
        if (id != dto.Id)
            return BadRequest("ID в пути и в теле не совпадают.");

        var task = await _uow.TaskItems.GetByIdForOwnerAsync(id, CurrentUserId);
        if (task is null)
            return NotFound();

        _mapper.Map(dto, task);
        _uow.TaskItems.Update(task);
        await _uow.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var task = await _uow.TaskItems.GetByIdForOwnerAsync(id, CurrentUserId);
        if (task is null)
            return NotFound();

        _uow.TaskItems.Remove(task);
        await _uow.SaveChangesAsync();

        return NoContent();
    }
}
