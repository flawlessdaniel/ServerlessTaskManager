using Microsoft.Azure.Cosmos;
using ServerlessTaskManager.Shared.Enums;
using ServerlessTaskManager.Shared.Models;

namespace ServerlessTaskManager.Api.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    // CREATE — Point write (bajo RU cost)
    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        var response = await _container.CreateItemAsync(
            task,
            new PartitionKey(task.UserId));

        return response.Resource;
    }

    // READ — Point read (1 RU para documentos < 1KB, el más eficiente)
    public async Task<TaskItem?> GetTaskItemAsync(string id, string userId)
    {
        try
        {   
            var response = await _container.ReadItemAsync<TaskItem>(
                id,
                new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    // QUERY — Cross-partition query (más costoso en RU/s)
    public async Task<IEnumerable<TaskItem>> GetTasksByUserAsync(string userId)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC")
            .WithParameter("@userId", userId);

        var iterator = _container.GetItemQueryIterator<TaskItem>(
            queryDefinition: query,
            requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(userId),
                MaxItemCount = 50  // Paginación
            });

        var results = new List<TaskItem>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    // PATCH — Partial update (modifica solo campos específicos, menos RU/s)
    public async Task PatchTaskStatusAsync(string id, string userId, TaskItemStatus newStatus)
    {
        var patchOperations = new List<PatchOperation>
        {
            PatchOperation.Set("/status", newStatus.ToString()),
            PatchOperation.Set("/updatedAt", DateTime.UtcNow)
        };

        await _container.PatchItemAsync<TaskItem>(
            id,
            new PartitionKey(userId),
            patchOperations);
    }

    // QUERY con LINQ (alternativa)
    public async Task<IEnumerable<TaskItem>> SearchTasksAsync(string userId, string searchTerm)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.userId = @userId AND CONTAINS(LOWER(c.title), LOWER(@search))")
            .WithParameter("@userId", userId)
            .WithParameter("@search", searchTerm);

        var iterator = _container.GetItemQueryIterator<TaskItem>(query);
        var results = new List<TaskItem>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    // DELETE — Soft delete con TTL (el documento expira automáticamente)
    public async Task SoftDeleteTaskAsync(string id, string userId)
    {
        var patchOperations = new List<PatchOperation>
        {
            PatchOperation.Set("/ttl", 60 * 60 * 24 * 30), // 30 días
            PatchOperation.Set("/status", TaskItemStatus.Cancelled.ToString())
        };

        await _container.PatchItemAsync<TaskItem>(
            id,
            new PartitionKey(userId),
            patchOperations);
    }

    // UPDATE — Replace (reemplaza documento completo)
    public async Task<TaskItem> UpdateTaskAsync(TaskItem task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        var response = await _container.ReplaceItemAsync(
            task,
            task.Id,
            new PartitionKey(task.UserId));
        return response.Resource;
    }
}