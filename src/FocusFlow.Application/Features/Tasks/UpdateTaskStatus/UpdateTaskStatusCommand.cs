using FocusFlow.Application.Features.Tasks.Common;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.UpdateTaskStatus;

/// <summary>
/// Command to update task status
/// </summary>
public record UpdateTaskStatusCommand(Guid TaskId, Domain.Enums.ProjectTaskStatus Status) : IRequest<TaskDto>;
