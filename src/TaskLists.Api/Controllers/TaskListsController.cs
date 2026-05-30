using Microsoft.AspNetCore.Mvc;
using TaskLists.Api.CurrentUser;
using TaskLists.Application.Abstractions.CurrentUser;
using TaskLists.Application.Exceptions;
using TaskLists.Application.TaskLists;
using TaskLists.Contracts.Common;
using TaskLists.Contracts.Shares;
using TaskLists.Contracts.TaskLists;

namespace TaskLists.Api.Controllers;

[ApiController]
[Route("api/task-lists")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public sealed class TaskListsController(
    ITaskListService taskListService,
    ICurrentUserProvider currentUserProvider) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TaskListDetailsResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TaskListDetailsResponse>> CreateAsync(
        [FromBody] CreateTaskListRequest? request,
        CancellationToken cancellationToken)
    {
        var response = await taskListService.CreateAsync(
            RequireBody(request),
            currentUserProvider.GetRequiredUserId(),
            cancellationToken);

        return CreatedAtRoute(
            "GetTaskListById",
            new { id = response.Id },
            response);
    }

    [HttpGet]
    [ProducesResponseType<PagedResponse<TaskListSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<TaskListSummaryResponse>>> GetPageAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await taskListService.GetPageAsync(
            currentUserProvider.GetRequiredUserId(),
            page,
            pageSize,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id}", Name = "GetTaskListById")]
    [ProducesResponseType<TaskListDetailsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskListDetailsResponse>> GetByIdAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var response = await taskListService.GetByIdAsync(
            id,
            currentUserProvider.GetRequiredUserId(),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<TaskListDetailsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskListDetailsResponse>> UpdateAsync(
        string id,
        [FromBody] UpdateTaskListRequest? request,
        CancellationToken cancellationToken)
    {
        var response = await taskListService.UpdateAsync(
            id,
            RequireBody(request),
            currentUserProvider.GetRequiredUserId(),
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(
        string id,
        CancellationToken cancellationToken)
    {
        await taskListService.DeleteAsync(
            id,
            currentUserProvider.GetRequiredUserId(),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id}/shares")]
    [ProducesResponseType<TaskListShareResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskListShareResponse>> AddShareAsync(
        string id,
        [FromBody] AddTaskListShareRequest? request,
        CancellationToken cancellationToken)
    {
        var response = await taskListService.AddShareAsync(
            id,
            RequireBody(request),
            currentUserProvider.GetRequiredUserId(),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id}/shares")]
    [ProducesResponseType<IReadOnlyList<TaskListShareResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskListShareResponse>>> GetSharesAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var response = await taskListService.GetSharesAsync(
            id,
            currentUserProvider.GetRequiredUserId(),
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id}/shares/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveShareAsync(
        string id,
        string userId,
        CancellationToken cancellationToken)
    {
        await taskListService.RemoveShareAsync(
            id,
            userId,
            currentUserProvider.GetRequiredUserId(),
            cancellationToken);

        return NoContent();
    }

    private static T RequireBody<T>(T? request) where T : class
    {
        return request
            ?? throw new ValidationException("A request body is required.");
    }
}
