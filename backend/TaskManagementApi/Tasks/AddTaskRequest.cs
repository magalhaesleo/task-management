using System.ComponentModel.DataAnnotations;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Tasks;

public class AddTaskRequest
{
    [NotEmptyGuid]
    public Guid Id { get; init; }
    [Required]
    [StringLength(maximumLength: 255)]
    public string? Title { get; init; }
    [StringLength(maximumLength: 1000)]
    public string? Content { get; init; }
    public Task ToTask()
    {
        return new Task()
        {
            Id = Id,
            Title = Title!,
            Content = Content,
            Completed = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}
