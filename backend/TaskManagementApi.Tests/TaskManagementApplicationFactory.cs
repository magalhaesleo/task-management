using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace TaskManagementApi.Tests;

public class TaskManagementApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:18.3")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:PostgresConnection", _postgreSqlContainer.GetConnectionString());

    public new async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
    }
}
