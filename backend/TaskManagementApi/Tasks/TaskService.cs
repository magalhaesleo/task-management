using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Tasks;

public interface ITaskService
{
    Task<IEnumerable<Task>> GetTasks(CancellationToken cancellationToken);
    Task<Task> Add(AddTaskRequest taskRequest, CancellationToken cancellationToken);
    Task<Task?> GetById(Guid id, CancellationToken cancellationToken);
}

public class TaskService(TaskManagementContext dbContext) : ITaskService
{
    public async Task<IEnumerable<Task>> GetTasks(CancellationToken cancellationToken)
    {
        return await dbContext
            .Set<Task>()
            .OrderBy(x => x.IsFlagged)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Task> Add(AddTaskRequest taskRequest, CancellationToken cancellationToken)
    {
        var task = taskRequest.ToTask();
        await dbContext.Set<Task>().AddAsync(task, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<Task?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext
            .Set<Task>()
            .Where(x => x.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
