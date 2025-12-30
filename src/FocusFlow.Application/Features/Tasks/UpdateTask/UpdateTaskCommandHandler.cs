using AutoMapper;
using FocusFlow.Application.Common.Events;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.UpdateTask;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;
	private readonly IEventPublisher _eventPublisher;

	public UpdateTaskCommandHandler(
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

	public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
		if (task == null)
			throw new FocusFlowNotFoundException("Task", request.TaskId);

		task.Update(
			request.Title,
			request.Description,
			request.DueDate,
			request.Priority);

		// Handle assignment changes
		if (string.IsNullOrWhiteSpace(request.AssignedUserId))
		{
			task.Unassign();
		}
		else if (task.AssignedUserId != request.AssignedUserId)
		{
			task.Assign(request.AssignedUserId);
		}

		await _taskRepository.UpdateAsync(task, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// CRITICAL: Publish event AFTER successful save
		//    This notifies all connected clients about the change
		await _eventPublisher.PublishTaskUpdatedAsync(task.Id, task.ProjectId, cancellationToken);

		return _mapper.Map<TaskDto>(task);
	}
}
