export interface CreateTaskListRequest {
  title: string;
}

export interface UpdateTaskListRequest {
  title: string;
}

export interface AddTaskListShareRequest {
  userId: string;
}

export interface TaskListDetailsResponse {
  id: string;
  title: string;
  ownerUserId: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface TaskListSummaryResponse {
  id: string;
  title: string;
  createdAtUtc: string;
}

export interface TaskListShareResponse {
  userId: string;
  createdAtUtc: string;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface GetTaskListsPageRequest {
  page?: number;
  pageSize?: number;
}
