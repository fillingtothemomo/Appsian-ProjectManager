
# Mini Project Manager – Backend

ASP.NET Core 8 Web API powered by Entity Framework Core (SQLite) that backs the mini project manager assignment.

## Features

- JWT authentication with registration and login flows.
- Multi-tenant project management (each user sees only their projects).
- Task management with due dates, estimated hours, completion toggles, and soft validation.
- Smart Scheduler endpoint that returns a dependency-aware execution order and timeline.
- DTO-based input validation, service-layer business logic, and clean separation of concerns.

## Local Development

1. Install the **.NET 8 SDK**.
2. From the repository root run:
	```powershell
	cd backend
	dotnet run
	```
3. The API listens on `https://localhost:5001` (and `http://localhost:5000`).

The first launch creates `app.db` in the backend folder. If you upgrade the schema (e.g., new columns), delete the file to regenerate a fresh database until you adopt migrations.

## Configuration

Settings can be overridden via environment variables or `appsettings.*.json`.

| Setting | Description |
|---------|-------------|
| `Jwt__Key` | Symmetric signing key for JWTs (replace in production). |
| `Jwt__Issuer` | JWT issuer value. |
| `ConnectionStrings__DefaultConnection` | SQLite connection string. |
| `FrontendOrigin` | Allowed origin for CORS in development. |

## API Overview

```
POST   /api/v1/auth/register
POST   /api/v1/auth/login

GET    /api/v1/projects
POST   /api/v1/projects
GET    /api/v1/projects/{id}
DELETE /api/v1/projects/{id}

POST   /api/v1/projects/{projectId}/tasks
PUT    /api/v1/tasks/{taskId}
PATCH  /api/v1/tasks/{taskId}/toggle
DELETE /api/v1/tasks/{taskId}

POST   /api/v1/projects/{projectId}/schedule
```

All non-auth routes require a `Bearer` token returned from the login/register endpoints.

## Testing

The solution relies on integration testing via HTTP tools (e.g., Thunder Client, Postman). Add automated tests if you extend the API.
