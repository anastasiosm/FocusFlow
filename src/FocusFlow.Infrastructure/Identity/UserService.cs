using FocusFlow.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace FocusFlow.Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IApplicationUser?> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IApplicationUser?> FindByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<bool> CheckPasswordAsync(IApplicationUser user, string password)
    {
        if (user is not ApplicationUser appUser)
            return false;

        var result = await _signInManager.CheckPasswordSignInAsync(appUser, password, lockoutOnFailure: false);
        return result.Succeeded;
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(
        string email, 
        string password, 
        string? firstName = null, 
        string? lastName = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName ?? "User",
            LastName = lastName ?? "",
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
}