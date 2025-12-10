using AutoMapper;
using FocusFlow.Application.DTO;
using FocusFlow.Domain.Entities;

namespace FocusFlow.Application.Mappings;

/// <summary>
/// AutoMapper profile for Project mappings
/// </summary>
public class ProjectMappingProfile : Profile
{
	public ProjectMappingProfile()
	{
		CreateMap<Project, ProjectDto>()
			.ConstructUsing(src => new ProjectDto(
				src.Id,
				src.Name,
				src.Description,
				src.OwnerId,
				src.CreatedAt,
				src.UpdatedAt,
				src.Tasks.Count));

		CreateMap<Project, ProjectDetailDto>()
			.ForMember(d => d.Tasks, opt => opt.MapFrom(s => s.Tasks));
	}
}
