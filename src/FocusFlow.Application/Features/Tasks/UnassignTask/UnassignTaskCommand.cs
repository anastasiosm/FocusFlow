using FocusFlow.Application.Features.Tasks.Common;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.UnassignTask;

/// <summary>
/// Command to unassign a user from a task
/// </summary>
public record UnassignTaskCommand(Guid TaskId) : IRequest<TaskDto>;
