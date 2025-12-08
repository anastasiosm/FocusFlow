using AutoMapper;
using FocusFlow.Application.DTO;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Tasks.Commands;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;

	public UpdateTaskCommandHandler(
		ITaskRepository taskRepository,
		IUnitOfWork unitOfWork,
		IMapper mapper)
	{
		_taskRepository = taskRepository;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
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

		await _taskRepository.UpdateAsync(task, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return _mapper.Map<TaskDto>(task);
	}
}
