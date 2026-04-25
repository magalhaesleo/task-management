namespace TaskManagementApi.Tasks;

public record AddTaskRequest(Guid Id, string Title, string Content)
{
    public Task ToTask()
    {
        return new Task()
        {
            Id = Id,
            Title = Title,
            Content = Content,
            IsFlagged = false
        };
    }
}
