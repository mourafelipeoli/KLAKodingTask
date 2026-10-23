using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using KodingTask.Api.Contracts;
using KodingTask.Api.Messaging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KodingTask.Api.Tests;

internal sealed partial class MessagingTestHost : IDisposable
{
    private MessagingTestHost(ApiFactory factory)
    {
        Factory = factory;
    }

    public ApiFactory Factory { get; }

    public RecordingEmailSender Emails { get; } = new();

    public RecordingWhatsAppSender WhatsApp { get; } = new();

    public static MessagingTestHost Create(Action<Dictionary<string, string>>? configure = null, Action<IServiceCollection>? services = null)
    {
        var host = new MessagingTestHost(new ApiFactory());
        var settings = host.Factory.Settings;
        settings["Email:Sender"] = "sender@example.com";
        settings["Email:Region"] = "us-east-1";
        settings["WhatsApp:PhoneNumberId"] = "1234567890";
        settings["WhatsApp:AccessToken"] = "test-token";
        configure?.Invoke(settings);
        host.Factory.ServiceOverrides.Add(collection =>
        {
            collection.RemoveAll<IEmailSender>();
            collection.AddSingleton<IEmailSender>(host.Emails);
            collection.RemoveAll<IWhatsAppSender>();
            collection.AddSingleton<IWhatsAppSender>(host.WhatsApp);
            services?.Invoke(collection);
        });
        return host;
    }

    public async Task<AccountClient> ClientAsync()
    {
        var client = new AccountClient(Factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") }));
        await client.RefreshAntiforgeryAsync();
        return client;
    }

    public async Task<AccountClient> ConfirmedAccountAsync(string email = "visitor@example.com", string password = "a long enough password")
    {
        var client = await ClientAsync();
        (await client.PostAsync("/api/account/register", new { email, password, uiLocale = "en-US" })).EnsureSuccessStatusCode();
        var (user, code) = LinkParameters(Emails.LastTo(email).Email.Text, "confirm-email");
        (await client.PostAsync("/api/account/confirm-email", new { userId = user, code })).EnsureSuccessStatusCode();
        return client;
    }

    public static (string User, string Code) LinkParameters(string text, string page)
    {
        var match = Regex.Match(text, $@"/{page}\?user=([^&\s]+)&code=([^&\s]+)");
        Assert.True(match.Success, $"No {page} link in the email.");
        return (Uri.UnescapeDataString(match.Groups[1].Value), Uri.UnescapeDataString(match.Groups[2].Value));
    }

    public void Dispose() => Factory.Dispose();
}

internal sealed class AccountClient(HttpClient http)
{
    public HttpClient Http { get; } = http;

    public async Task RefreshAntiforgeryAsync()
    {
        var token = await Http.GetFromJsonAsync<AntiforgeryResponse>("/api/account/antiforgery");
        Http.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
        Http.DefaultRequestHeaders.Add("X-XSRF-TOKEN", token!.Token);
    }

    public async Task<HttpResponseMessage> PostAsync(string path, object body)
    {
        var response = await Http.PostAsJsonAsync(path, body);
        if (path.Contains("sign-", StringComparison.Ordinal) || path.Contains("register", StringComparison.Ordinal)
            || path.Contains("confirm-email", StringComparison.Ordinal) || path.Contains("reset-password", StringComparison.Ordinal))
        {
            await RefreshAntiforgeryAsync();
        }

        return response;
    }
}

internal sealed class RecordingEmailSender : IEmailSender
{
    public ConcurrentQueue<(string To, ComposedEmail Email)> Sent { get; } = new();

    public Task<string> SendAsync(string to, ComposedEmail email, CancellationToken cancellationToken)
    {
        Sent.Enqueue((to, email));
        return Task.FromResult($"email-{Sent.Count}");
    }

    public (string To, ComposedEmail Email) LastTo(string address) => Sent.Last(sent => sent.To == address);
}

internal sealed class RecordingWhatsAppSender : IWhatsAppSender
{
    public ConcurrentQueue<(string Phone, string Locale, string Amount, string Words)> Results { get; } = new();

    public ConcurrentQueue<(string Phone, string Locale, string Code)> Codes { get; } = new();

    public Task<string> SendResultAsync(string phoneE164, string locale, string amountText, string words, CancellationToken cancellationToken)
    {
        Results.Enqueue((phoneE164, locale, amountText, words));
        return Task.FromResult($"wamid.result-{Results.Count}");
    }

    public Task<string> SendCodeAsync(string phoneE164, string locale, string code, CancellationToken cancellationToken)
    {
        Codes.Enqueue((phoneE164, locale, code));
        return Task.FromResult($"wamid.code-{Codes.Count}");
    }
}
