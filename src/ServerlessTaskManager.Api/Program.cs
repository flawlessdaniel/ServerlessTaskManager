using Microsoft.Azure.Cosmos;
using ServerlessTaskManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var connectionString = builder.Configuration["CosmosDb:ConnectionString"];

    if (string.IsNullOrWhiteSpace(connectionString) || !connectionString.StartsWith("AccountEndpoint="))
        throw new InvalidOperationException(
            "CosmosDb:ConnectionString is missing or invalid. " +
            "Expected format: AccountEndpoint=https://...;AccountKey=...;");

    return new CosmosClient(connectionString, new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            IgnoreNullValues = true
        }
    });
});

builder.Services.AddSingleton<ICosmosDbService>(sp =>
{
    var client = sp.GetRequiredService<CosmosClient>();
    return new CosmosDbService(client, "ServerlessTaskManager", "Tasks");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
