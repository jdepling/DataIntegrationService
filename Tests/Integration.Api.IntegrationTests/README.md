# API Integration Tests

## Setup

1. Ensure Docker Desktop and the .NET 8 SDK are installed.
2. Copy `.env.example` to `.env` in `docker/test-environments/api-integration/`.
3. Populate the required environment variables in `.env`. Ensure `SA_PASSWORD` is set. Do not commit `.env`.
4. Verify `Tests/Integration.Api.IntegrationTests/appsettings.json` has the correct API URL, SQL Server connection settings, and delivery timeout.

## Run the Tests

From the repository root, start the Docker environment:

```powershell
docker compose -f docker/test-environments/api-integration/compose.yml --env-file docker/test-environments/api-integration/.env up -d
```

Run the integration tests:

```powershell
dotnet test Tests/Integration.Api.IntegrationTests/Integration.Api.IntegrationTests.csproj
```

When finished, stop the environment:

```powershell
docker compose -f docker/test-environments/api-integration/compose.yml --env-file docker/test-environments/api-integration/.env down
```

To also remove the environment's persisted data:

```powershell
docker compose -f docker/test-environments/api-integration/compose.yml --env-file docker/test-environments/api-integration/.env down -v
```
