using FocusFlow.Application.Interfaces;
using FocusFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation for basic CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class Repository<T> : IRepository<T> where T : class
{
	protected readonly FocusFlowDbContext _context;

	protected Repository(FocusFlowDbContext context)
	{
		_context = context;
	}

	public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
	}

	public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _context.Set<T>().ToListAsync(cancellationToken);
	}

	public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
	{
		await _context.Set<T>().AddAsync(entity, cancellationToken);
	}

	public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
	{
		_context.Set<T>().Update(entity);
		return Task.CompletedTask;
	}

	public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
	{
		_context.Set<T>().Remove(entity);
		return Task.CompletedTask;
	}
}
