using System.ComponentModel.DataAnnotations;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Tasks;

public record AddTaskRequest(
    [NotEmptyGuid]
    Guid Id,
    [StringLength(maximumLength: 255)]
    string Title,
    [StringLength(maximumLength: 1000)]
    string Content)
{
    public Task ToTask()
    {
        return new Task()
        {
            Id = Id,
            Title = Title,
            Content = Content,
            Completed = false
        };
    }
}
