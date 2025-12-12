using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Projects.DeleteProject;

/// <summary>
/// Handler for DeleteProjectCommand
/// </summary>
public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteProjectCommandHandler(
		IProjectRepository projectRepository,
		IUnitOfWork unitOfWork)
	{
		_projectRepository = projectRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
	{
		var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);

		if (project == null)
			throw new FocusFlowNotFoundException("Project", request.Id);

		if (project.OwnerId != request.UserId)
			throw new FocusFlowUnauthorizedException("You do not have permission to delete this project");

		await _projectRepository.DeleteAsync(project, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);
	}
}