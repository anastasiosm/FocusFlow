using AutoMapper;
using FocusFlow.Application.DTO;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Tasks.Queries;

public class GetTasksByFilterQueryHandler : IRequestHandler<GetTasksByFilterQuery, List<TaskDto>>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IMapper _mapper;

	public GetTasksByFilterQueryHandler(ITaskRepository taskRepository, IMapper mapper)
	{
		_taskRepository = taskRepository;
		_mapper = mapper;
	}

	public async Task<List<TaskDto>> Handle(GetTasksByFilterQuery request, CancellationToken cancellationToken)
	{
		var filteredTasks = await _taskRepository.GetByFilterAsync(
			request.Status,
			request.Priority,
			request.IsOverdue,
			cancellationToken);

		return _mapper.Map<List<TaskDto>>(filteredTasks);
	}
}
