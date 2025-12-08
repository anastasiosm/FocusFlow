using AutoMapper;
using FocusFlow.Application.DTO;
using FocusFlow.Domain.Entities;

namespace FocusFlow.Application.Mappings;

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