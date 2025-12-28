using Fluxor;
using FocusFlow.BlazorApp.Services;
using FocusFlow.BlazorApp.Shared.Extensions;

namespace FocusFlow.BlazorApp.Store.Tasks;

/// <summary>
/// Effects for Tasks feature
/// </summary>
public class TasksEffects
{
    private readonly IApiService _apiService;
    private readonly ILogger<TasksEffects> _logger;
    private readonly IState<TasksState> _state;

    public TasksEffects(IApiService apiService, ILogger<TasksEffects> logger, IState<TasksState> state)
    {
        _apiService = apiService;
        _logger = logger;
        _state = state;
    }

    [EffectMethod]
    public async Task HandleLoadTasks(TasksActions.LoadTasksAction action, IDispatcher dispatcher)
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
            dispatcher.Dispatch(new TasksActions.LoadTasksSuccessAction(result.Data ?? new()));
        }
        else
        {
            _logger.LogError("Failed to load tasks: {Error}", result.Error);
            dispatcher.Dispatch(new TasksActions.LoadTasksFailureAction(result.Error ?? "Unknown error"));
        }
    }

    [EffectMethod]
    public async Task HandleSetStatusFilter(TasksActions.SetStatusFilterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Applying status filter: {Status}", action.Status);
        
        // Filter is already set in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksActions.LoadTasksAction());
    }

    [EffectMethod]
    public async Task HandleSetPriorityFilter(TasksActions.SetPriorityFilterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Applying priority filter: {Priority}", action.Priority);
        
        // Filter is already set in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksActions.LoadTasksAction());
    }

    [EffectMethod]
    public async Task HandleSetOverdueFilter(TasksActions.SetOverdueFilterAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Applying overdue filter: {IsOverdue}", action.IsOverdue);
        
        // Filter is already set in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksActions.LoadTasksAction());
    }

    [EffectMethod]
    public async Task HandleClearFilters(TasksActions.ClearFiltersAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Clearing all filters");
        
        // Filters cleared in reducer, now reload tasks
        await Task.CompletedTask;
        dispatcher.Dispatch(new TasksActions.LoadTasksAction());
    }

    // Task Detail effects
    [EffectMethod]
    public async Task HandleLoadTaskById(TasksActions.LoadTaskByIdAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Loading task by ID: {TaskId}", action.TaskId);

        var result = await _apiService.GetTaskByIdAsync(action.TaskId);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully loaded task: {TaskId}", action.TaskId);
            dispatcher.Dispatch(new TasksActions.LoadTaskByIdSuccessAction(result.Data!));
        }
        else
        {
            _logger.LogError("Failed to load task {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksActions.LoadTaskByIdFailureAction(result.Error ?? "Unknown error"));
        }
    }
  
    [EffectMethod]
    public async Task HandleUpdateTaskFromResult(TasksActions.UpdateTaskFromResultAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Updating task from edit result: {TaskId}", action.TaskId);

        // Convert TaskEditResult to UpdateTaskDto using extension method
        var updateDto = action.EditResult.ToUpdateDto();
        var result = await _apiService.UpdateTaskAsync(action.TaskId, updateDto);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully updated task: {TaskId}", action.TaskId);
            dispatcher.Dispatch(new TasksActions.UpdateTaskSuccessAction(result.Data!));
        }
        else
        {
            _logger.LogError("Failed to update task {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksActions.UpdateTaskFailureAction(result.Error ?? "Unknown error"));
        }
    }    

    [EffectMethod]
    public async Task HandleUpdateTaskStatus(TasksActions.UpdateTaskStatusAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Updating task status: {TaskId} to {Status}", action.TaskId, action.Status);

        var result = await _apiService.UpdateTaskStatusAsync(action.TaskId, action.Status);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully updated task status: {TaskId}", action.TaskId);
            // Reload the task to get updated data
            dispatcher.Dispatch(new TasksActions.LoadTaskByIdAction(action.TaskId));
        }
        else
        {
            _logger.LogError("Failed to update task status {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksActions.UpdateTaskStatusFailureAction(result.Error ?? "Unknown error"));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteTask(TasksActions.DeleteTaskAction action, IDispatcher dispatcher)
    {
        _logger.LogInformation("Deleting task: {TaskId}", action.TaskId);

        var result = await _apiService.DeleteTaskAsync(action.TaskId);

        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully deleted task: {TaskId}", action.TaskId);
            dispatcher.Dispatch(new TasksActions.DeleteTaskSuccessAction(action.TaskId));
        }
        else
        {
            _logger.LogError("Failed to delete task {TaskId}: {Error}", action.TaskId, result.Error);
            dispatcher.Dispatch(new TasksActions.DeleteTaskFailureAction(result.Error ?? "Unknown error"));
        }
    }
}
