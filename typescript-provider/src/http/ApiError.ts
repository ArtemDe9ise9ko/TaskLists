import type { ProblemDetails } from "./ProblemDetails.js";

export class ApiError extends Error {
  public readonly status: number;
  public readonly title?: string;
  public readonly detail?: string;
  public readonly traceId?: string;
  public readonly problem?: ProblemDetails;

  public constructor(status: number, problem?: ProblemDetails) {
    const title = problem?.title;
    const detail = problem?.detail;

    super(detail ?? title ?? `API request failed with status ${status}.`);

    this.name = "ApiError";
    this.status = status;
    this.title = title;
    this.detail = detail;
    this.traceId = problem?.traceId;
    this.problem = problem;
  }
}
