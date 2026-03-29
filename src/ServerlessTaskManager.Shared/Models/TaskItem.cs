using System.Text.Json.Serialization;
using ServerlessTaskManager.Shared.Enums;

namespace ServerlessTaskManager.Shared.Models;

public class TaskItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("userId")]        // ← Partition Key
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("status")]
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;

    [JsonPropertyName("priority")]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [JsonPropertyName("assignedTo")]
    public string? AssignedTo { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("attachments")]
    public List<TaskAttachment> Attachments { get; set; } = new();

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    // TTL en segundos (-1 = sin expiración, se usa para soft-delete)
    [JsonPropertyName("ttl")]
    public int? Ttl { get; set; }
}