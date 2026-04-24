using Microsoft.EntityFrameworkCore;

namespace TaskManagementApi.Infrastructure;

public class TaskManagementContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tasks.Task>();
    }
}
