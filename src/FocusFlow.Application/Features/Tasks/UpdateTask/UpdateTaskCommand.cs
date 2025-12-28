using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Domain.Enums;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.UpdateTask;

/// <summary>
/// Command to update an existing task
/// </summary>
public record UpdateTaskCommand(
	Guid TaskId,
	string Title,
	string? Description,
	DateTime? DueDate,
	Priority Priority,
	string? AssignedUserId) : IRequest<TaskDto>;
