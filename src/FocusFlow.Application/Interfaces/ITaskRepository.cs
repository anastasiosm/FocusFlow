using FocusFlow.Domain.Entities;

namespace FocusFlow.Application.Interfaces;

/// <summary>
/// Task repository interface
/// </summary>
public interface ITaskRepository
{
	Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<ProjectTask>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<List<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
	Task<List<ProjectTask>> GetByAssignedUserIdAsync(string userId, CancellationToken cancellationToken = default);
	Task AddAsync(ProjectTask task, CancellationToken cancellationToken = default);
	Task UpdateAsync(ProjectTask task, CancellationToken cancellationToken = default);
	Task DeleteAsync(ProjectTask task, CancellationToken cancellationToken = default);
}
