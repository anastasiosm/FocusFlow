using AutoMapper;
using FocusFlow.Domain.Entities;

namespace FocusFlow.Application.Features.Tasks.Common;

/// <summary>
/// AutoMapper profile for Task mappings
/// </summary>
public class TaskMappingProfile : Profile
{
	public TaskMappingProfile()
	{
		CreateMap<ProjectTask, TaskDto>();
	}
}