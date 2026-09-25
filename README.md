# DataIntegrationService

A .NET 8 proof-of-concept for building a resilient data integration service using an **Outbox Pattern**, **RabbitMQ**, and background workers.

The goal of this project is to explore reliable message delivery between systems while keeping the integration service resilient to failures.

## Architecture

```text
System A
   │
   │ HTTP POST
   ▼
Integration API
   │
   │ Save message
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
   │ HTTP
   ▼
System B
```

## Projects

* **Integration.Api** — Receives integration requests and stores them in the SQL outbox.
* **Integration.Data** — Shared Entity Framework Core models and database context.
* **Integration.OutboxPublisher** — Polls unpublished outbox messages and publishes them to RabbitMQ.
* **Integration.Worker** — Consumes RabbitMQ messages and will ultimately deliver them to System B.

## Technologies

* .NET 8
* C#
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* RabbitMQ
* Docker / Docker Compose
* Swagger / OpenAPI

## Key Concepts

### Outbox Pattern

The API stores the incoming message in SQL before returning `202 Accepted`.

This prevents a situation where the API successfully accepts a request but the message is lost because RabbitMQ is unavailable.

The Outbox Publisher periodically looks for messages that have not yet been published and sends them to RabbitMQ.

### At-Least-Once Delivery

The publisher marks an outbox message as published only after successfully publishing it to RabbitMQ.

If the publisher fails after publishing but before updating SQL, the message can be published again.

The planned solution is for System B to use an idempotency key such as `SourceId` so duplicate messages can be safely handled.

### Docker Compose

The entire application can be started with:

```bash
docker compose up -d --build
```

Docker Compose starts:

* SQL Server
* Database migration
* RabbitMQ
* Integration API
* Outbox Publisher
* Integration Worker

The migration service automatically applies Entity Framework Core migrations when starting from a new database.

## Running the Application

Start the application:

```bash
docker compose up -d --build
```

Check the services:

```bash
docker compose ps
```

The migration container is a one-time process and should show:

```text
Exited (0)
```

View logs:

```bash
docker compose logs worker
```

```bash
docker compose logs outbox-publisher
```

## Swagger

Once the application is running, open:

```text
http://localhost:5252/swagger
```

Example request:

```json
{
  "sourceId": "TEST-001",
  "data": {
    "customerId": 12345,
    "name": "John Smith",
    "amount": 99.95,
    "status": "Created"
  }
}
```

The API should return `202 Accepted`.

The message will then travel through:

```text
API → SQL Outbox → Outbox Publisher → RabbitMQ → Worker
```

## RabbitMQ Management UI

RabbitMQ's management interface is available at:

```text
http://localhost:15672
```

The local development credentials are:

```text
Username: admin
Password: admin
```

## Resetting the Environment

To stop the application:

```bash
docker compose down
```

To completely reset the development environment, including the SQL Server and RabbitMQ data:

```bash
docker compose down -v
```

The next:

```bash
docker compose up -d --build
```

will recreate the database and apply the EF Core migrations.

## Current Status

The current POC demonstrates:

* HTTP API receiving integration messages
* SQL Outbox persistence
* Background outbox publishing
* RabbitMQ messaging
* Background message consumption
* Dockerized deve
