using AutoMapper;
using FocusFlow.Application.DTO;
using FocusFlow.Application.Interfaces;
using MediatR;

namespace FocusFlow.Application.Tasks.Queries;

public class GetTasksByUserQueryHandler : IRequestHandler<GetTasksByUserQuery, List<TaskDto>>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IMapper _mapper;

	public GetTasksByUserQueryHandler(ITaskRepository taskRepository, IMapper mapper)
	{
		_taskRepository = taskRepository;
		_mapper = mapper;
	}

	public async Task<List<TaskDto>> Handle(GetTasksByUserQuery request, CancellationToken cancellationToken)
	{
		var tasks = await _taskRepository.GetByAssignedUserIdAsync(request.UserId, cancellationToken);
		return _mapper.Map<List<TaskDto>>(tasks);
	}
}
