using FocusFlow.Application.DTO;
using MediatR;

namespace FocusFlow.Application.Tasks.Commands;

/// <summary>
/// Command to unassign a user from a task
/// </summary>
public record UnassignTaskCommand(Guid TaskId) : IRequest<TaskDto>;
