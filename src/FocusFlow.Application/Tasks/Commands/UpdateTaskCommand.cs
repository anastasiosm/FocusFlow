using FocusFlow.Application.DTO;
using FocusFlow.Domain.Enums;
using MediatR;

namespace FocusFlow.Application.Tasks.Commands;

/// <summary>
/// Command to update an existing task
/// </summary>
public record UpdateTaskCommand(
	Guid TaskId,
	string Title,
	string? Description,
	DateTime? DueDate,
	Priority Priority) : IRequest<TaskDto>;
