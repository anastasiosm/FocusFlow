using System.Reflection;
using FocusFlow.Domain.Entities;
using FocusFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FocusFlow.Infrastructure.Data;

/// <summary>
/// Main database context for FocusFlow application
/// Inherits from IdentityDbContext to include ASP.NET Core Identity tables
/// </summary>
public class FocusFlowDbContext : IdentityDbContext<ApplicationUser>
{
	public FocusFlowDbContext(DbContextOptions<FocusFlowDbContext> options) : base(options)
	{
	}

	// Application domain tables
	public DbSet<Project> Projects => Set<Project>();
	public DbSet<ProjectTask> Tasks => Set<ProjectTask>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// IMPORTANT: Call base to configure Identity tables
		base.OnModelCreating(modelBuilder);

		// Apply all entity configurations from current assembly
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

		// Set default schema for application domain tables
		modelBuilder.HasDefaultSchema(Schemas.Application);

		// Configure Identity tables to use default schema (public)
		ConfigureIdentitySchema(modelBuilder);
	}

	/// <summary>
	/// Move Identity tables to public schema (or keep them in public by default)
	/// </summary>
	private void ConfigureIdentitySchema(ModelBuilder modelBuilder)
	{
		// Identity tables use default schema (public in PostgreSQL)
		// This keeps them separate from application domain tables

		modelBuilder.Entity<ApplicationUser>(entity =>
		{
			entity.ToTable("asp_net_users", Schemas.Identity);
		});

		modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>(entity =>
		{
			entity.ToTable("asp_net_roles", Schemas.Identity);
		});

		modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>(entity =>
		{
			entity.ToTable("asp_net_user_roles", Schemas.Identity);
		});

		// Ignore unused tables for simple identity (Users + Roles only)
		modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>();
		modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>();
		modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>();
		modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>();
	}
}