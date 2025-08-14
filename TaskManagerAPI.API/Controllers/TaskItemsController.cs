namespace TaskManagerAPI.API.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagerAPI.Application.DTO;
using TaskManagerAPI.Application.Interfaces;
using TaskManagerAPI.Domain.Entities;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskItemsController(IUnitOfWork uow, IMapper mapper) : ControllerBase
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IMapper _mapper = mapper;

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDTO>>> GetAll(CancellationToken ct)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var tasks = await _uow.TaskItems.GetByOwnerAsync(CurrentUserId, ct);
        return Ok(_mapper.Map<IEnumerable<TaskItemDTO>>(tasks));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItemDTO>> Get(int id, CancellationToken ct)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var task = await _uow.TaskItems.GetByIdForOwnerAsync(id, CurrentUserId, ct);
        if (task is null)
            return NotFound();

        return Ok(_mapper.Map<TaskItemDTO>(task));
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDTO>> Create(CreateTaskItemDTO dto, CancellationToken ct)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var task = _mapper.Map<TaskItem>(dto);
        task.OwnerId = CurrentUserId;

        await _uow.TaskItems.AddAsync(task, ct);
        await _uow.SaveChangesAsync(ct);

        var resultDto = _mapper.Map<TaskItemDTO>(task);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, resultDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TaskItemDTO dto, CancellationToken ct)
    {
        if (CurrentUserId is null)
            return Unauthorized();
        if (id != dto.Id)
            return BadRequest("ID в пути и в теле не совпадают.");

        var task = await _uow.TaskItems.GetByIdForOwnerAsync(id, CurrentUserId, ct);
        if (task is null)
            return NotFound();

        _mapper.Map(dto, task);
        _uow.TaskItems.Update(task);
        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (CurrentUserId is null)
            return Unauthorized();

        var task = await _uow.TaskItems.GetByIdForOwnerAsync(id, CurrentUserId, ct);
        if (task is null)
            return NotFound();

        _uow.TaskItems.Remove(task);
        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }
}
