using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.DeleteTask;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteTaskCommandHandler(
		ITaskRepository taskRepository,
		IUnitOfWork unitOfWork)
	{
		_taskRepository = taskRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
		if (task == null)
			throw new FocusFlowNotFoundException("Task", request.TaskId);

		await _taskRepository.DeleteAsync(task, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Unit.Value;
	}
}
