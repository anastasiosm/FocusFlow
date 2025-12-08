using FocusFlow.Domain.Entities;

namespace FocusFlow.Application.Interfaces;

/// <summary>
/// Project repository interface
/// </summary>
public interface IProjectRepository
{
	Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<List<Project>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);
	Task AddAsync(Project project, CancellationToken cancellationToken = default);
	Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
	Task DeleteAsync(Project project, CancellationToken cancellationToken = default);
}
