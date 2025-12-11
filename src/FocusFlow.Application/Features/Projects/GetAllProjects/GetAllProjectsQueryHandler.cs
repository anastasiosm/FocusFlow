using AutoMapper;
using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Features.Projects.GetAllProjects;

public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, List<ProjectDto>>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IMapper _mapper;

	public GetAllProjectsQueryHandler(IProjectRepository projectRepository, IMapper mapper)
	{
		_projectRepository = projectRepository;
		_mapper = mapper;
	}

	public async Task<List<ProjectDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
	{
		var projects = await _projectRepository.GetAllAsync(cancellationToken);
		return _mapper.Map<List<ProjectDto>>(projects);
	}
}
