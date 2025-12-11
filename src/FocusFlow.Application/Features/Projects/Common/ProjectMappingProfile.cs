using AutoMapper;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Domain.Entities;

namespace FocusFlow.Application.Features.Projects.Common;

/// <summary>
/// AutoMapper profile for Project mappings
/// </summary>
public class ProjectMappingProfile : Profile
{
	public ProjectMappingProfile()
	{
		// Map Project entity to ProjectDto
		CreateMap<Project, ProjectDto>()
			.ConstructUsing(src => new ProjectDto(
				src.Id,
				src.Name,
				src.Description,
				src.OwnerId,
				src.CreatedAt,
				src.UpdatedAt,
				src.Tasks.Count));

		// Map Project entity to ProjectDetailDto (includes full task list)
		CreateMap<Project, ProjectDetailDto>();
	}
}
