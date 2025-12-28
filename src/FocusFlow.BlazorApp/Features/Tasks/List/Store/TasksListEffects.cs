using Fluxor;
using FocusFlow.BlazorApp.Features.Tasks.Edit.Extensions;
using FocusFlow.BlazorApp.Services;

namespace FocusFlow.BlazorApp.Features.Tasks.List.Store;

/// <summary>
/// Effects for Tasks feature
/// </summary>
public class TasksListEffects
{
    private readonly IApiService _apiService;
    private readonly ILogger<TasksListEffects> _logger;
    private readonly IState<TasksListState> _state;

    public TasksListEffects(IApiService apiService, ILogger<TasksListEffects> logger, IState<TasksListState> state)
    {
        _apiService = apiService;
        _logger = logger;
        _state = state;
    }

    [EffectMethod]
    public async Task HandleLoadTasks(TasksListActions.LoadTasksAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Loading tasks with filters...");

        // Get filters from current state
        var status = _state.Value.StatusFilter;
        var priority = _state.Value.PriorityFilter;
        var isOverdue = _state.Value.IsOverdueFilter;

        var result = await _apiService.GetTasksFilteredAsync(status, priority, isOverdue);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully loaded {Count} tasks", result.Data?.Count ?? 0);
            dispatcher.Dispatch(new TasksListActions.LoadTasksSuccessAction(result.Data ?? new()));
        }
        else
        {
            _logger.LogError("Failed to load tasks: {Error}", result.Error);
            dispatcher.Dispatch(new TasksListActions.LoadTasksFailureAction(result.Error ?? "Unknown error"));
        }
    }

    [EffectMethod]
    public async Task HandleSetStatusFilter(TasksListActions.SetStatusFilterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Applying status filter: {Status}", action.Status);
        
        // Filter is already set in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksListActions.LoadTasksAction());
    }

    [EffectMethod]
    public async Task HandleSetPriorityFilter(TasksListActions.SetPriorityFilterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Applying priority filter: {Priority}", action.Priority);
        
        // Filter is already set in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksListActions.LoadTasksAction());
    }

    [EffectMethod]
    public async Task HandleSetOverdueFilter(TasksListActions.SetOverdueFilterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Applying overdue filter: {IsOverdue}", action.IsOverdue);
        
        // Filter is already set in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksListActions.LoadTasksAction());
    }

    [EffectMethod]
    public async Task HandleClearFilters(TasksListActions.ClearFiltersAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Clearing all filters");
        
        // Filters cleared in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksListActions.LoadTasksAction());
    }

    // Task Detail effects
    [EffectMethod]
    public async Task HandleLoadTaskById(TasksListActions.LoadTaskByIdAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Loading task by ID: {TaskId}", action.TaskId);

        var result = await _apiService.GetTaskByIdAsync(action.TaskId);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully loaded task: {TaskId}", action.TaskId);
            dispatcher.Dispatch(new TasksListActions.LoadTaskByIdSuccessAction(result.Data!));
        }
        else
        {
            _logger.LogError("Failed to load task {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksListActions.LoadTaskByIdFailureAction(result.Error ?? "Unknown error"));
        }
    }
  
    [EffectMethod]
    public async Task HandleUpdateTaskFromResult(TasksListActions.UpdateTaskFromResultAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Updating task from edit result: {TaskId}", action.TaskId);

        // Convert TaskEditResult to UpdateTaskRequest using extension method
        var updateRequest = action.EditResult.ToUpdateRequest();
        var result = await _apiService.UpdateTaskAsync(action.TaskId, updateRequest);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully updated task: {TaskId}", action.TaskId);
            dispatcher.Dispatch(new TasksListActions.UpdateTaskSuccessAction(result.Data!));
        }
        else
        {
            _logger.LogError("Failed to update task {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksListActions.UpdateTaskFailureAction(result.Error ?? "Unknown error"));
        }
    }    

    [EffectMethod]
    public async Task HandleUpdateTaskStatus(TasksListActions.UpdateTaskStatusAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Updating task status: {TaskId} to {Status}", action.TaskId, action.Status);

        var result = await _apiService.UpdateTaskStatusAsync(action.TaskId, action.Status);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully updated task status: {TaskId}", action.TaskId);
            // Reload the task to get updated data
            dispatcher.Dispatch(new TasksListActions.LoadTaskByIdAction(action.TaskId));
        }
        else
        {
            _logger.LogError("Failed to update task status {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksListActions.UpdateTaskStatusFailureAction(result.Error ?? "Unknown error"));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteTask(TasksListActions.DeleteTaskAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Deleting task: {TaskId}", action.TaskId);

        var result = await _apiService.DeleteTaskAsync(action.TaskId);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully deleted task: {TaskId}", action.TaskId);
            dispatcher.Dispatch(new TasksListActions.DeleteTaskSuccessAction(action.TaskId));
        }
        else
        {
            _logger.LogError("Failed to delete task {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksListActions.DeleteTaskFailureAction(result.Error ?? "Unknown error"));
        }
    }
}
