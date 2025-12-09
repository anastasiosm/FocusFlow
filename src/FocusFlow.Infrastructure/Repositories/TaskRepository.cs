using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Entities;
using FocusFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Infrastructure.Repositories;

/// <summary>
/// Task repository implementation
/// </summary>
public class TaskRepository : Repository<ProjectTask>, ITaskRepository
{
	public TaskRepository(FocusFlowDbContext context) : base(context)
	{
	}

	public override async Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(t => t.Project)
			.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
	}

	public override async Task<List<ProjectTask>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(t => t.Project)
			.OrderBy(t => t.DueDate)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(t => t.ProjectId == projectId)
			.OrderBy(t => t.DueDate)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ProjectTask>> GetByAssignedUserIdAsync(string userId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(t => t.Project)
			.Where(t => t.AssignedUserId == userId)
			.OrderBy(t => t.DueDate)
			.ToListAsync(cancellationToken);
	}
}