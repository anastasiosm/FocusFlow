using FocusFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FocusFlow.Infrastructure.Data;

/// <summary>
/// Main database context for FocusFlow application
/// </summary>
public class FocusFlowDbContext : DbContext
{
	public FocusFlowDbContext(DbContextOptions<FocusFlowDbContext> options) : base(options)
	{
	}

	public DbSet<Project> Projects => Set<Project>();
	public DbSet<ProjectTask> Tasks => Set<ProjectTask>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Apply all configurations from the current assembly
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		// TODO: check if needed
		// modelBuilder.HasDefaultSchema(Schemas.Application);
	}
}