import type { ApiProviderConfig } from "../config.js";
import { ApiClient } from "../http/ApiClient.js";
import type {
  AddTaskListShareRequest,
  CreateTaskListRequest,
  GetTaskListsPageRequest,
  PagedResponse,
  TaskListDetailsResponse,
  TaskListShareResponse,
  TaskListSummaryResponse,
  UpdateTaskListRequest,
} from "./taskLists.types.js";

export class TaskListsApi {
  private readonly apiClient: ApiClient;

  public constructor(config: ApiProviderConfig) {
    this.apiClient = new ApiClient(config);
  }

  public create(request: CreateTaskListRequest): Promise<TaskListDetailsResponse> {
    return this.apiClient.post<TaskListDetailsResponse, CreateTaskListRequest>(
      "/api/task-lists",
      request,
    );
  }

  public getPage(
    request: GetTaskListsPageRequest = {},
  ): Promise<PagedResponse<TaskListSummaryResponse>> {
    const query = new URLSearchParams();

    if (request.page !== undefined) {
      query.set("page", request.page.toString());
    }

    if (request.pageSize !== undefined) {
      query.set("pageSize", request.pageSize.toString());
    }

    const queryString = query.toString();
    const path = `/api/task-lists${queryString ? `?${queryString}` : ""}`;

    return this.apiClient.get<PagedResponse<TaskListSummaryResponse>>(path);
  }

  public getById(id: string): Promise<TaskListDetailsResponse> {
    return this.apiClient.get<TaskListDetailsResponse>(
      `/api/task-lists/${encodeURIComponent(id)}`,
    );
  }

  public update(
    id: string,
    request: UpdateTaskListRequest,
  ): Promise<TaskListDetailsResponse> {
    return this.apiClient.put<TaskListDetailsResponse, UpdateTaskListRequest>(
      `/api/task-lists/${encodeURIComponent(id)}`,
      request,
    );
  }

  public delete(id: string): Promise<void> {
    return this.apiClient.delete(`/api/task-lists/${encodeURIComponent(id)}`);
  }

  public addShare(
    taskListId: string,
    request: AddTaskListShareRequest,
  ): Promise<TaskListShareResponse> {
    return this.apiClient.post<TaskListShareResponse, AddTaskListShareRequest>(
      `/api/task-lists/${encodeURIComponent(taskListId)}/shares`,
      request,
    );
  }

  public getShares(taskListId: string): Promise<TaskListShareResponse[]> {
    return this.apiClient.get<TaskListShareResponse[]>(
      `/api/task-lists/${encodeURIComponent(taskListId)}/shares`,
    );
  }

  public removeShare(taskListId: string, userId: string): Promise<void> {
    return this.apiClient.delete(
      `/api/task-lists/${encodeURIComponent(taskListId)}/shares/${encodeURIComponent(userId)}`,
    );
  }
}
