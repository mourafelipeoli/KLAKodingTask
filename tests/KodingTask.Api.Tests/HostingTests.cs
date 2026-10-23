using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KodingTask.Api.Contracts;
using KodingTask.Conversion;

namespace KodingTask.Api.Tests;

public sealed class HostingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_reports_status_and_version_without_caching()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("ok", body.RootElement.GetProperty("status").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.RootElement.GetProperty("version").GetString()));
    }

    [Fact]
    public async Task Health_answers_head_without_a_body_and_refuses_other_methods()
    {
        var head = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/health"));
        Assert.Equal(HttpStatusCode.OK, head.StatusCode);
        Assert.Empty(await head.Content.ReadAsByteArrayAsync());

        var post = await _client.PostAsync("/health", null);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, post.StatusCode);
    }

    [Fact]
    public async Task App_info_lists_optional_features_as_unavailable_by_default()
    {
        var info = await _client.GetFromJsonAsync<JsonElement>("/api/app-info");

        var features = info.GetProperty("features");
        foreach (var name in new[] { "email", "whatsApp", "ai", "machineTranslation", "externalLogin", "deliveries" })
            Assert.False(features.GetProperty(name).GetBoolean(), name);
        Assert.Equal("https://klakodingtask.mourafelipe.com", info.GetProperty("publicSiteUrl").GetString());
        Assert.Equal("https://mourafelipe.com", info.GetProperty("author").GetProperty("url").GetString());
        Assert.Equal("https://mourafelipe.com/terminal", info.GetProperty("author").GetProperty("terminalUrl").GetString());
    }

    [Fact]
    public async Task Sends_security_headers_and_a_content_security_policy()
    {
        var response = await _client.GetAsync("/api/conversions/languages");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        var policy = response.Headers.GetValues("Content-Security-Policy").Single();
        Assert.Contains("script-src 'self'", policy);
        Assert.Contains("frame-ancestors 'none'", policy);
    }

    [Fact]
    public void Creates_the_local_sqlite_database_on_startup()
    {
        _ = factory.Services;
        Assert.True(File.Exists(factory.DatabaseFile));
    }

    [Fact]
    public void Every_conversion_error_has_an_api_message()
    {
        foreach (var error in Enum.GetValues<ConversionError>())
            Assert.False(string.IsNullOrWhiteSpace(ErrorResponse.For(error).Message), error.ToString());
    }

    [Fact]
    public async Task Publishes_the_openapi_document()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();
        Assert.Contains("/api/conversions", await response.Content.ReadAsStringAsync());
    }
}
