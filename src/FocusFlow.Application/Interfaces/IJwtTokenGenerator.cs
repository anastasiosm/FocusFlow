namespace FocusFlow.Application.Interfaces;

public interface IJwtTokenGenerator
{
    Task<string> GenerateAsync(IApplicationUser user);
}