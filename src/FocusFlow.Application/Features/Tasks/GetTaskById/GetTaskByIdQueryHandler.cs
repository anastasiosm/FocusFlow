using AutoMapper;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.GetTaskById;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IMapper _mapper;

	public GetTaskByIdQueryHandler(ITaskRepository taskRepository, IMapper mapper)
	{
		_taskRepository = taskRepository;
		_mapper = mapper;
	}

	public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
	{
		var task = await _taskRepository.GetByIdAsync(request.Id, cancellationToken);

		if (task == null)
		{
			throw new FocusFlowNotFoundException(nameof(FocusFlow.Domain.Entities.ProjectTask), request.Id);
		}

		return _mapper.Map<TaskDto>(task);
	}
}
