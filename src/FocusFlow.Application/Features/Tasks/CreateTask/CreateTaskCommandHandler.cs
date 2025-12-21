using AutoMapper;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Tasks.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
	private readonly ITaskRepository _taskRepository;
	private readonly IProjectRepository _projectRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;

	public CreateTaskCommandHandler(
		ITaskRepository taskRepository,
		IProjectRepository projectRepository,
		IUnitOfWork unitOfWork,
		IMapper mapper)
	{
		_taskRepository = taskRepository;
		_projectRepository = projectRepository;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
	}

	public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
	{
		var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
		if (project == null)
			throw new FocusFlowNotFoundException("Project", request.ProjectId);

		var task = new ProjectTask(
			request.Title,
			request.Description,
			request.ProjectId,
			request.DueDate,
			request.Priority,
			request.AssignedUserId);

		await _taskRepository.AddAsync(task, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return _mapper.Map<TaskDto>(task);
	}
}
