using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TaskManagementApi.Infrastructure;

namespace TaskManagementApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Open telemetry instrumentation
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .UseOtlpExporter()
            .WithTracing(x => x.AddAspNetCoreInstrumentation().AddNpgsql())
            .WithMetrics(x => x.AddAspNetCoreInstrumentation().AddNpgsqlInstrumentation());

        var postgresConnection = builder.Configuration.GetConnectionString("PostgresConnection")!;

        // Add health checks
        builder.Services.AddHealthChecks().AddNpgSql(postgresConnection, name: "Postgres", tags: ["db"]);

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<TaskManagementContext>(opt => opt.UseNpgsql(postgresConnection));
        builder.Services.AddTransient<ITaskRepository, TaskRepository>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        app.MapHealthChecks("/health");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.UseCors("AllowFrontend");
        
        app.MapControllers();

        app.Run();
    }
}