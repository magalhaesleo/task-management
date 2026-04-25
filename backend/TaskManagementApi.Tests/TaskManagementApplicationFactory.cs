using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi.Tests;

public class TaskManagementApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == 
                     typeof(IDbContextOptionsConfiguration<TaskManagementContext>));

            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbConnection));

            if (dbConnectionDescriptor is not null)
                services.Remove(dbConnectionDescriptor);
            
            var context = services.SingleOrDefault(d => d.ServiceType == typeof(TaskManagementContext));
            if (context is not null)
                services.Remove(context);

            services.AddDbContext<TaskManagementContext>(opt => opt.UseInMemoryDatabase("TaskManagementDb"));
        });
    }
}