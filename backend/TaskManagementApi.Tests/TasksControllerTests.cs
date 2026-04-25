using System.Net;
using System.Net.Http.Json;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Tests;

public class TasksControllerTests
    : IClassFixture<TaskManagementApplicationFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    private readonly TaskManagementContext _context;
    private readonly HttpClient _client;
    private readonly Fixture _fixture = new();
    private const string GetAllTasksRoute = "tasks";

    public TasksControllerTests(TaskManagementApplicationFactory factory)
    {
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<TaskManagementContext>();
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    
    [Fact]
    public async Task Given_request_when_database_is_empty_should_return_empty_list()
    {
        // Act
        using var response = await _client.GetAsync(GetAllTasksRoute);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<Tasks.Task[]>();
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Given_request_should_have_expected_response()
    {
        // Arrange
        var task = CreateTask();
        await SeedTasks([task]);
        
        // Act
        using var response = await _client.GetAsync(GetAllTasksRoute);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<Tasks.Task[]>();
        Assert.NotNull(tasks);
        Assert.Equivalent(task, Assert.Single(tasks));
    }
    
    [Fact]
    public async Task Given_completed_and_not_completed_tasks_should_return_completed_first()
    {
        // Arrange
        var completedTask = CreateTask(completed: true);
        var notCompletedTask = CreateTask(completed: false);
        await SeedTasks([completedTask, notCompletedTask]);
        
        // Act
        using var response = await _client.GetAsync(GetAllTasksRoute);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<Tasks.Task[]>();
        Assert.NotNull(tasks);
        Assert.Collection(tasks,
            item => Assert.Equivalent(notCompletedTask.Id, item.Id),
            item => Assert.Equal(completedTask.Id, item.Id));
    }
    
    [Fact]
    public async Task Given_get_by_id_request_should_have_expected_response()
    {
        // Arrange
        var taskOne = CreateTask();
        var taskTwo = CreateTask();
        await SeedTasks([taskOne, taskTwo]);
        
        // Act
        using var response = await _client.GetAsync($"tasks/{taskTwo.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<Tasks.Task>();
        Assert.Equivalent(taskTwo, task);
    }
    
    [Fact]
    public async Task Given_get_by_id_request_when_id_is_invalid_should_return_not_found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        using var response = await _client.GetAsync($"tasks/{id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task Given_add_request_should_save_successfully()
    {
        // Arrange
        var request = _fixture.Create<Tasks.AddTaskRequest>();
        var expectedLocation = $"{_client.BaseAddress}tasks/{request.Id}";
        
        // Act
        using var response = await _client.PostAsJsonAsync("tasks", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(expectedLocation, response.Headers.Location?.ToString(), ignoreCase: true);
        var task = await response.Content.ReadFromJsonAsync<Tasks.Task>();
        Assert.Equal(request.Id, task?.Id);
        Assert.Equal(request.Title, task?.Title);
        Assert.Equal(request.Content, task?.Content);
        Assert.False(task?.Completed);
    }

    private Tasks.Task CreateTask(bool completed = false) =>
        _fixture.Build<Tasks.Task>()
            .With(x => x.Completed, completed)
            .Create();

    private async Task SeedTasks(IEnumerable<Tasks.Task> tasks)
    {
        await _context.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        var tasks = await _context.Tasks.ToListAsync();
        if (tasks.Count != 0)
        {
            _context.Tasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();
        }

        _scope.Dispose();
    }
}
