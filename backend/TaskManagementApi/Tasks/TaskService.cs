using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Tasks;

public interface ITaskService
{
    Task<IEnumerable<Task>> GetTasks(CancellationToken cancellationToken);
}

public class TaskService(TaskManagementContext dbContext) : ITaskService
{
    public async Task<IEnumerable<Task>> GetTasks(CancellationToken cancellationToken)
    {
        return await dbContext.Set<Task>().AsNoTracking().ToListAsync(cancellationToken);
    }
}
