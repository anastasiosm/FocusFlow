using AutoMapper;
using FocusFlow.Application.Features.Projects.Common;
using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Interfaces;
using Moq;

namespace FocusFlow.Application.Tests;

/// <summary>
/// Base class for application tests with common setup
/// </summary>
public abstract class TestBase
{
	protected readonly Mock<IProjectRepository> MockProjectRepository;
	protected readonly Mock<ITaskRepository> MockTaskRepository;
	protected readonly Mock<IUnitOfWork> MockUnitOfWork;
	protected readonly IMapper Mapper;

	protected TestBase()
	{
		MockProjectRepository = new Mock<IProjectRepository>();
		MockTaskRepository = new Mock<ITaskRepository>();
		MockUnitOfWork = new Mock<IUnitOfWork>();

		// Setup AutoMapper
		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<ProjectMappingProfile>();
			cfg.AddProfile<TaskMappingProfile>();
		});
		Mapper = config.CreateMapper();
	}

	protected void VerifyUnitOfWorkSaveChanges(Times times)
	{
		MockUnitOfWork.Verify(
			uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
			times);
	}
}