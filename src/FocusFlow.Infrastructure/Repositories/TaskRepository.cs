using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Enums;
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

	public async Task<List<ProjectTask>> GetByFilterAsync(ProjectTaskStatus? status, Priority? priority, bool? isOverdue, CancellationToken cancellationToken = default)
	{
		var query = _context.Tasks
			.Include(t => t.Project)
			.AsQueryable();

		if (status.HasValue)
		{
			query = query.Where(t => t.Status == status.Value);
		}

		if (priority.HasValue)
		{
			query = query.Where(t => t.Priority == priority.Value);
		}

		if (isOverdue.HasValue)
		{
			var currentDate = DateTime.UtcNow;
			if (isOverdue.Value)
			{
				query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value < currentDate && t.Status != ProjectTaskStatus.Done);
			}
			else
			{
				query = query.Where(t => !t.DueDate.HasValue || t.DueDate.Value >= currentDate || t.Status == ProjectTaskStatus.Done);
			}
		}

		return await query.OrderBy(t => t.DueDate).ToListAsync(cancellationToken);
	}

	public async Task<List<ProjectTask>> GetByOwnerWithFiltersAsync(string ownerId, ProjectTaskStatus? status, Priority? priority, bool? isOverdue, CancellationToken cancellationToken = default)
	{
		var query = _context.Tasks
			.Include(t => t.Project)
			// guard against null navigation property to avoid potential null dereference
			.Where(t => t.Project != null && t.Project.OwnerId == ownerId)
			.AsQueryable();

		if (status.HasValue)
		{
			query = query.Where(t => t.Status == status.Value);
		}

		if (priority.HasValue)
		{
			query = query.Where(t => t.Priority == priority.Value);
		}

		if (isOverdue.HasValue)
		{
			var currentDate = DateTime.UtcNow;
			if (isOverdue.Value)
			{
				query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value < currentDate && t.Status != ProjectTaskStatus.Done);
			}
			else
			{
				query = query.Where(t => !t.DueDate.HasValue || t.DueDate.Value >= currentDate || t.Status == ProjectTaskStatus.Done);
			}
		}

		return await query.OrderBy(t => t.DueDate).ToListAsync(cancellationToken);
	}
}