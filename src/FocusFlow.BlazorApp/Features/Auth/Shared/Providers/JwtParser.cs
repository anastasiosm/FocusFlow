using System.Security.Claims;
using System.Text.Json;

namespace FocusFlow.BlazorApp.Auth;

public static class JwtParser
{
	public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
	{
		if (string.IsNullOrWhiteSpace(jwt))
		{
			throw new ArgumentException("JWT cannot be null or empty", nameof(jwt));
		}

		var parts = jwt.Split('.');
		if (parts.Length != 3)
		{
			throw new ArgumentException("Invalid JWT format", nameof(jwt));
		}

		var payload = parts[1];
		var jsonBytes = ParseBase64WithoutPadding(payload);
		var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

		return keyValuePairs?.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty))
			   ?? Enumerable.Empty<Claim>();
	}

	private static byte[] ParseBase64WithoutPadding(string base64)
	{
		switch (base64.Length % 4)
		{
			case 2: base64 += "=="; break;
			case 3: base64 += "="; break;
		}
		return Convert.FromBase64String(base64);
	}
}
