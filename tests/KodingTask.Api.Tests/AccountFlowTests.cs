using System.Net;
using System.Net.Http.Json;
using KodingTask.Data;
using Microsoft.Extensions.DependencyInjection;

namespace KodingTask.Api.Tests;

public sealed class AccountFlowTests
{
    [Theory]
    [InlineData("/api/account/register")]
    [InlineData("/api/account/sign-in")]
    [InlineData("/api/account/forgot-password")]
    [InlineData("/api/account/reset-password")]
    [InlineData("/api/account/confirm-email")]
    [InlineData("/api/account/phone/code")]
    [InlineData("/api/account/phone/verify")]
    public async Task There_are_no_password_or_phone_code_endpoints(string path)
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();

        var response = await client.PostAsync(path, new { email = "visitor@example.com", password = "a long enough password", phone = "+4917012345678" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task State_changing_calls_need_the_antiforgery_header()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();
        client.Http.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");

        var response = await client.Http.PostAsJsonAsync("/api/deliveries", new { channel = "email", amount = "1", language = "en-US" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(host.Emails.Sent);
    }

    [Fact]
    public async Task Writes_every_message_to_the_history_with_a_masked_destination()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync("history@example.com");
        (await client.PostAsync("/api/deliveries", new { channel = "email", amount = "1", language = "en-US" })).EnsureSuccessStatusCode();

        using var scope = host.Factory.Services.CreateScope();
        var messages = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().OutboundMessages.ToList();

        var result = Assert.Single(messages);
        Assert.Equal("h******@example.com", result.MaskedDestination);
        Assert.Equal("email-1", result.ProviderMessageId);
    }
}
