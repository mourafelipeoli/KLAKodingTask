using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace KodingTask.Api.Tests;

public sealed class DeliveryTests
{
    [Fact]
    public async Task Emails_the_result_converted_again_on_the_server_to_the_confirmed_address()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync("reader@example.com");

        var response = await client.PostAsync("/api/deliveries",
            new { channel = "email", amount = "45 100", language = "fr-BE", uiLocale = "en-US" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("r*****@example.com", body.GetProperty("maskedDestination").GetString());
        Assert.Equal(9, body.GetProperty("deliveriesLeftToday").GetInt32());

        var (to, email) = host.Emails.LastTo("reader@example.com");
        Assert.Equal("reader@example.com", to);
        Assert.Equal("US$ 45,100.00 in words", email.Subject);
        Assert.Contains("quarante-cinq mille cent dollars", email.Text);
        Assert.Contains("Français (Belgique)", email.Text);
        Assert.Contains("/?amount=45%20100%2C00&language=fr-BE&lang=en-US", email.Text);
    }

    [Fact]
    public async Task Sends_nothing_to_an_address_that_is_not_confirmed()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync("early@example.com");
        using (var scope = host.Factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var user = await users.FindByEmailAsync("early@example.com");
            user!.EmailConfirmed = false;
            await users.UpdateAsync(user);
        }

        var response = await client.PostAsync("/api/deliveries", new { channel = "email", amount = "1", language = "en-US" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(host.Emails.Sent);
    }

    [Fact]
    public async Task Requires_a_session()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.ClientAsync();

        var response = await client.PostAsync("/api/deliveries", new { channel = "email", amount = "1", language = "en-US" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refuses_invalid_amounts_with_the_conversion_error()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();

        var response = await client.PostAsync("/api/deliveries", new { channel = "email", amount = "12.50", language = "en-US" });

        Assert.Equal("DecimalPointNotAllowed", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Stops_at_the_daily_cap_of_the_account()
    {
        using var host = MessagingTestHost.Create(settings => settings["Limits:DeliveriesPerUserPerDay"] = "1");
        var client = await host.SignedInAccountAsync();
        (await client.PostAsync("/api/deliveries", new { channel = "email", amount = "1", language = "en-US" })).EnsureSuccessStatusCode();

        var refused = await client.PostAsync("/api/deliveries", new { channel = "email", amount = "2", language = "en-US" });

        Assert.Equal(HttpStatusCode.TooManyRequests, refused.StatusCode);
        Assert.Equal("UserLimitReached", (await refused.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Stops_at_the_instance_wide_email_cap()
    {
        using var host = MessagingTestHost.Create(settings => settings["Limits:EmailsPerDay"] = "1");
        var first = await host.SignedInAccountAsync("first@example.com");
        (await first.PostAsync("/api/deliveries", new { channel = "email", amount = "1", language = "en-US" })).EnsureSuccessStatusCode();
        var second = await host.SignedInAccountAsync("second@example.com");

        var refused = await second.PostAsync("/api/deliveries", new { channel = "email", amount = "1", language = "en-US" });

        Assert.Equal("DailyLimitReached", (await refused.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
    }

    [Fact]
    public async Task The_kill_switch_pauses_every_delivery()
    {
        using var paused = MessagingTestHost.Create(settings => settings["Limits:KillSwitch"] = "true");
        var client = await paused.SignedInAccountAsync();

        var refused = await client.PostAsync("/api/deliveries", new { channel = "email", amount = "1", language = "en-US" });

        Assert.Equal("Paused", (await refused.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
        Assert.Empty(paused.Emails.Sent);
    }

    [Fact]
    public async Task Sends_whatsapp_results_only_to_a_verified_number()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();

        var unverified = await client.PostAsync("/api/deliveries", new { channel = "whatsapp", amount = "2", language = "pl-PL" });
        Assert.Equal("PhoneNotVerified", (await unverified.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());

        (await client.PostAsync("/api/account/phone/code", new { phone = "+48 600 100 200" })).EnsureSuccessStatusCode();
        var (_, _, code) = host.WhatsApp.Codes.Single();
        (await client.PostAsync("/api/account/phone/verify", new { code })).EnsureSuccessStatusCode();

        var sent = await client.PostAsync("/api/deliveries", new { channel = "whatsapp", amount = "2", language = "pl-PL", uiLocale = "pl-PL" });

        Assert.Equal(HttpStatusCode.OK, sent.StatusCode);
        var (phone, locale, amount, words) = host.WhatsApp.Results.Single();
        Assert.Equal("+48600100200", phone);
        Assert.Equal("pl-PL", locale);
        Assert.Equal("US$ 2,00", amount);
        Assert.Equal("dwa dolary", words);
    }

    [Fact]
    public async Task Refuses_unknown_channels()
    {
        using var host = MessagingTestHost.Create();
        var client = await host.SignedInAccountAsync();

        var response = await client.PostAsync("/api/deliveries", new { channel = "sms", amount = "1", language = "en-US" });

        Assert.Equal("InvalidChannel", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetString());
    }
}
