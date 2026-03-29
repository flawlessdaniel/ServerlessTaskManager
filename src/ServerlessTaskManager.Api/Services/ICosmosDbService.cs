using ServerlessTaskManager.Shared.Enums;
using ServerlessTaskManager.Shared.Models;

namespace ServerlessTaskManager.Api.Services;

public interface ICosmosDbService
{
    Task<TaskItem> CreateTaskAsync(TaskItem task);

    Task<TaskItem?> GetTaskItemAsync(string id, string userId);

    Task<IEnumerable<TaskItem>> GetTasksByUserAsync(string userId);

    Task<TaskItem> UpdateTaskAsync(TaskItem task);

    Task PatchTaskStatusAsync(string id, string userId, TaskItemStatus newStatus);

    Task SoftDeleteTaskAsync(string id, string userId);

    Task<IEnumerable<TaskItem>> SearchTasksAsync(string userId, string searchTerm);

}