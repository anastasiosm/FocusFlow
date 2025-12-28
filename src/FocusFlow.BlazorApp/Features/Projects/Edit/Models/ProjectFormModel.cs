using FocusFlow.BlazorApp.Features.Projects.Create.Models;

namespace FocusFlow.BlazorApp.Features.Projects.Edit.Models;

public class ProjectFormModel
{
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }

	public CreateProjectDto ToDto() => new(Name, Description);
}