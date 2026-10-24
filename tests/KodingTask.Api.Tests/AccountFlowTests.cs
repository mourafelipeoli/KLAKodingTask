using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KodingTask.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace KodingTask.Api.Tests;

public sealed class AccountFlowTests
{
    [Theory]
    [InlineData("/api/account/register")]
    [InlineData("/api/account/sign-in")]
    [InlineData("/api/account/forgot-password")]
    [InlineData("/api/account/reset-password")]
    [InlineData("/api/account/confirm-email")]
    public async Task There_are_no_password_accounts(string path)
    {
        using var host = MessagingTestHost.Create();
        var client = await host.ClientAsync();

        var response = await client.PostAsync(path, new { email = "visitor@example.com", password = "a long enough password" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task State_changing_calls_need_the_antiforgery_header()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();
        client.Http.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");

        var response = await client.Http.PostAsJsonAsync("/api/account/phone/code", new { phone = "+4917012345678" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(host.WhatsApp.Codes);
    }

    [Fact]
    public async Task Verifies_a_whatsapp_number_with_a_one_time_code()
    {
        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        using var host = MessagingTestHost.Create(services: services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(clock);
        });
        var client = await host.SignedInAccountAsync();

        var sent = await client.PostAsync("/api/account/phone/code", new { phone = "+49 170 1234-5678", uiLocale = "en-US" });
        Assert.Equal("+49*******5678", (await sent.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("maskedNumber").GetString());
        var (phone, _, code) = host.WhatsApp.Codes.Single();
        Assert.Equal("+4917012345678", phone);
        Assert.Matches("^[0-9]{6}$", code);

        var tooSoon = await client.PostAsync("/api/account/phone/code", new { phone = "+4917012345678" });
        Assert.Equal("CodeCooldown", (await tooSoon.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());

        var wrong = await client.PostAsync("/api/account/phone/verify", new { code = code == "000000" ? "111111" : "000000" });
        Assert.Equal("WrongCode", (await wrong.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());

        var verified = await client.PostAsync("/api/account/phone/verify", new { code });
        Assert.Equal("+49*******5678", (await verified.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("whatsAppNumber").GetString());
    }

    [Fact]
    public async Task Expired_codes_are_refused()
    {
        var clock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        using var host = MessagingTestHost.Create(services: services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(clock);
        });
        var client = await host.SignedInAccountAsync();
        (await client.PostAsync("/api/account/phone/code", new { phone = "+4917012345678" })).EnsureSuccessStatusCode();
        var (_, _, code) = host.WhatsApp.Codes.Single();

        clock.Advance(TimeSpan.FromMinutes(11));
        var expired = await client.PostAsync("/api/account/phone/verify", new { code });

        Assert.Equal("CodeExpired", (await expired.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Phone_codes_need_a_session_and_an_international_number()
    {
        using var host = MessagingTestHost.Create();
        var anonymous = await host.ClientAsync();
        var refused = await anonymous.PostAsync("/api/account/phone/code", new { phone = "+4917012345678" });
        Assert.Equal(HttpStatusCode.Unauthorized, refused.StatusCode);

        var signedIn = await host.SignedInAccountAsync();
        var national = await signedIn.PostAsync("/api/account/phone/code", new { phone = "0170 1234567" });
        Assert.Equal("InvalidPhone", (await national.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
        Assert.Empty(host.WhatsApp.Codes);
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
