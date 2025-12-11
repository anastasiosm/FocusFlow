using AutoMapper;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.GetTasksByOwnerAndFilter;

public class GetTasksByOwnerAndFilterQueryHandler : IRequestHandler<GetTasksByOwnerAndFilterQuery, List<TaskDto>>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IMapper _mapper;

	public GetTasksByOwnerAndFilterQueryHandler(ITaskRepository taskRepository, IMapper mapper)
	{
		_taskRepository = taskRepository;
		_mapper = mapper;
	}

	public async Task<List<TaskDto>> Handle(GetTasksByOwnerAndFilterQuery request, CancellationToken cancellationToken)
	{
		var tasks = await _taskRepository.GetByOwnerWithFiltersAsync(
			request.OwnerId,
			request.Status,
			request.Priority,
			request.IsOverdue,
			cancellationToken);

		return _mapper.Map<List<TaskDto>>(tasks);
	}
}
