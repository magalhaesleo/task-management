using Microsoft.EntityFrameworkCore;

namespace TaskManagementApi.Infrastructure;

public class TaskManagementContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<Tasks.Task> Tasks => Set<Tasks.Task>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tasks.Task>()
            .ToTable("Tasks");
    }
}
