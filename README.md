# Data Integration Service

A .NET 8 proof-of-concept for reliable data integration demonstrating use of the **Outbox Pattern**, **RabbitMQ**, background workers, **Docker**, **Kubernetes**, unit tests, integration tests, performance tests, a CI/CD pipeline, a **MVC** application for controlled replay, Swagger, and an idempotent receiving system.

The flow is:

**System A → Integration API → SQL Outbox → Outbox Publisher → RabbitMQ → Integration Worker → System B → SQL**

The project demonstrates reliable, at-least-once message delivery while preventing duplicate records in System B.

## Architecture

```text
System A
    │
    │ POST
    ▼
Integration API
    │
    │ Save
    ▼
SQL Outbox
    │
    │ Poll
    ▼
Outbox Publisher
    │
    │ Publish
    ▼
RabbitMQ
    │
    │ Consume
    ▼
Integration Worker
    │
    │ HTTP POST
    ▼
System B
    │
    │ Save
    ▼
System B SQL Database
```

### Key concepts demonstrated

* **Outbox Pattern** — API saves the message to SQL before returning success.
* **Background publishing** — Outbox Publisher sends unpublished messages to RabbitMQ.
* **At-least-once delivery** — messages can be delivered more than once if a failure occurs.
* **Explicit RabbitMQ ACKs** — Worker acknowledges a message only after System B successfully processes it.
* **Idempotency** — System B uses `SourceId` to prevent duplicate records.
* **Database protection** — `SourceId` has a unique database index.
* **Docker Compose** — all services run together in containers.
* **EF Core migrations** — databases are initialized automatically when the environment starts.

## Configuration

Create a `.env` file in the solution root:

```env
SA_PASSWORD=YourStrongPassword123!
RABBITMQ_USERNAME=admin
RABBITMQ_PASSWORD=admin
```

These credentials are used by the Docker environment.

> **Note:** `.env` is ignored by Git. Do not commit real credentials.

## Run

Make sure Docker Desktop is running, then from the solution directory:

```powershell
docker compose up -d --build
```

Check the containers:

```powershell
docker compose ps
```

To also see the completed migration containers:

```powershell
docker compose ps -a
```

The migration containers should show:

```text
Exited (0)
```

## Test

Open the Integration API Swagger UI:

http://localhost:5252/swagger

POST a message using:

```json
{
  "sourceId": "TEST-DOCKER-001",
  "data": {
    "customerId": 12345,
    "name": "John Smith",
    "amount": 99.95,
    "status": "Created"
  }
}
```

The Integration API should return **202 Accepted**.

The message is then:

1. Saved to the SQL Outbox.
2. Published by the Outbox Publisher.
3. Delivered to RabbitMQ.
4. Consumed by the Integration Worker.
5. Sent to System B.
6. Saved to the System B database.
7. Acknowledged by the Worker.

### Verify the Worker

Follow the Worker logs:

```powershell
docker compose logs -f worker
```

You should see messages similar to:

```text
Worker is listening for messages...
Raw RabbitMQ message: {"SourceId":"TEST-DOCKER-001","Data":...}
SourceId: TEST-DOCKER-001
System B request: {"SourceId":"TEST-DOCKER-001",...}
Received HTTP response headers ... 201
Message TEST-DOCKER-001 successfully sent to System B.
```

Press **Ctrl+C** to stop following the logs.

### Verify System B

Check the System B database:

```powershell
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrongPassword123!" -C -Q "SELECT Id, SourceId, CustomerId, Name, Amount, Status, CreatedAt FROM SystemB.dbo.Messages ORDER BY Id"
```

The test message should appear in the results.

## Test Idempotency

Send the same `SourceId` more than once:

```json
{
  "sourceId": "TEST-DOCKER-001",
  "data": {
    "customerId": 12345,
    "name": "John Smith",
    "amount": 99.95,
    "status": "Created"
  }
}
```

Multiple messages can exist in the Integration API's Outbox because each request is recorded.

System B, however, uses `SourceId` as an idempotency key and will not create duplicate records.

Verify this by querying System B:

```powershell
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrongPassword123!" -C -Q "SELECT Id, SourceId, CreatedAt FROM SystemB.dbo.Messages WHERE SourceId = 'TEST-DOCKER-001'"
```

The query should return **one record**, even though the same message was submitted more than once.

The System B database also has a unique index on `SourceId` as an additional safeguard.

## Swagger

Integration API:

http://localhost:5252/swagger

System B:

http://localhost:5080/swagger

## RabbitMQ Management UI

RabbitMQ provides a management interface at:

http://localhost:15672

Login:

```text
Username: admin
Password: admin
```

The integration queue is:

```text
integration-queue
```
## Admin Screen
A MVC application that allows a user to do a controlled replay on failed messages with a helpful user interface. It also allows the user to view and correct the payload before resubmitting for a replay.
http://localhost:5253

## Stop

Stop the containers:

```powershell
docker compose down
```

To completely reset the environment, including SQL Server and RabbitMQ data:

```powershell
docker compose down -v
```

> **Warning:** `docker compose down -v` deletes the Docker volumes containing the SQL Server and RabbitMQ data.
