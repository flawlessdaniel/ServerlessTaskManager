using Microsoft.AspNetCore.Mvc;
using ServerlessTaskManager.Api.Services;
using ServerlessTaskManager.Shared.Dtos;
using ServerlessTaskManager.Shared.Enums;
using ServerlessTaskManager.Shared.Models;

namespace ServerlessTaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ICosmosDbService _cosmosDbService;

    public TasksController(ICosmosDbService cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
    }

    // GET /api/tasks?userId={userId}
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByUser([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var tasks = await _cosmosDbService.GetTasksByUserAsync(userId);
        return Ok(tasks.Select(MapToDto));
    }

    // GET /api/tasks/{id}?userId={userId}
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(string id, [FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var task = await _cosmosDbService.GetTaskItemAsync(id, userId);
        if (task is null)
            return NotFound();

        return Ok(MapToDto(task));
    }

    // GET /api/tasks/search?userId={userId}&q={searchTerm}
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> SearchTasks(
        [FromQuery] string userId,
        [FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("q (search term) is required.");

        var tasks = await _cosmosDbService.SearchTasksAsync(userId, q);
        return Ok(tasks.Select(MapToDto));
    }

    // POST /api/tasks
    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask(
        [FromQuery] string userId,
        [FromBody] CreateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var task = new TaskItem
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            AssignedTo = dto.AssignedTo,
            DueDate = dto.DueDate,
            Tags = dto.Tags ?? new List<string>()
        };

        var created = await _cosmosDbService.CreateTaskAsync(task);
        return CreatedAtAction(nameof(GetTask), new { id = created.Id, userId }, MapToDto(created));
    }

    // PUT /api/tasks/{id}?userId={userId}
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(
        string id,
        [FromQuery] string userId,
        [FromBody] UpdateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var existing = await _cosmosDbService.GetTaskItemAsync(id, userId);
        if (existing is null)
            return NotFound();

        if (dto.Title is not null) existing.Title = dto.Title;
        if (dto.Description is not null) existing.Description = dto.Description;
        if (dto.Status is not null) existing.Status = dto.Status.Value;
        if (dto.Priority is not null) existing.Priority = dto.Priority.Value;
        if (dto.AssignedTo is not null) existing.AssignedTo = dto.AssignedTo;
        if (dto.DueDate is not null) existing.DueDate = dto.DueDate;
        if (dto.Tags is not null) existing.Tags = dto.Tags;

        var updated = await _cosmosDbService.UpdateTaskAsync(existing);
        return Ok(MapToDto(updated));
    }

    // PATCH /api/tasks/{id}/status?userId={userId}
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> PatchTaskStatus(
        string id,
        [FromQuery] string userId,
        [FromBody] PatchStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var existing = await _cosmosDbService.GetTaskItemAsync(id, userId);
        if (existing is null)
            return NotFound();

        await _cosmosDbService.PatchTaskStatusAsync(id, userId, dto.Status);
        return NoContent();
    }

    // DELETE /api/tasks/{id}?userId={userId}
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDeleteTask(string id, [FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("userId is required.");

        var existing = await _cosmosDbService.GetTaskItemAsync(id, userId);
        if (existing is null)
            return NotFound();

        await _cosmosDbService.SoftDeleteTaskAsync(id, userId);
        return NoContent();
    }

    private static TaskDto MapToDto(TaskItem task) => new(
        task.Id,
        task.UserId,
        task.Title,
        task.Description,
        task.Status,
        task.Priority,
        task.AssignedTo,
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt,
        task.Tags,
        task.Attachments.Select(a => new AttachmentDto(
            a.FileName, a.BlobUri, a.ContentType, a.SizeBytes, a.UploadedAt)).ToList(),
        task.IsCompleted
    );
}

public record PatchStatusDto(TaskItemStatus Status);
