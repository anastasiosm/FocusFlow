using Fluxor;
using FocusFlow.BlazorApp.Features.Tasks.Shared.Models;

namespace FocusFlow.BlazorApp.Features.Tasks.Detail.Store;

/// <summary>
/// State for Task Detail feature
/// </summary>
[FeatureState]
public record TaskDetailState
{
	public TaskResponse? Task { get; init; }
	public bool IsLoading { get; init; }
	public string? ErrorMessage { get; init; }
}
