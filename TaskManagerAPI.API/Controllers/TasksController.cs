namespace TaskManagerAPI.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Infrastructure.Data;

[ApiController]
[Route("api/[controller]")]
public class TaskItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TaskItemsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/taskitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItems()
    {
        return await _context.TaskItems.ToListAsync();
    }

    // GET: api/taskitems/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskItem(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null)
            return NotFound();

        return taskItem;
    }

    // POST: api/taskitems
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTaskItem(TaskItem taskItem)
    {
        _context.TaskItems.Add(taskItem);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTaskItem), new { id = taskItem.Id }, taskItem);
    }

    // PUT: api/taskitems/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTaskItem(int id, TaskItem taskItem)
    {
        if (id != taskItem.Id)
            return BadRequest();

        _context.Entry(taskItem).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.TaskItems.Any(e => e.Id == id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    // DELETE: api/taskitems/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTaskItem(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null)
            return NotFound();

        _context.TaskItems.Remove(taskItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}