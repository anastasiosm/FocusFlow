using FocusFlow.Application.DTO;
using MediatR;

namespace FocusFlow.Application.Tasks.Commands;

/// <summary>
/// Command to assign a user to a task
/// </summary>
public record AssignTaskCommand(Guid TaskId, string UserId) : IRequest<TaskDto>;
