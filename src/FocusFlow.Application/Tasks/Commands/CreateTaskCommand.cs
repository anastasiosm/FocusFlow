using FocusFlow.Application.DTO;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Enums;
using MediatR;

namespace FocusFlow.Application.Tasks.Commands;

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
