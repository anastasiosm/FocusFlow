using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Domain.Enums;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.GetTasksByFilter;

/// <summary>
/// Query to get tasks filtered by status, priority, and overdue status
/// </summary>
public record GetTasksByFilterQuery(
	ProjectTaskStatus? Status = null,
	Priority? Priority = null,
	bool? IsOverdue = null) : IRequest<List<TaskDto>>;
