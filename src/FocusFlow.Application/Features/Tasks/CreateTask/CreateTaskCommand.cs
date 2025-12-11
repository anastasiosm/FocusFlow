using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Enums;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.CreateTask;

/// <summary>
/// Command to create a new task
/// </summary>
public record CreateTaskCommand(
	Guid ProjectId,
	string Title,
	string? Description,
	DateTime? DueDate,
	Priority Priority,
	string? AssignedUserId) : IRequest<TaskDto>;
