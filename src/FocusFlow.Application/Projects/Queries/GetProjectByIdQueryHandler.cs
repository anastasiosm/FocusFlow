using AutoMapper;
using FocusFlow.Application.DTO;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Projects.Queries;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDetailDto>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IMapper _mapper;

	public GetProjectByIdQueryHandler(IProjectRepository projectRepository, IMapper mapper)
	{
		_projectRepository = projectRepository;
		_mapper = mapper;
	}

	public async Task<ProjectDetailDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
	{
		var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);

		if (project == null)
			throw new FocusFlowNotFoundException("Project", request.Id);

		return _mapper.Map<ProjectDetailDto>(project);
	}
}
