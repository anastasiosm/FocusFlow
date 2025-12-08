using FocusFlow.Application.Interfaces;
using FocusFlow.Application.Tasks.Commands;
using FocusFlow.Domain.Entities;
using FocusFlow.Domain.Exceptions;
using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Threading;
using System;
using MediatR;

namespace FocusFlow.Application.Tests.Tasks.Commands
{
    public class DeleteTaskCommandTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DeleteTaskCommandHandler _handler;

        public DeleteTaskCommandTests()
        {
            _taskRepositoryMock = new Mock<ITaskRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new DeleteTaskCommandHandler(
                _taskRepositoryMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldDeleteTask_WhenTaskExists()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var command = new DeleteTaskCommand(taskId);
            var task = new ProjectTask("Test Task", "Test Description", Guid.NewGuid());

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(command.TaskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(task);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _taskRepositoryMock.Verify(x => x.DeleteAsync(task, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(Unit.Value, result);
        }

        [Fact]
        public async Task Handle_ShouldThrowFocusFlowNotFoundException_WhenTaskDoesNotExist()
        {
            // Arrange
            var command = new DeleteTaskCommand(Guid.NewGuid());

            _taskRepositoryMock.Setup(x => x.GetByIdAsync(command.TaskId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProjectTask)null);

            // Act & Assert
            await Assert.ThrowsAsync<FocusFlowNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
