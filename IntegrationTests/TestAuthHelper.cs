using System.Net.Http.Headers;
using System.Net.Http.Json;
using StreetSignalApi.DTOs.Requests;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.IntegrationTests;

public static class TestAuthHelper
{
    public static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var resp = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.Token;
    }

    public static async Task<HttpClient> AuthedAsync(this StreetSignalWebAppFactory factory, string email)
    {
        var client = factory.CreateClient();
        var token = await LoginAsync(client, email, StreetSignalWebAppFactory.Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
