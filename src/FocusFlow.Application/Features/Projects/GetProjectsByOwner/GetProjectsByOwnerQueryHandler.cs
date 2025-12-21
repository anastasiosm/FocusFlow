using AutoMapper;
using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Features.Projects.GetProjectsByOwner;

public class GetProjectsByOwnerQueryHandler : IRequestHandler<GetProjectsByOwnerQuery, List<ProjectDto>>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IMapper _mapper;

	public GetProjectsByOwnerQueryHandler(IProjectRepository projectRepository, IMapper mapper)
	{
		_projectRepository = projectRepository;
		_mapper = mapper;
	}

	public async Task<List<ProjectDto>> Handle(GetProjectsByOwnerQuery request, CancellationToken cancellationToken)
	{
		var projects = await _projectRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
		return _mapper.Map<List<ProjectDto>>(projects);
	}
}