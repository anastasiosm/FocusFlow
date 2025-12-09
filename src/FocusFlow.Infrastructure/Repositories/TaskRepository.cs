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

	public async Task<List<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
	{
		return await _context.Tasks
			.Include(t => t.Project)
			.Where(t => t.ProjectId == projectId)
			.OrderBy(t => t.DueDate)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<ProjectTask>> GetByAssignedUserIdAsync(string userId, CancellationToken cancellationToken = default)
	{
		return await _context.Tasks
			.Include(t => t.Project)
			.Where(t => t.AssignedUserId == userId)
			.OrderBy(t => t.DueDate)
			.ToListAsync(cancellationToken);
	}
}