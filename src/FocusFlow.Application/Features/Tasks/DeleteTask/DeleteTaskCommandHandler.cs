using FocusFlow.Application.Common.Events;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IEventPublisher _eventPublisher;

	public DeleteTaskCommandHandler(
		ITaskRepository taskRepository,
		IUnitOfWork unitOfWork,
		IEventPublisher eventPublisher)
	{
		_taskRepository = taskRepository;
		_unitOfWork = unitOfWork;
		_eventPublisher = eventPublisher;
	}

	public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
		if (task == null)
			throw new FocusFlowNotFoundException("Task", request.TaskId);

		// Store project ID before deletion
		var projectId = task.ProjectId;

		await _taskRepository.DeleteAsync(task, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		// Publish deletion event
		await _eventPublisher.PublishTaskDeletedAsync(request.TaskId, projectId, cancellationToken);

		return Unit.Value;
	}
}
