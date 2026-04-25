using System.Net.Http.Json;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Tests;

public class TasksControllerTests(TaskManagementApplicationFactory factory)
    : IClassFixture<TaskManagementApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task Given_valid_request_should_not_have_failure()
    {
        // Arrange
        var task = _fixture.Create<Tasks.Task>();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskManagementContext>();
        await context.AddAsync(task);
        await context.SaveChangesAsync();
        
        // Act
        var tasks = await _client.GetFromJsonAsync<Tasks.Task[]>("tasks");
        
        // Assert
        Assert.NotNull(tasks);
        Assert.Equivalent(task, Assert.Single(tasks));
    }
}
