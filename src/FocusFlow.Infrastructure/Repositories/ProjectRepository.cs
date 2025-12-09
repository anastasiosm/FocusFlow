using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Entities;
using FocusFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Infrastructure.Repositories;

/// <summary>
/// Project repository implementation
/// </summary>
public class ProjectRepository : Repository<Project>, IProjectRepository
{
	public ProjectRepository(FocusFlowDbContext context) : base(context)
	{
	}

	public async Task<Project?> GetByIdWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _context.Projects
			.Include(p => p.Tasks)
			.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
	}

	public async Task<List<Project>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
	{
		return await _context.Projects
			.Where(p => p.OwnerId == ownerId)
			.OrderBy(p => p.Name)
			.ToListAsync(cancellationToken);
	}
}