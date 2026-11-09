using System.Text.Json;
using BlazorApp.UI.Auth.Models;

namespace BlazorApp.UI.Auth.Services
{
    /// <summary>
    /// Loads demo credentials from the API. The password is not stored in this UI.
    /// </summary>
    public static class DemoCredentialsClient
    {
        public static async Task<DemoCredentials?> GetAsync(HttpClient client)
        {
            try
            {
                using var response = await client.GetAsync("api/auth/demo");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var body = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (body.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var email = ReadString(body, "email", "Email");
                var password = ReadString(body, "password", "Password");
                if (email.Length == 0 || password.Length == 0)
                {
                    return null;
                }

                return new DemoCredentials(email, password);
            }
            catch
            {
                return null;
            }
        }

        private static string ReadString(JsonElement body, params string[] names)
        {
            foreach (var name in names)
            {
                if (body.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    var text = value.GetString()?.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        return text;
                    }
                }
            }

            return string.Empty;
        }
    }
}
