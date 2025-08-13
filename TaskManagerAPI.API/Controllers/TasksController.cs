namespace TaskManagerAPI.API.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagerAPI.Application.DTO;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Infrastructure.Data;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskItemsController(ApplicationDbContext context, IMapper mapper) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    // GET: api/taskitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDTO>>> GetAll()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tasks = await _context.TaskItems
            .Where(t => t.OwnerId == userId)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<TaskItemDTO>>(tasks));
    }

    // GET: api/taskitems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItemDTO>> Get(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var task = await _context.TaskItems
            .FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == userId);

        if (task == null)
            return NotFound();

        return Ok(_mapper.Map<TaskItemDTO>(task));
    }

    // POST: api/taskitems
    [HttpPost]
    public async Task<ActionResult<TaskItemDTO>> Create(CreateTaskItemDTO dto)
    {
        var task = _mapper.Map<TaskItem>(dto);
        _context.TaskItems.Add(task);
        task.OwnerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<TaskItemDTO>(task);

        return CreatedAtAction(nameof(Get), new { id = task.Id }, resultDto);
    }

    // PUT: api/taskitems/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TaskItemDTO dto)
    {
        var task = await _context.TaskItems.FindAsync(id);
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (task == null || task.OwnerId != userId)
            return Forbid();

        _mapper.Map(dto, task);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/taskitems/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.TaskItems.FindAsync(id);
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (task == null || task.OwnerId != userId)
            return Forbid();

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
