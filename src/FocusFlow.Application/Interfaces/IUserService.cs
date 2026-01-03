namespace FocusFlow.Application.Interfaces;

public interface IUserService
{
    Task<IApplicationUser?> FindByEmailAsync(string email);
    Task<IApplicationUser?> FindByIdAsync(string userId);
    Task<bool> CheckPasswordAsync(IApplicationUser user, string password);
    Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(
        string email, 
        string password, 
        string? firstName = null, 
        string? lastName = null);
}