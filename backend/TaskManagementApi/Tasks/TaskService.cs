using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Tasks;

public interface ITaskService
{
    Task<IEnumerable<Task>> GetTasks(CancellationToken cancellationToken);
    System.Threading.Tasks.Task Add(Task task, CancellationToken cancellationToken);
    Task<Task?> GetById(Guid id, CancellationToken cancellationToken);
    Task<bool> Toggle(Guid id, bool completed, CancellationToken cancellationToken);
}

public class TaskService(TaskManagementContext dbContext) : ITaskService
{
    public async Task<IEnumerable<Task>> GetTasks(CancellationToken cancellationToken)
    {
        return await dbContext
            .Set<Task>()
            .OrderBy(x => x.Completed)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task Add(Task task, CancellationToken cancellationToken)
    {
        await dbContext.Set<Task>().AddAsync(task, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Task?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext
            .Set<Task>()
            .Where(x => x.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> Toggle(Guid id, bool completed, CancellationToken cancellationToken)
    {
        var updatedRows = await dbContext
            .Set<Task>()
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(x => x.SetProperty(t => t.Completed, completed), cancellationToken);
        
        return updatedRows > 0;
    }
}
