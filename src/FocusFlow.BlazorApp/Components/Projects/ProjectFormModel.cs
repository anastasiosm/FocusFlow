using FocusFlow.Application.Features.Projects.CreateProject;

namespace FocusFlow.BlazorApp.Components.Projects;

public class ProjectFormModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public CreateProjectDto ToDto() => new(Name, Description);
}