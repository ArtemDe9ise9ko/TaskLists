# Task Lists API

A .NET 8 Web API test assignment for managing task lists shared between users.

## Architecture

The solution is split into focused layers:

- `TaskLists.Api`: HTTP transport, OpenAPI, middleware, and composition root.
- `TaskLists.Application`: use-case orchestration and application abstractions.
- `TaskLists.Domain`: domain models and business rules.
- `TaskLists.Contracts`: request and response DTOs.
  `TaskLists.Infrastructure`: MongoDB configuration, document mappings, indexes, and repository implementations.
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
- Stage 3: Domain and Application Contracts - Done
- Stage 4: Application Service Implementation - Done
- Stage 5: MongoDB Persistence Implementation - Done

## Implemented In This Stage

- .NET 8 solution and project structure.
- Swagger/OpenAPI setup.
- `GET /health` endpoint.
- Centralized exception middleware skeleton.
- `X-User-Id` current user provider.
- Application and Infrastructure DI extension methods.
- MongoDB options binding.
- Docker Compose with MongoDB.
- Pragmatic domain models: `TaskList` and `TaskListShare`.
- Request and response DTO contracts.
- `PagedResponse<T>`.
- Repository abstraction interfaces.
- `ITaskListService` interface.
- `IClock` and `SystemClock`.
- `ITaskListAccessPolicy` and `TaskListAccessPolicy`.
- Application exceptions: `ValidationException`, `ForbiddenException`,
  `NotFoundException`, and `ConflictException`.
- Unit tests for access-control rules.
- Unit tests for domain validation.
- Application-layer `TaskListService` implemented against repository
  abstractions.
- Internal DTO mapping and application-level validation.
- Unit tests for task-list service business rules.
- Official MongoDB driver in the Infrastructure layer.
- MongoDB document models and explicit domain mappings.
- Infrastructure repository implementations.
- Idempotent MongoDB index creation during application startup.
- Share-relation cleanup when a task list is deleted.

REST CRUD endpoints and the TypeScript provider are intentionally deferred to
later stages.
