using AutoMapper;
using FocusFlow.Application.DTO;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Tasks.Queries;

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
		// Single database query with all filters applied at SQL level
		var tasks = await _taskRepository.GetByOwnerWithFiltersAsync(
			request.OwnerId,
			request.Status,
			request.Priority,
			request.IsOverdue,
			cancellationToken);

		return _mapper.Map<List<TaskDto>>(tasks);
	}
}