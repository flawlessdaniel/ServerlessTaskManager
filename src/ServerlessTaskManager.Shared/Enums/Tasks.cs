using System.Text.Json.Serialization;

namespace ServerlessTaskManager.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskItemStatus
{
    Pending,
    InProgress,
    WaitingApproval,
    Completed,
    Expired,
    Cancelled
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}