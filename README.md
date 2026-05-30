# Task Lists API

A .NET 8 Web API test assignment for managing task lists shared between users.

## Architecture

The solution is split into focused layers:

- `TaskLists.Api`: HTTP transport, OpenAPI, middleware, and composition root.
- `TaskLists.Application`: use-case orchestration and application abstractions.
- `TaskLists.Domain`: domain models and business rules.
- `TaskLists.Contracts`: request and response DTOs.
- `TaskLists.Infrastructure`: MongoDB configuration and future persistence implementations.
- `TaskLists.Tests`: automated tests.

The detailed architecture and API contract are documented in
[`docs/architecture-and-api-contract.md`](docs/architecture-and-api-contract.md).

## Prerequisites

- .NET 8 SDK or a newer SDK capable of targeting .NET 8.
- Docker with Docker Compose.

## Run MongoDB

```sh
docker compose up -d
```

MongoDB is exposed at `mongodb://localhost:27017`.

## Build

```sh
dotnet restore TaskLists.sln
dotnet build TaskLists.sln
```

## Run The API

```sh
dotnet run --project src/TaskLists.Api
```

Use `GET /health` to verify that the API is running. Swagger UI is enabled in
the Development environment.

## Current Stage

- Stage 1: Architecture and API Contract - Done
- Stage 2: Solution Skeleton - Done

## Implemented In This Stage

- .NET 8 solution and project structure.
- Swagger/OpenAPI setup.
- `GET /health` endpoint.
- Centralized exception middleware skeleton.
- `X-User-Id` current user provider.
- Application and Infrastructure DI extension methods.
- MongoDB options binding.
- Docker Compose with MongoDB.

Business logic, CRUD endpoints, MongoDB repositories, and the TypeScript
provider are intentionally deferred to later stages.
