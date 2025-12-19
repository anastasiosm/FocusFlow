using FocusFlow.Application.Features.Tasks.Common;
using FocusFlow.Application.Features.Tasks.CreateTask;
using FocusFlow.Domain.Enums;
using Refit;

namespace FocusFlow.BlazorApp.Services.Api;

public interface ITasksApi
{
    [Get("/api/projects/{projectId}/tasks")]
    Task<List<TaskDto>> GetTasksAsync(Guid projectId);

    [Get("/api/tasks")]
    Task<List<TaskDto>> GetTasksFilteredAsync(
        [Query] ProjectTaskStatus? status = null, 
        [Query] Priority? priority = null, 
        [Query] bool? isOverdue = null);

    [Post("/api/tasks")]
    Task<TaskDto> CreateTaskAsync([Body] CreateTaskDto dto);

    [Delete("/api/tasks/{id}")]
    Task DeleteTaskAsync(Guid id);
}