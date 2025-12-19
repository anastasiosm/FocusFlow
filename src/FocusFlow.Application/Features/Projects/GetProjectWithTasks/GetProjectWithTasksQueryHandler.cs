using AutoMapper;
using FocusFlow.Application.Features.Projects.GetProjectById;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Projects.GetProjectWithTasks;

public class GetProjectWithTasksQueryHandler : IRequestHandler<GetProjectWithTasksQuery, ProjectDetailDto>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IMapper _mapper;

	public GetProjectWithTasksQueryHandler(IProjectRepository projectRepository, IMapper mapper)
	{
		_projectRepository = projectRepository;
		_mapper = mapper;
	}

	public async Task<ProjectDetailDto> Handle(GetProjectWithTasksQuery request, CancellationToken cancellationToken)
	{
		var project = await _projectRepository.GetByIdWithTasksAsync(request.Id, cancellationToken);

		if (project == null)
			throw new FocusFlowNotFoundException("Project", request.Id);

		return _mapper.Map<ProjectDetailDto>(project);
	}
}