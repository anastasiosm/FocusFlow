using AutoMapper;
using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Interfaces;
using FocusFlow.Domain.Entities;
using MediatR;

namespace FocusFlow.Application.Features.Projects.CreateProject;

/// <summary>
/// Handler for CreateProjectCommand
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
	private readonly IProjectRepository _projectRepository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;

	public CreateProjectCommandHandler(
		IProjectRepository projectRepository,
		IUnitOfWork unitOfWork,
		IMapper mapper)
	{
		_projectRepository = projectRepository;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
	}

	public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
	{
		var project = new Project(request.Name, request.Description, request.OwnerId);

		await _projectRepository.AddAsync(project, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return _mapper.Map<ProjectDto>(project);
	}
}