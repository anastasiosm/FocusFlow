using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Domain.Enums;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.GetTasksByOwnerAndFilter;

/// <summary>
/// Query to get tasks by owner with filters - optimized single database query
/// </summary>
public record GetTasksByOwnerAndFilterQuery(
	string OwnerId,
	ProjectTaskStatus? Status = null,
	Priority? Priority = null,
	bool? IsOverdue = null) : IRequest<List<TaskDto>>;
