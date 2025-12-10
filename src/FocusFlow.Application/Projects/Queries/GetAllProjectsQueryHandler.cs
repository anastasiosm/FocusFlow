using AutoMapper;
using FocusFlow.Application.DTO;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Projects.Queries;

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
