using AutoMapper;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.UnassignTask;

public class UnassignTaskCommandHandler : IRequestHandler<UnassignTaskCommand, TaskDto>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;

	public UnassignTaskCommandHandler(
		ITaskRepository taskRepository,
		IUnitOfWork unitOfWork,
		IMapper mapper)
	{
		_taskRepository = taskRepository;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
	}

	public async Task<TaskDto> Handle(UnassignTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
		if (task == null)
			throw new FocusFlowNotFoundException("Task", request.TaskId);

		task.Unassign();

		await _taskRepository.UpdateAsync(task, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return _mapper.Map<TaskDto>(task);
	}
}
