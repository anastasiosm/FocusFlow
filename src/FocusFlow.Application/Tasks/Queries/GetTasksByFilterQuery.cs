using FocusFlow.Application.DTO;
using MediatR;
using FocusFlow.Domain.Enums;

namespace FocusFlow.Application.Tasks.Queries;

/// <summary>
/// Query to get tasks filtered by status, priority, and overdue status
/// </summary>
public record GetTasksByFilterQuery(
	ProjectTaskStatus? Status = null,
	Priority? Priority = null,
	bool? IsOverdue = null) : IRequest<List<TaskDto>>;
