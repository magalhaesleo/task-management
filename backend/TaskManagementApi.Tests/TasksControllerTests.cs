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
    private const string TasksRoute = "tasks";

    public TasksControllerTests(TaskManagementApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<TaskManagementContext>();
    }

    public async ValueTask InitializeAsync()
    {
        await _context.Database.MigrateAsync();
    }
    
    [Fact]
    public async Task Given_get_all_request_when_database_is_empty_should_return_empty_list()
    {
        // Act
        using var response = await _client.GetAsync(TasksRoute, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<Tasks.Task[]>(TestContext.Current.CancellationToken);
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task Given_get_all_request_when_there_is_task_should_have_expected_response()
    {
        // Arrange
        var task = CreateTask();
        await SeedTasks([task]);
        
        // Act
        using var response = await _client.GetAsync(TasksRoute, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<Tasks.Task[]>(TestContext.Current.CancellationToken);
        Assert.NotNull(tasks);
        var taskResponse = Assert.Single(tasks);
        Assert.EquivalentWithExclusions(task, taskResponse, x => x.CreatedAt);
        Assert.Equal(task.CreatedAt, taskResponse.CreatedAt, TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public async Task Given_completed_and_not_completed_tasks_should_return_completed_first()
    {
        // Arrange
        var completedTask = CreateTask(completed: true);
        var notCompletedTask = CreateTask(completed: false);
        await SeedTasks([completedTask, notCompletedTask]);
        
        // Act
        using var response = await _client.GetAsync(TasksRoute, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<Tasks.Task[]>(TestContext.Current.CancellationToken);
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
        using var response = await _client.GetAsync($"{TasksRoute}/{taskTwo.Id}", TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<Tasks.Task>(TestContext.Current.CancellationToken);
        Assert.NotNull(task);
        Assert.EquivalentWithExclusions(taskTwo, task, x => x.CreatedAt);
        Assert.Equal(taskTwo.CreatedAt, task.CreatedAt, TimeSpan.FromSeconds(1));
    }
    
    [Fact]
    public async Task Given_get_by_id_request_when_id_is_invalid_should_return_not_found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        using var response = await _client.GetAsync($"{TasksRoute}/{id}", TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task Given_add_request_should_save_successfully()
    {
        // Arrange
        var request = _fixture.Create<Tasks.AddTaskRequest>();
        var expectedLocation = $"{_client.BaseAddress}{TasksRoute}/{request.Id}";
        
        // Act
        using var response = await _client.PostAsJsonAsync(TasksRoute, request, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(expectedLocation, response.Headers.Location?.ToString(), ignoreCase: true);
        var task = await response.Content.ReadFromJsonAsync<Tasks.Task>(TestContext.Current.CancellationToken);
        Assert.Equal(request.Id, task?.Id);
        Assert.Equal(request.Title, task?.Title);
        Assert.Equal(request.Content, task?.Content);
        Assert.False(task?.Completed);
    }
    
        
    [Fact]
    public async Task Given_add_request_when_idempotence_conflict_occurs_should_return_created_status_code()
    {
        // Arrange
        var databaseTask = CreateTask();
        await SeedTasks([databaseTask]);
        var request = _fixture.Build<Tasks.AddTaskRequest>()
            .With(x => x.Id, databaseTask.Id)
            .Create();
        var expectedLocation = $"{_client.BaseAddress}{TasksRoute}/{request.Id}";
        
        // Act
        using var response = await _client.PostAsJsonAsync(TasksRoute, request, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(expectedLocation, response.Headers.Location?.ToString(), ignoreCase: true);
        var task = await response.Content.ReadFromJsonAsync<Tasks.Task>(TestContext.Current.CancellationToken);
        Assert.Equal(request.Id, task?.Id);
        Assert.Equal(request.Title, task?.Title);
        Assert.Equal(request.Content, task?.Content);
        Assert.False(task?.Completed);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_toggle_request_should_have_expected_response(bool completed)
    {
        // Arrange
        var task = CreateTask(completed: !completed);
        await SeedTasks([task]);
        
        // Act
        using var response = await _client.PatchAsJsonAsync($"{TasksRoute}/{task.Id}", completed, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedTask = await _context.Tasks.Where(x => x.Id == task.Id).AsNoTracking()
            .SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(updatedTask.Completed, completed);
    }
    
    [Fact]
    public async Task Given_toggle_request_when_an_invalid_id_is_sent_should_return_not_found()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        using var response = await _client.PatchAsJsonAsync($"{TasksRoute}/{id}", true, TestContext.Current.CancellationToken);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Tasks.Task CreateTask(bool completed = false) =>
        _fixture.Build<Tasks.Task>()
            .With(x => x.Completed, completed)
            .With(x => x.CreatedAt, DateTime.UtcNow)
            .Create();

    private async Task SeedTasks(IEnumerable<Tasks.Task> tasks)
    {
        await _context.AddRangeAsync(tasks);
        await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.Tasks.ExecuteDeleteAsync();
        _scope.Dispose();
    }
}
