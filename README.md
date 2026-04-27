# Task Management

A full-stack task management application built with ASP.NET Core (backend) and React (frontend), containerized with Docker Compose.

---

## Table of Contents

- [Running the App](#running-the-app)
- [Project Structure](#project-structure)
- [Observability](#observability)
- [Database Migrations](#database-migrations)
- [Idempotent Task Creation](#idempotent-task-creation)
- [Tests](#tests)
- [Next Steps](#next-steps)

---

## Running the App

### Prerequisites

- [Docker](https://www.docker.com/) with Compose

### Start

```bash
docker compose up --build
```

This single command starts all services:

| Service | Description | URL |
|---|---|---|
| `frontend` | React UI | http://localhost:5173 |
| `task-management-api` | ASP.NET Core REST API | http://localhost:5000 |
| `db` | PostgreSQL 18.3 | `localhost:5432` |
| `migration` | EF Core migration runner | — |
| `aspire-dashboard` | Observability dashboard | http://localhost:18888 |

### Service startup order

Docker Compose enforces a strict dependency chain:

```
db (healthy) → migration (completes) → task-management-api → frontend
                                                           ↑
                                          aspire-dashboard (independent)
```

1. **`db`** starts first; a health check confirms PostgreSQL is ready to accept connections.
2. **`migration`** runs the pre-built EF Core migrations bundle (`efbundle`) against the database and exits.
3. **`task-management-api`** starts only after migrations complete, ensuring the schema is always up to date before the application accepts requests.
4. **`aspire-dashboard`** starts independently and receives telemetry from the API over OTLP/gRPC on port 18889.

---

## Project Structure

```
task-management/
├── compose.yaml
├── backend/
│   ├── TaskManagementApi/           # ASP.NET Core 10 Web API
│   │   ├── Controllers/             # HTTP endpoints
│   │   ├── Infrastructure/          # DbContext, repository, validation
│   │   ├── Tasks/                   # Domain models and request DTOs
│   │   ├── Migrations/              # EF Core migration files
│   │   └── Program.cs               # App bootstrap and DI configuration
│   └── TaskManagementApi.Tests/     # xUnit integration and unit tests
└── frontend/
    └── src/
        ├── components/              # React UI components
        └── services/                # Axios HTTP client
```

### Backend

The backend is an **ASP.NET Core 10** minimal-style API using the **repository pattern** over **Entity Framework Core 10** with a PostgreSQL database.

- `TasksController` exposes the REST endpoints (`GET /tasks`, `GET /tasks/{id}`, `POST /tasks`, `PATCH /tasks/{id}/toggle`).
- `TaskRepository` handles all database access and contains the idempotency logic for task creation.
- `TaskManagementContext` is the EF Core `DbContext`, mapping the `Task` domain model to the `Tasks` table.
- Dependency injection is configured in `Program.cs`, which also wires up OpenTelemetry, health checks, and CORS.

### Frontend

The frontend is a **React 19** single-page application built with **Vite**.

- **`NewTask`** — form for creating tasks; generates a client-side UUID v7 as the idempotency key before submitting.
- **`TaskList`** — renders tasks split into two columns: pending and completed.
- **`TaskItem`** — individual task row with a checkbox to toggle completion status.
- **`services/api.js`** — Axios instance preconfigured with `http://localhost:5000` as the base URL.

---

## Observability

The application ships with full **OpenTelemetry** instrumentation out of the box, visualised through the **.NET Aspire Dashboard** at http://localhost:18888.

### What is collected

| Signal | Source |
|---|---|
| Traces | ASP.NET Core request pipeline, Npgsql (SQL queries) |
| Metrics | ASP.NET Core (request duration, active requests), Npgsql connection pool |

### How it works

`Program.cs` configures the OTLP exporter, which pushes signals from the API container to the Aspire Dashboard container over gRPC:

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .UseOtlpExporter()
    .WithTracing(x => x.AddAspNetCoreInstrumentation().AddNpgsql())
    .WithMetrics(x => x.AddAspNetCoreInstrumentation().AddNpgsqlInstrumentation());
```

The `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable is set to `http://aspire-dashboard:18889` in `compose.yaml`, so no code change is needed between local and container environments.

### Health check

A database health check is available at `GET /health`. It verifies PostgreSQL connectivity and returns `200 OK` / `503 Service Unavailable` accordingly.

---

## Database Migrations

Migrations are managed with **Entity Framework Core** and applied automatically on every `docker compose up`.

### How it works

The backend `Dockerfile` compiles a self-contained **EF migrations bundle** (`efbundle`) during the image build:

```dockerfile
RUN dotnet-ef migrations bundle --self-contained -r linux-x64 -o efbundle
```

A dedicated `migration` service in `compose.yaml` then runs this bundle at startup:

```yaml
migration:
  command: ["./efbundle", "--connection", "<connection-string>"]
  depends_on:
    db:
      condition: service_healthy
```

The bundle applies any pending migrations and exits. Because `task-management-api` depends on `migration` completing successfully, the database schema is always in sync before the API starts accepting traffic.

To create a new migration locally:

```bash
cd backend/TaskManagementApi
dotnet ef migrations add <MigrationName>
```

The next `docker compose up --build` will pick it up automatically.

---

## Idempotent Task Creation

Submitting the same "create task" request more than once (e.g., due to a network retry) always produces exactly one task and always returns `201 Created`.

### How it works

**Client side** — `NewTask.jsx` generates a **UUID v7** immediately when the user submits the form and includes it as the task `id` in the request body:

```js
import { v7 as uuidv7 } from 'uuid';

const id = uuidv7();
await api.post('tasks', { id, title, content });
```

**Server side** — `AddTaskRequest` validates that the `Id` field is a non-empty GUID. The `TaskRepository.Add` method catches the PostgreSQL duplicate-key error (SQLSTATE `23505`) that occurs when the same `id` is inserted twice, logs a warning, and returns normally instead of throwing:

```csharp
catch (DbUpdateException ex)
    when (ex.InnerException is PostgresException { SqlState: "23505" })
{
    logger.LogWarning(ex, "Duplicate key exception while adding task.");
}
```

The controller always responds with `201 Created`, regardless of whether the row was actually inserted or already existed. This means clients can safely retry the request without producing duplicate tasks.

---

## Tests

### Running tests

```bash
cd backend
dotnet test
```

### Integration tests with TestContainers

`TaskManagementApi.Tests` uses **TestContainers** to spin up a real PostgreSQL 18.3 container for each test class, ensuring tests run against the same database engine used in production:

```csharp
private readonly PostgreSqlContainer _postgreSqlContainer =
    new PostgreSqlBuilder("postgres:18.3").Build();
```

`TaskManagementApplicationFactory` replaces the connection string at runtime with the one provided by TestContainers. EF Core migrations are applied inside `InitializeAsync` before any test runs, so every test starts with a clean, fully migrated schema.

The integration tests cover:

- Listing tasks (empty state, with data, ordering — completed tasks last)
- Fetching a task by ID (found and not found)
- Creating a task (happy path and idempotent duplicate)
- Toggling task completion (found and not found)

### Unit tests

`AddTaskRequestTests` validates the request DTO in isolation, covering the `[NotEmptyGuid]` custom attribute, title (required, max 255 chars), and content (optional, max 1000 chars) constraints.

---

## Next Steps

### Delete operation

There is currently no endpoint to delete tasks. Adding `DELETE /tasks/{id}` is the most natural next step — it would mirror the existing `PATCH` toggle and require a `Remove` method in `TaskRepository`.

### Pagination

`GET /tasks` returns all tasks in a single response. As the dataset grows this becomes a performance and usability problem. Implementing cursor- or offset-based pagination (e.g., `?page=1&pageSize=20`) on both the API and the frontend `TaskList` component would keep response sizes predictable.

### Database index

The `Tasks` table has no secondary indexes beyond the primary key. If the list endpoint adds filtering (e.g., by `Completed` or `CreatedAt`) a partial or composite index would prevent sequential scans. For example:

```sql
CREATE INDEX idx_tasks_completed ON "Tasks" ("Completed", "CreatedAt" DESC);
```

This would benefit both the current ordering logic (completed tasks last) and any future filtering features.
