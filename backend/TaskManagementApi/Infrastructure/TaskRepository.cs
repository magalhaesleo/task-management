using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace TaskManagementApi.Infrastructure;

public interface ITaskRepository
{
    Task<IEnumerable<Tasks.Task>> GetTasks(CancellationToken cancellationToken);
    Task Add(Tasks.Task task, CancellationToken cancellationToken);
    Task<Tasks.Task?> GetById(Guid id, CancellationToken cancellationToken);
    Task<bool> Toggle(Guid id, bool completed, CancellationToken cancellationToken);
}

public class TaskRepository(
    TaskManagementContext dbContext,
    ILogger<TaskRepository> logger) : ITaskRepository
{
    public async Task<IEnumerable<Tasks.Task>> GetTasks(CancellationToken cancellationToken)
    {
        var c = dbContext.Database.GetDbConnection().ConnectionString;
        return await dbContext
            .Tasks
            .OrderBy(x => x.Completed)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task Add(Tasks.Task task, CancellationToken cancellationToken)
    {
        try
        {
            var c = dbContext.Database.GetDbConnection().ConnectionString;
            await dbContext.Tasks.AddAsync(task, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            // We don't care about duplicate keys
            logger.LogWarning(ex, "Duplicate key exception while adding task.");
        }
    }

    public async Task<Tasks.Task?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext
            .Tasks
            .Where(x => x.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> Toggle(Guid id, bool completed, CancellationToken cancellationToken)
    {
        var updatedRows = await dbContext
            .Tasks
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(x => x.SetProperty(t => t.Completed, completed), cancellationToken);
        
        return updatedRows > 0;
    }
}
