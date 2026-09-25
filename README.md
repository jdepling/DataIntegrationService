# DataIntegrationService

A .NET 8 proof-of-concept for reliable data integration using an **Outbox Pattern**, **RabbitMQ**, and background workers.

The flow is:

**API → SQL Outbox → Outbox Publisher → RabbitMQ → Worker**

## Configuration

Create a `.env` file in the solution root:

```env
SA_PASSWORD=YourStrongPassword123!
RABBITMQ_USERNAME=admin
RABBITMQ_PASSWORD=admin
```

These credentials are used by the Docker containers.

> **Note:** `.env` is ignored by Git. Do not commit real credentials.

## Run

Make sure Docker Desktop is running, then from the solution directory:

```powershell
docker compose up -d --build
```

Check that everything is running:

```powershell
docker compose ps
```

To also see the completed migration container:

```powershell
docker compose ps -a
```

The migration container should show `Exited (0)`.

## Test

Open Swagger:

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

The API should return **202 Accepted**.

## See the Worker Receive the Message

Run:

```powershell
docker compose logs -f worker
```

You should see something like:

```text
Worker is listening for messages...
Received: {
    "customerId": 12345,
    "name": "John Smith",
    "amount": 99.95,
    "status": "Created"
}
```

## Stop

```powershell
docker compose down
```

To completely reset the environment, including SQL Server and RabbitMQ data:

```powershell
docker compose down -v
```
