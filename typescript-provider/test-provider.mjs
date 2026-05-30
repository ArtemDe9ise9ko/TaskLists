import { TaskListsApi, ApiError } from "./dist/index.js";

const api = new TaskListsApi({
  baseUrl: "http://localhost:5091",
  getUserId: () => "user-1",
});

try {
  console.log("Creating task list...");

  const created = await api.create({
    title: "Test list from TypeScript provider",
  });

  console.log("Created:", created);

  console.log("Getting page...");

  const page = await api.getPage({
    page: 1,
    pageSize: 20,
  });

  console.log("Page:", page);

  console.log("Sharing with user-2...");

  const share = await api.addShare(created.id, {
    userId: "user-2",
  });

  console.log("Share:", share);

  console.log("Getting shared list as user-2...");

  const user2Api = new TaskListsApi({
    baseUrl: "http://localhost:5091",
    getUserId: () => "user-2",
  });

  const sharedList = await user2Api.getById(created.id);

  console.log("Shared list for user-2:", sharedList);

  console.log("Trying to delete as user-2. Expected 403...");

  try {
    await user2Api.delete(created.id);
  } catch (error) {
    if (error instanceof ApiError) {
      console.log("Expected error:", {
        status: error.status,
        title: error.title,
        detail: error.detail,
        traceId: error.traceId,
      });
    } else {
      throw error;
    }
  }

  console.log("Deleting as owner user-1...");

  await api.delete(created.id);

  console.log("Deleted successfully.");
} catch (error) {
  if (error instanceof ApiError) {
    console.error("API error:", {
      status: error.status,
      title: error.title,
      detail: error.detail,
      traceId: error.traceId,
      problem: error.problem,
    });
  } else {
    console.error("Unexpected error:", error);
  }

  process.exitCode = 1;
}
