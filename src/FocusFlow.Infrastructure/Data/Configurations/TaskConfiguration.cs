using FocusFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FocusFlow.Infrastructure.Data.Configurations;

/// <summary>
/// Entity configuration for ProjectTask
/// </summary>
public class TaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
	public void Configure(EntityTypeBuilder<ProjectTask> builder)
	{
		builder.ToTable("Tasks");

		builder.HasKey(t => t.Id);

		builder.Property(t => t.Title)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(t => t.Description)
			.HasMaxLength(2000);

		builder.Property(t => t.Status)
			.IsRequired()
			.HasConversion<string>()
			.HasMaxLength(50);

		builder.Property(t => t.Priority)
			.IsRequired()
			.HasConversion<string>()
			.HasMaxLength(50);

		builder.Property(t => t.ProjectId)
			.IsRequired();

		builder.Property(t => t.CreatedAt)
			.IsRequired();

		builder.Property(t => t.UpdatedAt)
			.IsRequired();

		builder.Property(t => t.DueDate);

		builder.Property(t => t.CompletedAt);

		// Relationship configured in ProjectConfiguration
		builder.HasOne(t => t.Project)
			.WithMany(p => p.Tasks)
			.HasForeignKey(t => t.ProjectId);

		// Indexes for performance
		builder.HasIndex(t => t.ProjectId);
		builder.HasIndex(t => t.Status);
		builder.HasIndex(t => t.DueDate);
		builder.HasIndex(t => t.CreatedAt);
	}
}