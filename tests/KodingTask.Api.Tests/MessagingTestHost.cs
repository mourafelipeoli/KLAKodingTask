using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using KodingTask.Api.Contracts;
using KodingTask.Api.Messaging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;

namespace KodingTask.Api.Tests;

internal sealed class MessagingTestHost : IDisposable
{
    private MessagingTestHost(ApiFactory factory)
    {
        Factory = factory;
    }

    public ApiFactory Factory { get; }

    public FakeFmIdProvider FmId { get; } = new();

    public RecordingEmailSender Emails { get; } = new();

    public RecordingWhatsAppSender WhatsApp { get; } = new();

    public static MessagingTestHost Create(
        Action<Dictionary<string, string>>? configure = null,
        Action<IServiceCollection>? services = null,
        bool withFmId = true)
    {
        var host = new MessagingTestHost(new ApiFactory());
        var settings = host.Factory.Settings;
        settings["Email:Sender"] = "sender@example.com";
        settings["Email:Region"] = "us-east-1";
        settings["WhatsApp:PhoneNumberId"] = "1234567890";
        settings["WhatsApp:AccessToken"] = "test-token";
        if (withFmId)
            FakeFmIdProvider.Configure(settings);
        configure?.Invoke(settings);
        host.Factory.ServiceOverrides.Add(collection =>
        {
            collection.RemoveAll<IEmailSender>();
            collection.AddSingleton<IEmailSender>(host.Emails);
            collection.RemoveAll<IWhatsAppSender>();
            collection.AddSingleton<IWhatsAppSender>(host.WhatsApp);
            host.FmId.Install(collection);
            services?.Invoke(collection);
        });
        return host;
    }

    public HttpClient Browser() =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    public async Task<AccountClient> ClientAsync()
    {
        var client = new AccountClient(Browser());
        await client.RefreshAntiforgeryAsync();
        return client;
    }

    public async Task<AccountClient> SignedInAccountAsync(string email = "visitor@example.com")
    {
        var client = await ClientAsync();
        FmId.Subject = $"subject-of-{email}";
        FmId.Email = email;
        FmId.EmailVerified = true;
        Assert.Equal("/", await SignInWithFmIdAsync(client.Http, "/"));
        await client.RefreshAntiforgeryAsync();
        return client;
    }

    public async Task<string> SignInWithFmIdAsync(HttpClient browser, string returnUrl)
    {
        var challenge = await StartFmIdAsync(browser, returnUrl);
        Assert.StartsWith(FakeFmIdProvider.Issuer, challenge.Headers.Location!.ToString());
        return await FinishFmIdAsync(browser, QueryHelpers.ParseQuery(challenge.Headers.Location!.Query));
    }

    public static Task<HttpResponseMessage> StartFmIdAsync(HttpClient browser, string returnUrl, string? token = null) =>
        PostFormAsync(browser, "/api/account/fmid/sign-in", returnUrl, token);

    public static async Task<HttpResponseMessage> PostFormAsync(HttpClient browser, string path, string returnUrl, string? token = null)
    {
        token ??= (await browser.GetFromJsonAsync<AntiforgeryResponse>("/api/account/antiforgery"))!.Token;
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["returnUrl"] = returnUrl,
            ["uiLocale"] = "de-DE",
        });
        return await browser.PostAsync(path, form);
    }

    public async Task<string> FinishFmIdAsync(HttpClient browser, Dictionary<string, StringValues> authorize)
    {
        FmId.Nonce = authorize["nonce"];
        var callback = await browser.GetAsync($"/signin-fmid?code=test-code&state={Uri.EscapeDataString(authorize["state"]!)}");
        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        var complete = await browser.GetAsync(callback.Headers.Location);
        Assert.Equal(HttpStatusCode.Redirect, complete.StatusCode);
        return complete.Headers.Location!.ToString();
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

    public Task<HttpResponseMessage> PostAsync(string path, object body) => Http.PostAsJsonAsync(path, body);
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
