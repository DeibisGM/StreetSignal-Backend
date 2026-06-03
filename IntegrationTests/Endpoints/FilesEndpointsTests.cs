using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using StreetSignalApi.DTOs.Responses;

namespace StreetSignalApi.IntegrationTests.Endpoints;

public class FilesEndpointsTests : IClassFixture<StreetSignalWebAppFactory>
{
    private readonly StreetSignalWebAppFactory _factory;
    public FilesEndpointsTests(StreetSignalWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Upload_without_token_returns_401()
    {
        var client = _factory.CreateClient();

        using var content = new MultipartFormDataContent();
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "tiny.jpg");

        var resp = await client.PostAsync("/api/files/upload", content);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Upload_with_wrong_content_type_returns_400()
    {
        var client = await _factory.AuthedAsync(StreetSignalWebAppFactory.CitizenEmail);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "note.txt");

        var resp = await client.PostAsync("/api/files/upload", content);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body!.Code.Should().BeOneOf("VALIDATION_ERROR", "INVALID_FILE_TYPE");
    }
}
