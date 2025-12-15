using Blazored.LocalStorage;

public interface ITokenProvider
{
	string? GetToken();
	Task SetTokenAsync(string token, ILocalStorageService localStorage);
	Task ClearTokenAsync(ILocalStorageService localStorage);
	Task InitializeAsync(ILocalStorageService localStorage);
}
