using FocusFlow.Application.Features.Tasks.Common;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.AssignTask;

/// <summary>
/// Command to assign a user to a task
/// </summary>
public record AssignTaskCommand(Guid TaskId, string UserId) : IRequest<TaskDto>;
