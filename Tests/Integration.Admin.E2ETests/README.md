# Admin End-to-End Tests

## Setup

1. Ensure Docker Desktop and the .NET 8 SDK are installed.
2. Copy `.env.example` to `.env` in `docker/test-environments/admin-e2e/`.
3. Populate the required environment variables in `.env`. Do not commit `.env` or real credentials.

## Run the Tests

From the repository root, run:

```powershell
dotnet test Tests/Integration.Admin.E2ETests/Integration.Admin.E2ETests.csproj
```

Testcontainers starts and stops the required Docker environment automatically. Ensure Docker Desktop is running before executing the tests.
