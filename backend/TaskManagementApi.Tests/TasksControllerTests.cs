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
    private const string GetAllTasksRoute = "tasks";
    
    [Fact]
    public async Task Given_request_when_database_is_empty_should_return_empty_list()
    {
        // Act
        var tasks = await _client.GetFromJsonAsync<Tasks.Task[]>(GetAllTasksRoute);
        
        // Assert
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Given_request_should_have_expected_response()
    {
        // Arrange
        var task = _fixture.Create<Tasks.Task>();
        await SeedTasks([task]);
        
        // Act
        var tasks = await _client.GetFromJsonAsync<Tasks.Task[]>(GetAllTasksRoute);
        
        // Assert
        Assert.NotNull(tasks);
        Assert.Equivalent(task, Assert.Single(tasks));
    }
    
    [Fact]
    public async Task Given_completed_and_not_completed_tasks_should_return_completed_first()
    {
        // Arrange
        var completedTask = _fixture.Build<Tasks.Task>()
            .With(x => x.Completed, true)
            .Create();
        var notCompletedTask = _fixture.Build<Tasks.Task>()
            .With(x => x.Completed, false)
            .Create();
        await SeedTasks([completedTask, notCompletedTask]);
        
        // Act
        var tasks = await _client.GetFromJsonAsync<Tasks.Task[]>(GetAllTasksRoute);
        
        // Assert
        Assert.NotNull(tasks);
        Assert.Collection(tasks,
            item => Assert.Equivalent(notCompletedTask.Id, item.Id),
            item => Assert.Equal(completedTask.Id, item.Id));
    }

    private async Task SeedTasks(IEnumerable<Tasks.Task> tasks)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskManagementContext>();
        await context.AddRangeAsync(tasks);
        await context.SaveChangesAsync();
    }
}
