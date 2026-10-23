using System.Net;
using System.Net.Http.Json;
using KodingTask.Api.Contracts;

namespace KodingTask.Api.Tests;

public sealed class RateLimitTests
{
    [Fact]
    public async Task Refuses_conversions_over_the_limit_with_a_json_429()
    {
        using var factory = new ApiFactory();
        factory.Settings["RateLimits:ConversionsPerMinute"] = "3";
        var client = factory.CreateClient();

        for (var attempt = 0; attempt < 3; attempt++)
            (await client.GetAsync("/api/conversions?amount=1")).EnsureSuccessStatusCode();

        var refused = await client.GetAsync("/api/conversions?amount=1");

        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
        Assert.True(refused.Headers.Contains("Retry-After"));
        var error = await refused.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("TooManyRequests", error?.Error);
    }

    [Fact]
    public async Task Does_not_limit_the_health_endpoint()
    {
        using var factory = new ApiFactory();
        factory.Settings["RateLimits:ConversionsPerMinute"] = "1";
        var client = factory.CreateClient();

        for (var attempt = 0; attempt < 5; attempt++)
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
    }
}
