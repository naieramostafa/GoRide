using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Constants;
using RideSharing.Core.Enums;

namespace RideSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public TasksController(IUnitOfWork uow) => _uow = uow;

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var tasks = await _uow.Tasks.GetAllAsync();
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> GetById(Guid id)
    {
        var task = await _uow.Tasks.GetByIdAsync(id);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateTaskDto dto)
    {
        var task = new Core.Entities.TaskItem(dto.Title, dto.Description, dto.Priority, dto.AssignedToUserId);
        await _uow.Tasks.AddAsync(task);
        await _uow.CommitAsync();
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
    {
        var task = await _uow.Tasks.GetByIdAsync(id);
        if (task == null) return NotFound();

        switch (status.ToLowerInvariant())
        {
            case "pending":
                return Ok(new { message = "Task already pending", status });
            case "inprogress":
            case "in_progress":
                if (task.Status != TaskItemStatus.Pending)
                    return BadRequest(new { error = "Only pending tasks can be started" });
                task.Start();
                break;
            case "completed":
                if (task.Status != TaskItemStatus.InProgress)
                    return BadRequest(new { error = "Only in-progress tasks can be completed" });
                task.Complete();
                break;
            case "cancelled":
                if (task.Status == TaskItemStatus.Completed)
                    return BadRequest(new { error = "Completed tasks cannot be cancelled" });
                task.Cancel();
                break;
            default:
                return BadRequest(new { error = $"Invalid status: {status}. Valid values: pending, in_progress, completed, cancelled" });
        }

        await _uow.CommitAsync();
        return Ok(new { message = "Task status updated", status });
    }
}

public record CreateTaskDto(string Title, string Description, Guid AssignedToUserId, TaskPriority Priority);
