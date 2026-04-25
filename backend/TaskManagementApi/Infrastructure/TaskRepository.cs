using Microsoft.EntityFrameworkCore;

namespace TaskManagementApi.Infrastructure;

public interface ITaskRepository
{
    Task<IEnumerable<Tasks.Task>> GetTasks(CancellationToken cancellationToken);
    Task Add(Tasks.Task task, CancellationToken cancellationToken);
    Task<Tasks.Task?> GetById(Guid id, CancellationToken cancellationToken);
    Task<bool> Toggle(Guid id, bool completed, CancellationToken cancellationToken);
}

public class TaskRepository(TaskManagementContext dbContext) : ITaskRepository
{
    public async Task<IEnumerable<Tasks.Task>> GetTasks(CancellationToken cancellationToken)
    {
        return await dbContext
            .Set<Tasks.Task>()
            .OrderBy(x => x.Completed)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task Add(Tasks.Task task, CancellationToken cancellationToken)
    {
        await dbContext.Set<Tasks.Task>().AddAsync(task, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Tasks.Task?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext
            .Set<Tasks.Task>()
            .Where(x => x.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> Toggle(Guid id, bool completed, CancellationToken cancellationToken)
    {
        var updatedRows = await dbContext
            .Set<Tasks.Task>()
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(x => x.SetProperty(t => t.Completed, completed), cancellationToken);
        
        return updatedRows > 0;
    }
}
