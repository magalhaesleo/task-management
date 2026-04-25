namespace TaskManagementApi.Tasks;

public class Task
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string? Content { get; init; }
    public required bool Completed { get; init; }
}
