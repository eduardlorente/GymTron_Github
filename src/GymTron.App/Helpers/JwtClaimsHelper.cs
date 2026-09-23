using System.Text.Json;

namespace GymTron.App.Helpers;

public static class JwtClaimsHelper
{
    public static string? GetUsernameFromToken(string? jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            return null;
        }

        string[] parts = jwtToken.Split('.');
        if (parts.Length < 2)
        {
            return null;
        }

        try
        {
            string payload = parts[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            byte[] bytes = Convert.FromBase64String(payload);
            using var doc = JsonDocument.Parse(bytes);

            if (doc.RootElement.TryGetProperty("unique_name", out var uniqueNameProp))
            {
                return uniqueNameProp.GetString();
            }

            if (doc.RootElement.TryGetProperty("name", out var nameProp))
            {
                return nameProp.GetString();
            }

            if (doc.RootElement.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out var claimNameProp))
            {
                return claimNameProp.GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
