using FocusFlow.Domain.Entities;

namespace FocusFlow.Application.Interfaces;

/// <summary>
/// Task repository interface
/// </summary>
public interface ITaskRepository : IRepository<ProjectTask>
{
	Task<List<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
	Task<List<ProjectTask>> GetByAssignedUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
