using MediatR;

namespace FocusFlow.Application.Features.Tasks.DeleteTask;

/// <summary>
/// Command to delete a task
/// </summary>
public record DeleteTaskCommand(Guid TaskId) : IRequest<Unit>;
