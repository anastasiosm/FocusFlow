using AutoMapper;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.GetTasksByProject;

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, List<TaskDto>>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IMapper _mapper;

	public GetTasksByProjectQueryHandler(ITaskRepository taskRepository, IMapper mapper)
	{
		_taskRepository = taskRepository;
		_mapper = mapper;
	}

	public async Task<List<TaskDto>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
	{
		var tasks = await _taskRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);
		return _mapper.Map<List<TaskDto>>(tasks);
	}
}
