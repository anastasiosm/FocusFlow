using FocusFlow.Application.Interfaces;
using FocusFlow.Infrastructure.Data;

namespace FocusFlow.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for transaction coordination
/// </summary>
public class UnitOfWork : IUnitOfWork
{
	private readonly FocusFlowDbContext _context;

	public UnitOfWork(FocusFlowDbContext context)
	{
		_context = context;
	}

	public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await _context.SaveChangesAsync(cancellationToken);
	}
}