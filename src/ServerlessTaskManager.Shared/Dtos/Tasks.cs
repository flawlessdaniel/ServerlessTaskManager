using ServerlessTaskManager.Shared.Enums;

namespace ServerlessTaskManager.Shared.Dtos;

public record CreateTaskDto(
    string Title,
    string? Description,
    TaskPriority Priority = TaskPriority.Medium,
    string? AssignedTo = null,
    DateTime? DueDate = null,
    List<string>? Tags = null
);

public record UpdateTaskDto(
    string? Title = null,
    string? Description = null,
    TaskItemStatus? Status = null,
    TaskPriority? Priority = null,
    string? AssignedTo = null,
    DateTime? DueDate = null,
    List<string>? Tags = null
);

public record TaskDto(
    string Id,
    string UserId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    string? AssignedTo,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<string> Tags,
    List<AttachmentDto> Attachments,
    bool IsCompleted
);

public record AttachmentDto(
    string FileName,
    string BlobUri,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAt
);