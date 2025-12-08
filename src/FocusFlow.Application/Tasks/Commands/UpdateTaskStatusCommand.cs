using FocusFlow.Application.DTO;
using MediatR;

namespace FocusFlow.Application.Tasks.Commands;

/// <summary>
/// Command to update task status
/// </summary>
public record UpdateTaskStatusCommand(Guid TaskId, Domain.Enums.TaskStatus Status) : IRequest<TaskDto>;
