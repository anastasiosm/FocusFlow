using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;

namespace FocusFlow.Application.Interfaces;

public interface ITaskRepository : IRepository<ProjectTask>
{
	Task<List<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
	Task<List<ProjectTask>> GetByAssignedUserIdAsync(string userId, CancellationToken cancellationToken = default);
	Task<List<ProjectTask>> GetByFilterAsync(ProjectTaskStatus? status, Priority? priority, bool? isOverdue, CancellationToken cancellationToken = default);
	
	/// <summary>
	/// Get tasks by project owner with optional filters - single optimized query
	/// </summary>
	Task<List<ProjectTask>> GetByOwnerWithFiltersAsync(string ownerId, ProjectTaskStatus? status, Priority? priority, bool? isOverdue, CancellationToken cancellationToken = default);
}
