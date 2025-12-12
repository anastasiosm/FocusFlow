using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Exceptions;
using MediatR;

namespace FocusFlow.Application.Features.Projects.UpdateProject;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, bool>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IUnitOfWork _unitOfWork;

	public UpdateProjectCommandHandler(
		IProjectRepository projectRepository,
		IUnitOfWork unitOfWork)
	{
		_projectRepository = projectRepository;
		_unitOfWork = unitOfWork;
	}

	public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
	{
		var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);

		if (project == null)
			throw new FocusFlowNotFoundException("Project", request.Id);

		if (project.OwnerId != request.UserId)
			throw new FocusFlowUnauthorizedException("You do not have permission to update this project");

		project.Update(request.Name, request.Description);

		await _projectRepository.UpdateAsync(project, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return true;
	}
}