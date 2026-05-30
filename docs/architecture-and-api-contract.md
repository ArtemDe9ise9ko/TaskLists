# Shared Task Lists Web API

## Architecture Decision and API Contract

## 1. Project Goal

Build a RESTful Web API using .NET 8 for managing shared task lists.

Users can create and manage task lists and share them with other users. Authentication and authorization infrastructure are outside the scope of this test assignment. Each request identifies the current user through an `X-User-Id` header.

This document captures the architecture decisions and API contract used by the implementation.

## 2. Key Decisions

| Area             | Decision                                     |
| ---------------- | -------------------------------------------- |
| Runtime          | .NET 8                                       |
| API style        | RESTful HTTP API                             |
| Persistence      | MongoDB                                      |
| Application flow | Explicit application services; no MediatR    |
| Current user     | `X-User-Id` request header                   |
| Pagination       | Offset pagination with `page` and `pageSize` |
| Sharing model    | Separate `taskListShares` collection         |
| Unit of Work     | Not required at this stage                   |

## 3. Proposed Solution Structure

```text
TaskLists.sln
src/
  TaskLists.Api/
    Controllers/
    Middleware/
    DependencyInjection/
    Program.cs

  TaskLists.Application/
    Abstractions/
      Persistence/
    TaskLists/
    Shares/
    DependencyInjection/

  TaskLists.Domain/
    TaskLists/
    Shares/

  TaskLists.Contracts/
    TaskLists/
    Shares/
    Common/

  TaskLists.Infrastructure/
    Persistence/
      MongoDb/
        Documents/
        Repositories/
        Indexes/
    DependencyInjection/
    Options/

tests/
  TaskLists.Tests/

typescript-provider/
```

Project references:

```text
TaskLists.Api            -> TaskLists.Application, TaskLists.Contracts, TaskLists.Infrastructure
TaskLists.Application    -> TaskLists.Domain, TaskLists.Contracts
TaskLists.Infrastructure -> TaskLists.Application, TaskLists.Domain
TaskLists.Contracts      -> no project dependencies
TaskLists.Domain         -> no project dependencies
```

`TaskLists.Api` is the composition root. Application logic depends on repository abstractions, not on MongoDB implementation details.

## 4. Layer Responsibilities

### `TaskLists.Api`

- Exposes HTTP endpoints.
- Reads route values, query parameters, JSON bodies, and `X-User-Id`.
- Maps contracts to application inputs and application results to responses.
- Produces consistent Problem Details error responses.
- Configures dependency injection, middleware, and OpenAPI.
- Contains no business logic or MongoDB queries.

### `TaskLists.Application`

- Implements use cases through explicit application services without MediatR.
- Enforces access control rules.
- Defines persistence interfaces implemented by infrastructure.
- Remains independent from ASP.NET Core and MongoDB.

Application service:

```text
ITaskListService
  CreateAsync
  GetPageAsync
  GetByIdAsync
  UpdateAsync
  DeleteAsync
  AddShareAsync
  GetSharesAsync
  RemoveShareAsync
```

### `TaskLists.Domain`

- Contains the `TaskList` and `TaskListShare` domain models.
- Defines core invariants, such as title validation and ownership.
- Remains independent from transport and persistence concerns.

The initial model should stay pragmatic. Use primitive string identifiers and enforce validation in the domain or application layer. Stronger value objects can be introduced later if domain complexity grows.

### `TaskLists.Contracts`

- Contains HTTP request and response DTOs.
- Contains shared response shapes such as pagination.
- Does not expose domain entities or MongoDB document classes.

### `TaskLists.Infrastructure`

- Implements application repository interfaces using `MongoDB.Driver`.
- Owns MongoDB documents, mappings, collection configuration, and indexes.
- Keeps MongoDB-specific details outside the application layer.

## 5. Domain Model

### `TaskList`

```text
TaskList
  Id: string
  Title: string
  OwnerUserId: string
  CreatedAtUtc: DateTime
  UpdatedAtUtc: DateTime
```

Rules:

- `Title` is required after trimming and must contain 1 to 255 characters.
- `OwnerUserId` is required and is assigned from the current user during creation.
- `CreatedAtUtc` is immutable.
- `UpdatedAtUtc` changes when the list is updated.

### `TaskListShare`

```text
TaskListShare
  Id: string
  TaskListId: string
  UserId: string
  CreatedAtUtc: DateTime
```

Rules:

- A relation is unique for `(TaskListId, UserId)`.
- The owner is not added as a shared user because ownership already grants access.
- Only the owner can add or remove share relations.

IDs are primitive strings and are exposed as opaque values. Do not introduce custom identifier or title value objects such as `TaskListId`, `TaskListTitle`, or `UserId` at this stage. Clients must not depend on the MongoDB storage representation.

## 6. API Endpoint Contract

Base path: `/api/task-lists`

Every task-list endpoint requires an `X-User-Id` header. The health endpoint
does not require it.

| Method   | Path                                   | Purpose                      | Allowed caller       | Success status   |
| -------- | -------------------------------------- | ---------------------------- | -------------------- | ---------------- |
| `POST`   | `/api/task-lists`                      | Create a task list           | Any identified user  | `201 Created`    |
| `GET`    | `/api/task-lists?page=1&pageSize=20`   | Get accessible task lists    | Any identified user  | `200 OK`         |
| `GET`    | `/api/task-lists/{id}`                 | Get one task list            | Owner or shared user | `200 OK`         |
| `PUT`    | `/api/task-lists/{id}`                 | Update task list title       | Owner or shared user | `200 OK`         |
| `DELETE` | `/api/task-lists/{id}`                 | Delete a task list           | Owner only           | `204 No Content` |
| `POST`   | `/api/task-lists/{id}/shares`          | Add a share relation         | Owner only           | `200 OK`         |
| `GET`    | `/api/task-lists/{id}/shares`          | Get users shared with a list | Owner or shared user | `200 OK`         |
| `DELETE` | `/api/task-lists/{id}/shares/{userId}` | Remove a share relation      | Owner only           | `204 No Content` |

Notes:

- The current user is always taken from `X-User-Id`, never from a request body.
- `POST /api/task-lists` returns a `Location` header for `/api/task-lists/{id}`.
- `PUT` replaces the currently mutable resource fields. At this stage, that is only `title`.
- Adding a duplicate share relation returns `409 Conflict`.
- Removing a missing share relation is idempotent and returns `204 No Content`.
- Route values must be URL encoded by clients.

## 7. Request and Response DTO Draft

### Requests

```csharp
public sealed record CreateTaskListRequest(string Title);

public sealed record UpdateTaskListRequest(string Title);

public sealed record AddTaskListShareRequest(string UserId);
```

### Responses

```csharp
public sealed record TaskListDetailsResponse(
    string Id,
    string Title,
    string OwnerUserId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record TaskListSummaryResponse(
    string Id,
    string Title,
    DateTime CreatedAtUtc);

public sealed record TaskListShareResponse(
    string UserId,
    DateTime CreatedAtUtc);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount);
```

Example create request:

```http
POST /api/task-lists
X-User-Id: user-123
Content-Type: application/json

{
  "title": "Release checklist"
}
```

Example create response:

```http
HTTP/1.1 201 Created
Location: /api/task-lists/1f8e714e-640f-45cc-8f97-9a18e54b7bf8

{
  "id": "1f8e714e-640f-45cc-8f97-9a18e54b7bf8",
  "title": "Release checklist",
  "ownerUserId": "user-123",
  "createdAtUtc": "2026-05-30T09:30:00Z",
  "updatedAtUtc": "2026-05-30T09:30:00Z"
}
```

Example paged response:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0
}
```

## 8. Current User Handling

Use the `X-User-Id` request header as a temporary replacement for real authentication:

```http
X-User-Id: user-123
```

The API layer validates the header and passes the current user ID explicitly to application services. A small API-layer abstraction can centralize access:

```csharp
public interface ICurrentUserProvider
{
    string? UserId { get; }
}
```

Validation:

- Header must exist.
- Header must contain exactly one non-empty value.
- A reasonable maximum length should be applied, such as 200 characters.

In a production system, the API implementation can be replaced with one that reads the user ID from authenticated claims. Application logic does not need to change.

## 9. Access Control Rules

The list endpoint returns only task lists where the current user is the owner or has a share relation. Any identified user can create a new list and becomes its owner.

| Operation    | Owner | Shared User |
| ------------ | ----- | ----------- |
| Get List     | Yes   | Yes         |
| Update List  | Yes   | Yes         |
| Delete List  | Yes   | No          |
| View Shares  | Yes   | Yes         |
| Add Share    | Yes   | No          |
| Remove Share | Yes   | No          |

An unrelated user cannot access any existing task list or its shares. Application services enforce these rules. The API layer is responsible only for transport concerns and error mapping.

## 10. Error Handling Strategy

Use consistent ASP.NET Core Problem Details responses with media type `application/problem+json`.

| Situation                                               | Status                      |
| ------------------------------------------------------- | --------------------------- |
| Request validation error                                | `400 Bad Request`           |
| Missing or invalid `X-User-Id`                          | `400 Bad Request`           |
| Current user lacks permission                           | `403 Forbidden`             |
| Task list or share relation not found                   | `404 Not Found`             |
| Duplicate share relation or attempt to share with owner | `409 Conflict`              |
| Unexpected error                                        | `500 Internal Server Error` |

Example:

```json
{
  "type": "https://tasklists.example/errors/validation",
  "title": "Request validation failed",
  "status": 400,
  "detail": "One or more request values are invalid.",
  "errors": {
    "title": ["Title must contain between 1 and 255 characters."]
  },
  "traceId": "00-..."
}
```

Implementation guidelines:

- Map known application errors centrally through middleware or an exception handler.
- Do not expose stack traces or database details.
- Include a trace ID for diagnostics.
- Use structured logging for relevant task-list and user IDs.

## 11. Pagination Strategy

Use simple offset pagination:

```http
GET /api/task-lists?page=1&pageSize=20
```

Rules:

- `page` is optional and defaults to `1`.
- `pageSize` is optional and defaults to `20`.
- Minimum `pageSize` is `1`.
- Maximum `pageSize` is `100`.
- Results are sorted by `createdAtUtc DESC`, then `id DESC` as a deterministic tie-breaker.
- The response includes `page`, `pageSize`, and `totalCount`.

Offset pagination was intentionally chosen because it is simpler, easier to review in a test assignment and fully satisfies current requirements. Cursor pagination can be introduced later if scalability requirements increase.

## 12. MongoDB Persistence Strategy

Use two collections:

```text
taskLists
taskListShares
```

### `taskLists`

Stores the main task-list aggregate data:

```json
{
  "_id": "1f8e714e-640f-45cc-8f97-9a18e54b7bf8",
  "title": "Release checklist",
  "ownerUserId": "user-123",
  "createdAtUtc": "2026-05-30T09:30:00Z",
  "updatedAtUtc": "2026-05-30T09:30:00Z"
}
```

### `taskListShares`

Stores explicit user access relations:

```json
{
  "_id": "generated-share-id",
  "taskListId": "1f8e714e-640f-45cc-8f97-9a18e54b7bf8",
  "userId": "user-456",
  "createdAtUtc": "2026-05-30T10:00:00Z"
}
```

This separation is chosen because:

- `taskLists` stores the main aggregate data.
- `taskListShares` stores user access relations explicitly.
- Share relations are easier to query and audit.
- Duplicate share relations can be prevented with a unique index.

For a small system, embedding shared user IDs inside each task-list document would also be possible. Two collections are preferred here because they make the sharing model and repository responsibilities clearer.

### Repository Approach

Application-layer repository interfaces should expose intention-revealing operations for:

- Creating, reading, updating, and deleting task lists.
- Listing task lists owned by or shared with a user.
- Creating, listing, checking, and deleting share relations.

UnitOfWork is intentionally not introduced at this stage. MongoDB repositories and document-level atomic operations are sufficient for current requirements. If a future use case requires coordinated multi-document transactions, introduce that capability explicitly where needed.

Deleting a task list also requires deleting its share relations. For this assignment, the infrastructure implementation should document and test the chosen cleanup behavior. If stronger consistency becomes necessary, an explicit MongoDB multi-document transaction can be added later.

## 13. Suggested Indexes

### `taskLists`

```text
{ ownerUserId: 1 }
{ createdAtUtc: -1 }
{ createdAtUtc: -1, _id: -1 }
```

### `taskListShares`

```text
{ taskListId: 1 }
{ userId: 1 }
{ taskListId: 1, userId: 1 } UNIQUE
```

These indexes support:

- Listing owned task lists in creation order.
- Resolving task lists shared with a user.
- Preventing duplicate share relations.
- Listing and deleting relations for a task list.

Index creation should be idempotent and handled by infrastructure startup initialization.

## 14. Testing Strategy

### Access Control Unit Tests

- Owner can perform every operation.
- Shared user can get and update a list and view its shares.
- Shared user cannot add or remove shares or delete a list.
- Unrelated user cannot access protected list operations.
- List results include only owned or shared task lists.

### Application Service Unit Tests

- Creating a list assigns the current user as owner.
- Title validation enforces the 1-to-255-character rule.
- Duplicate share creation returns a conflict result.
- Sharing with the owner returns a conflict result.
- Missing entities return not-found results.
- Pagination defaults and limits are applied correctly.

### MongoDB Repository and Integration Tests

- Verify CRUD operations.
- Verify unique share-relation index behavior.
- Verify queries for owned and shared lists.
- Verify sorting and offset pagination.
- Verify share cleanup behavior when deleting a list.

### API Tests

- Verify main endpoint routes and response DTOs.
- Verify success status codes and the create `Location` header.
- Verify `400 Bad Request` Problem Details for missing or invalid `X-User-Id`.
- Verify `403 Forbidden` Problem Details for forbidden access.
- Verify `400`, `404`, `409`, and `500` Problem Details responses where applicable.

## 15. TypeScript API Provider

The TypeScript provider is a typed API client only, with no UI components.

Suggested structure:

```text
typescript-provider/src/
  http/
    ApiClient.ts
    ApiError.ts
    ProblemDetails.ts
  task-lists/
    TaskListsApi.ts
    taskLists.types.ts
  index.ts
```

It should provide:

- Typed request and response models.
- Configurable `baseUrl`.
- Centralized `X-User-Id` injection.
- Centralized Problem Details error mapping.
- A `TaskListsApi` class or module for all task-list and sharing operations.
- No UI components.

The client can later be generated from or validated against the OpenAPI
document to reduce contract drift.

## 16. Implementation Stages

### Stage 1: Architecture and Contract

- Finalize architectural boundaries, domain model, permissions, routes, DTOs, error semantics, pagination, and persistence decisions.
- Maintain this document as the review artifact.

### Stage 2: Solution Skeleton

- Create the .NET 8 solution and projects.
- Add project references and dependency injection registration.
- Configure OpenAPI and centralized Problem Details handling.
- Add MongoDB configuration and local development setup.

### Stage 3: Domain and Application Contracts

- Implement pragmatic domain models.
- Add repository abstractions, service contracts, access policy, and unit tests.

### Stage 4: Application Service Implementation

- Implement application use cases against repository abstractions.
- Add focused application service unit tests.

### Stage 5: MongoDB Persistence Implementation

- Implement MongoDB documents, mappings, repositories, and index initialization.
- Add share cleanup when deleting a task list.

### Stage 6: REST API Controllers and ProblemDetails Mapping

- Implement controllers, current-user extraction, Problem Details mapping, and
  Swagger-visible endpoints.

### Stage 7: TypeScript API Provider

- Implement the typed, framework-agnostic API provider.
- Validate it with TypeScript typecheck and build commands.

### Stage 8: Final Polish and Verification

- Reconcile documentation with the implementation.
- Run backend and TypeScript verification commands.
