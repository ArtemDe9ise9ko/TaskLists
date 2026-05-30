# Shared Task Lists API

A production-style .NET 8 Web API test assignment for managing task lists that
can be shared between users. The repository includes the backend, MongoDB
persistence, automated tests, Docker Compose setup, Swagger documentation, and
a framework-agnostic TypeScript API provider.

## Assignment Coverage

- Create, list, get, update, and delete task lists.
- Share task lists with users and remove share relations.
- Return only lists owned by or shared with the current user.
- Enforce owner-only deletion and share management.
- Allow owners and shared users to read, update, and view shares.
- Provide offset pagination ordered by newest task lists first.

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- MongoDB with `MongoDB.Driver`
- Docker Compose
- xUnit
- Swagger/OpenAPI
- TypeScript provider using native `fetch`

## Architecture

```text
src/
  TaskLists.Api/             HTTP transport, middleware, Swagger, composition root
  TaskLists.Application/     Use cases, policies, abstractions, application errors
  TaskLists.Domain/          Pragmatic domain models and invariants
  TaskLists.Contracts/       Request and response DTOs
  TaskLists.Infrastructure/  MongoDB documents, mappings, repositories, indexes
tests/
  TaskLists.Tests/           Domain, access-policy, and application-service tests
typescript-provider/         Framework-agnostic typed API client
```

The detailed design is documented in
[`docs/architecture-and-api-contract.md`](docs/architecture-and-api-contract.md).

## Architecture Decisions

- MediatR is intentionally not used because the assignment discourages it.
- Domain and Contracts do not depend on transport or infrastructure.
- Application depends on repository abstractions, not MongoDB or HTTP.
- MongoDB implementation details are isolated in Infrastructure.
- `X-User-Id` is a temporary current-user mechanism instead of authentication.
- API failures use consistent Problem Details responses.
- The TypeScript provider is framework-agnostic and contains no UI components.

## Implemented Features

- Layered .NET 8 solution with dependency injection.
- Pragmatic `TaskList` and `TaskListShare` domain models.
- Explicit application service and access-control policy.
- MongoDB documents, mappings, repositories, indexes, and share cleanup.
- REST controllers, Swagger documentation, and `GET /health`.
- Centralized Problem Details exception mapping.
- Unit tests for domain validation, permissions, and application behavior.
- Typed TypeScript provider with configurable requests and error mapping.

## API Endpoints

All API endpoints except `GET /health` require an `X-User-Id` header.

| Method   | Path                                   | Description                |
| -------- | -------------------------------------- | -------------------------- |
| `GET`    | `/health`                              | Check API health           |
| `POST`   | `/api/task-lists`                      | Create a task list         |
| `GET`    | `/api/task-lists?page=1&pageSize=20`   | List accessible task lists |
| `GET`    | `/api/task-lists/{id}`                 | Get a task list            |
| `PUT`    | `/api/task-lists/{id}`                 | Update a task list         |
| `DELETE` | `/api/task-lists/{id}`                 | Delete an owned task list  |
| `POST`   | `/api/task-lists/{id}/shares`          | Share an owned task list   |
| `GET`    | `/api/task-lists/{id}/shares`          | View task-list shares      |
| `DELETE` | `/api/task-lists/{id}/shares/{userId}` | Remove a share relation    |

## Current User

Authentication is intentionally outside the assignment scope. Send the current
user as a required request parameter:

```http
X-User-Id: user-1
```

Missing, empty, duplicated, or oversized values return `400 Bad Request`. In a
production system, the provider can be replaced by authenticated claims without
changing application logic.

## Error Handling

The API returns `application/problem+json` responses with a trace ID.

| Status                      | Meaning                                         |
| --------------------------- | ----------------------------------------------- |
| `400 Bad Request`           | Validation error or missing/invalid `X-User-Id` |
| `403 Forbidden`             | Current user lacks permission                   |
| `404 Not Found`             | Task list does not exist                        |
| `409 Conflict`              | Duplicate share relation or similar conflict    |
| `500 Internal Server Error` | Unexpected server error                         |

## Run Locally

Prerequisites:

- .NET 8 SDK or a newer SDK capable of targeting .NET 8.
- Docker with Docker Compose.
- Node.js and npm for the optional TypeScript provider build.

Start MongoDB:

```sh
docker compose up -d
```

Run the API:

```sh
dotnet run --project src/TaskLists.Api
```

Open Swagger UI at `http://localhost:5091/swagger` when using the default
development launch profile.

## Verification

Run backend restore, build, and tests:

```sh
dotnet restore TaskLists.sln
dotnet build TaskLists.sln --no-restore
dotnet test TaskLists.sln --no-build
```

Build and typecheck the TypeScript provider:

```sh
cd typescript-provider
npm install
npm run typecheck
npm run build
```

## Manual Scenario

Use Swagger UI or an HTTP client:

1. Start MongoDB and run the API.
2. Create a list as `user-1`.
3. Get the list as `user-1`.
4. Share the list with `user-2`.
5. Get the shared list as `user-2`.
6. Update the shared list as `user-2`.
7. Try to delete it as `user-2`; expect `403 Forbidden`.
8. Delete it as `user-1`; expect `204 No Content`.

## TypeScript Provider

The provider lives in [`typescript-provider/`](typescript-provider/). It offers
typed API methods, `X-User-Id` injection through `getUserId`, request
configuration, and Problem Details error mapping without UI dependencies.

## Stage Status

- Stage 1: Architecture and API Contract - Done
- Stage 2: Solution Skeleton - Done
- Stage 3: Domain and Application Contracts - Done
- Stage 4: Application Service Implementation - Done
- Stage 5: MongoDB Persistence Implementation - Done
- Stage 6: REST API Controllers and ProblemDetails Mapping - Done
- Stage 7: TypeScript API Provider - Done
- Stage 8: Final Polish, Full Verification and Submission Readiness - Done
