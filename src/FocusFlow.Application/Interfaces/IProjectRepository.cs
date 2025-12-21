using FocusFlow.Domain.Entities;

namespace FocusFlow.Application.Interfaces;

/// <summary>
/// Project repository interface
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
	Task<Project?> GetByIdWithTasksAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<Project>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);
	Task<List<Project>> GetByOwnerIdWithTasksAsync(string ownerId, CancellationToken cancellationToken = default);
}
