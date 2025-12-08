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
		var allTasks = await _taskRepository.GetAllAsync(cancellationToken);

		var filteredTasks = allTasks.AsQueryable();

		if (request.Status.HasValue)
		{
			filteredTasks = filteredTasks.Where(t => t.Status == request.Status.Value);
		}

		if (request.Priority.HasValue)
		{
			filteredTasks = filteredTasks.Where(t => t.Priority == request.Priority.Value);
		}

		if (request.IsOverdue.HasValue)
		{
			if (request.IsOverdue.Value)
			{
				filteredTasks = filteredTasks.Where(t => t.IsOverdue());
			}
			else
			{
				filteredTasks = filteredTasks.Where(t => !t.IsOverdue());
			}
		}

		return _mapper.Map<List<TaskDto>>(filteredTasks.ToList());
	}
}
