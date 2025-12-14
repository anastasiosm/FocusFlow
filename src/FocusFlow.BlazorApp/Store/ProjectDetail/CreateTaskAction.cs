using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.CreateTask;

namespace FocusFlow.BlazorApp.Store.ProjectDetail;

public record CreateTaskAction(CreateTaskCommand Command);

public record CreateTaskSuccessAction(TaskDto Task);

public record CreateTaskFailureAction(string ErrorMessage);
