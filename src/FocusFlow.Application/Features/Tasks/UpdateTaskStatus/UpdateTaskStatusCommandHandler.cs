using AutoMapper;
using FocusFlow.Application.Common.Events;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.UpdateTaskStatus;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, TaskDto>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;
	private readonly IEventPublisher _eventPublisher;

	public UpdateTaskStatusCommandHandler(
		ITaskRepository taskRepository,
		IUnitOfWork unitOfWork,
		IMapper mapper,
		IEventPublisher eventPublisher)
	{
		_taskRepository = taskRepository;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
		_eventPublisher = eventPublisher;
	}

	public async Task<TaskDto> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
	{
		var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
		if (task == null)
			throw new FocusFlowNotFoundException("Task", request.TaskId);

		task.SetStatus(request.Status);

		await _taskRepository.UpdateAsync(task, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// Publish status change event
		await _eventPublisher.PublishTaskStatusChangedAsync(task.Id, task.ProjectId, task.Status, cancellationToken);

		return _mapper.Map<TaskDto>(task);
	}
}