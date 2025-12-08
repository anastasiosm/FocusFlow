using MediatR;

namespace FocusFlow.Application.Tasks.Commands;

/// <summary>
/// Command to delete a task
/// </summary>
public record DeleteTaskCommand(Guid TaskId) : IRequest<Unit>;
