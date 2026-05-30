# Task Lists API Provider

A small, typed TypeScript client for the Task Lists REST API. It uses native
`fetch`, injects the required `X-User-Id` header centrally, and maps API
Problem Details responses to `ApiError`.

This package contains no UI components and has no frontend framework
dependencies.

## Install And Build

```sh
npm install
npm run typecheck
npm run build
```

## Usage

```ts
import { TaskListsApi } from "./dist/index.js";

const taskListsApi = new TaskListsApi({
  baseUrl: "http://localhost:5091",
  getUserId: () => "owner-user",
});

const created = await taskListsApi.create({
  title: "Release checklist",
});

await taskListsApi.addShare(created.id, {
  userId: "shared-user",
});
```

`baseUrl` is normalized automatically, so a trailing slash is safe.
`getUserId` may return a string immediately or resolve one asynchronously.
Use `defaultHeaders` for additional headers and `fetchFn` for testing or custom
runtime integration.

## Error Handling

```ts
import { ApiError, TaskListsApi } from "./dist/index.js";

const taskListsApi = new TaskListsApi({
  baseUrl: "http://localhost:5091",
  getUserId: () => "shared-user",
});

try {
  await taskListsApi.delete("task-list-id");
} catch (error) {
  if (error instanceof ApiError) {
    console.error(error.status, error.detail, error.traceId);
  }
}
```
