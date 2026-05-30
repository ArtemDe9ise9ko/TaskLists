using Microsoft.AspNetCore.Mvc;
using TaskLists.Application.Exceptions;

namespace TaskLists.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (status, title) = exception switch
            {
                ValidationException => (
                    StatusCodes.Status400BadRequest,
                    "Request validation failed."),
                ForbiddenException => (
                    StatusCodes.Status403Forbidden,
                    "The request is forbidden."),
                NotFoundException => (
                    StatusCodes.Status404NotFound,
                    "The requested resource was not found."),
                ConflictException => (
                    StatusCodes.Status409Conflict,
                    "The request conflicts with the current resource state."),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.")
            };

            if (status == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "An unexpected error occurred.");
            }
            else
            {
                logger.LogInformation(
                    exception,
                    "Request failed with status code {StatusCode}.",
                    status);
            }

            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "The server could not complete the request."
                    : exception.Message,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
