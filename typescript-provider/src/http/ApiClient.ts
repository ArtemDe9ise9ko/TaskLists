import type { ApiProviderConfig } from "../config.js";
import { ApiError } from "./ApiError.js";
import type { HttpMethod } from "./HttpMethod.js";
import type { ProblemDetails } from "./ProblemDetails.js";

export class ApiClient {
  private readonly baseUrl: string;
  private readonly getUserId: ApiProviderConfig["getUserId"];
  private readonly defaultHeaders: Record<string, string>;
  private readonly fetchFn: typeof fetch;

  public constructor(config: ApiProviderConfig) {
    this.baseUrl = config.baseUrl.replace(/\/+$/, "");
    this.getUserId = config.getUserId;
    this.defaultHeaders = config.defaultHeaders ?? {};
    this.fetchFn = config.fetchFn ?? fetch;
  }

  public get<T>(path: string): Promise<T> {
    return this.request<T>("GET", path);
  }

  public post<TResponse, TRequest = unknown>(
    path: string,
    body: TRequest,
  ): Promise<TResponse> {
    return this.request<TResponse>("POST", path, body);
  }

  public put<TResponse, TRequest = unknown>(
    path: string,
    body: TRequest,
  ): Promise<TResponse> {
    return this.request<TResponse>("PUT", path, body);
  }

  public delete(path: string): Promise<void> {
    return this.request<void>("DELETE", path);
  }

  private async request<TResponse>(
    method: HttpMethod,
    path: string,
    body?: unknown,
  ): Promise<TResponse> {
    const userId = await this.getUserId();
    const headers: Record<string, string> = {
      ...this.defaultHeaders,
      Accept: "application/json",
      "X-User-Id": userId,
    };

    if (body !== undefined) {
      headers["Content-Type"] = "application/json";
    }

    const response = await this.fetchFn(`${this.baseUrl}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    });

    if (!response.ok) {
      throw await this.createApiError(response);
    }

    if (response.status === 204) {
      return undefined as TResponse;
    }

    return (await response.json()) as TResponse;
  }

  private async createApiError(response: Response): Promise<ApiError> {
    let problem: ProblemDetails | undefined;

    try {
      problem = (await response.json()) as ProblemDetails;
    } catch {
      problem = undefined;
    }

    return new ApiError(response.status, problem);
  }
}
