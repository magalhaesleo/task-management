using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Tests;

public class TaskRepositoryTests
{
    private readonly Mock<TaskManagementContext> _mockContext;
    private readonly TaskRepository _repository;
    private readonly Fixture _fixture = new();

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TaskManagementContext>().Options;
        _mockContext = new Mock<TaskManagementContext>(options);
        _mockContext
            .Setup(x => x.Set<Tasks.Task>())
            .Returns(Mock.Of<DbSet<Tasks.Task>>());
        _repository = new TaskRepository(_mockContext.Object, Mock.Of<ILogger<TaskRepository>>());
    }
    
    [Fact]
    public async Task Given_duplicate_id_should_not_throw_exception()
    {
        // Arrange
        PostgresException postgresException =
            new("duplicate key value violates unique constraint", "ERROR", string.Empty, "23505");
        var dbUpdateException = new DbUpdateException(string.Empty, postgresException);
        _mockContext
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Throws(dbUpdateException);
        var task = _fixture.Create<Tasks.Task>();
        var cancellationToken = CancellationToken.None;
        
        // Act
        var exception = await Record.ExceptionAsync(() => _repository.Add(task, cancellationToken));
        
        // Assert
        Assert.Null(exception);
    }
}
