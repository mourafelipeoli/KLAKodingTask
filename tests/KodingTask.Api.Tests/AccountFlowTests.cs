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
    [Fact]
    public async Task Registering_signs_in_and_emails_a_confirmation_link_that_confirms_the_address()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.ClientAsync();

        var registered = await client.PostAsync("/api/account/register",
            new { email = "visitor@example.com", password = "a long enough password", uiLocale = "de-DE", returnUrl = "/?amount=1" });

        Assert.Equal(HttpStatusCode.OK, registered.StatusCode);
        var body = await registered.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("confirmationSent").GetBoolean());
        Assert.True(body.GetProperty("account").GetProperty("isSignedIn").GetBoolean());
        Assert.False(body.GetProperty("account").GetProperty("emailConfirmed").GetBoolean());

        var (_, email) = host.Emails.LastTo("visitor@example.com");
        Assert.Equal("Bestätigen Sie Ihre E-Mail-Adresse", email.Subject);
        Assert.Contains("returnUrl=%2F%3Famount%3D1", email.Text);

        var (user, code) = MessagingTestHost.LinkParameters(email.Text, "confirm-email");
        var confirmed = await client.PostAsync("/api/account/confirm-email", new { userId = user, code });
        var account = await confirmed.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(account.GetProperty("emailConfirmed").GetBoolean());
    }

    [Fact]
    public async Task Refuses_a_second_account_for_the_same_address_and_weak_passwords()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.ClientAsync();
        (await client.PostAsync("/api/account/register", new { email = "taken@example.com", password = "a long enough password" })).EnsureSuccessStatusCode();

        var other = await host.ClientAsync();
        var taken = await other.PostAsync("/api/account/register", new { email = "taken@example.com", password = "another long password" });
        var weak = await other.PostAsync("/api/account/register", new { email = "new@example.com", password = "short" });

        Assert.Equal(HttpStatusCode.Conflict, taken.StatusCode);
        Assert.Equal("WeakPassword", (await weak.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
    }

    [Fact]
    public async Task State_changing_calls_need_the_antiforgery_header()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.ClientAsync();
        client.Http.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");

        var response = await client.Http.PostAsJsonAsync("/api/account/sign-in", new { email = "a@example.com", password = "whatever-123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Locks_an_account_out_after_five_wrong_passwords()
    {
        using var host = MessagingTestHost.Create();
        await host.ConfirmedAccountAsync("locked@example.com");
        var stranger = await host.ClientAsync();

        HttpResponseMessage last = null!;
        for (var attempt = 0; attempt < 6; attempt++)
            last = await stranger.PostAsync("/api/account/sign-in", new { email = "locked@example.com", password = "wrong password!" });

        Assert.Equal("LockedOut", (await last.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Resets_a_forgotten_password_through_the_emailed_link_without_revealing_unknown_addresses()
    {
        using var host = MessagingTestHost.Create();
        await host.ConfirmedAccountAsync("forgetful@example.com");
        var client = await host.ClientAsync();
        var sentBefore = host.Emails.Sent.Count;

        var unknown = await client.PostAsync("/api/account/forgot-password", new { email = "nobody@example.com" });
        Assert.Equal(HttpStatusCode.NoContent, unknown.StatusCode);
        Assert.Equal(sentBefore, host.Emails.Sent.Count);

        (await client.PostAsync("/api/account/forgot-password", new { email = "forgetful@example.com", uiLocale = "en-US" })).EnsureSuccessStatusCode();
        var (user, code) = MessagingTestHost.LinkParameters(host.Emails.LastTo("forgetful@example.com").Email.Text, "reset-password");
        (await client.PostAsync("/api/account/reset-password", new { userId = user, code, password = "a brand new password" })).EnsureSuccessStatusCode();

        var fresh = await host.ClientAsync();
        var signedIn = await fresh.PostAsync("/api/account/sign-in", new { email = "forgetful@example.com", password = "a brand new password" });
        Assert.True((await signedIn.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("isSignedIn").GetBoolean());
    }

    [Fact]
    public async Task Registration_is_unavailable_without_email()
    {
        using var factory = new ApiFactory();
        var client = new AccountClient(factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") }));
        await client.RefreshAntiforgeryAsync();

        var response = await client.PostAsync("/api/account/register", new { email = "a@example.com", password = "a long enough password" });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Caps_registrations_per_client_address()
    {
        using var host = MessagingTestHost.Create(settings => settings["Limits:RegistrationsPerClientPerDay"] = "1");
        var first = await host.ClientAsync();
        (await first.PostAsync("/api/account/register", new { email = "one@example.com", password = "a long enough password" })).EnsureSuccessStatusCode();

        var second = await host.ClientAsync();
        var refused = await second.PostAsync("/api/account/register", new { email = "two@example.com", password = "a long enough password" });

        Assert.Equal("RegistrationLimitReached", (await refused.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
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
        var client = await host.ConfirmedAccountAsync();

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
        var client = await host.ConfirmedAccountAsync();
        (await client.PostAsync("/api/account/phone/code", new { phone = "+4917012345678" })).EnsureSuccessStatusCode();
        var (_, _, code) = host.WhatsApp.Codes.Single();

        clock.Advance(TimeSpan.FromMinutes(11));
        var expired = await client.PostAsync("/api/account/phone/verify", new { code });

        Assert.Equal("CodeExpired", (await expired.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Phone_codes_need_a_confirmed_email_and_an_international_number()
    {
        using var host = MessagingTestHost.Create();
        var unconfirmed = await host.ClientAsync();
        (await unconfirmed.PostAsync("/api/account/register", new { email = "fresh@example.com", password = "a long enough password" })).EnsureSuccessStatusCode();
        var refused = await unconfirmed.PostAsync("/api/account/phone/code", new { phone = "+4917012345678" });
        Assert.Equal(HttpStatusCode.Forbidden, refused.StatusCode);

        var confirmed = await host.ConfirmedAccountAsync("confirmed@example.com");
        var national = await confirmed.PostAsync("/api/account/phone/code", new { phone = "0170 1234567" });
        Assert.Equal("InvalidPhone", (await national.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
        Assert.Empty(host.WhatsApp.Codes);
    }

    [Fact]
    public async Task Writes_every_message_to_the_history_with_a_masked_destination()
    {
        using var host = MessagingTestHost.Create();
        await host.ConfirmedAccountAsync("history@example.com");

        using var scope = host.Factory.Services.CreateScope();
        var messages = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().OutboundMessages.ToList();

        var confirmation = Assert.Single(messages);
        Assert.Equal("h******@example.com", confirmation.MaskedDestination);
        Assert.Equal("email-1", confirmation.ProviderMessageId);
    }
}
