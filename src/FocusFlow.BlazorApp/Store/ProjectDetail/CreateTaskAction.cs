using FocusFlow.BlazorApp.Models.Dtos;
using FocusFlow.BlazorApp.Models;
using FocusFlow.Domain.Enums;

namespace FocusFlow.BlazorApp.Store.ProjectDetail;

public record CreateTaskAction(Guid ProjectId, CreateTaskFormModel FormModel);

public record CreateTaskSuccessAction(TaskDto Task);

public record CreateTaskFailureAction(string ErrorMessage);
