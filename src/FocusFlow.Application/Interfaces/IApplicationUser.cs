namespace FocusFlow.Application.Interfaces;

public interface IApplicationUser
{
    string Id { get; }
    string? UserName { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    string FullName { get; }
}