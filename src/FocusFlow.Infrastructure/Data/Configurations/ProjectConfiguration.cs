using FocusFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FocusFlow.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for Project
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
	public void Configure(EntityTypeBuilder<Project> builder)
	{
		builder.ToTable("Projects");

		builder.HasKey(p => p.Id);

		builder.Property(p => p.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(p => p.Description)
			.HasMaxLength(1000);

		builder.Property(p => p.OwnerId)
			.IsRequired()
			.HasMaxLength(450);

		builder.Property(p => p.CreatedAt)
			.IsRequired();

		builder.Property(p => p.UpdatedAt)
			.IsRequired();

		// One-to-many relationship with Tasks
		builder.HasMany(p => p.Tasks)
			.WithOne(t => t.Project)
			.HasForeignKey(t => t.ProjectId)
			.OnDelete(DeleteBehavior.Cascade);

		// Index for performance
		builder.HasIndex(p => p.OwnerId);
		builder.HasIndex(p => p.CreatedAt);
	}
}