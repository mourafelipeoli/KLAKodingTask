using System.Net;
using System.Net.Http.Json;
using KodingTask.Api.Accounts;
using KodingTask.Api.Contracts;
using KodingTask.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KodingTask.Api.Tests;

public sealed class FmIdSignInTests
{
    private const string ReturnUrl = "/?amount=45%20100&language=de-DE&send=email#send";
    private const string EscapedReturnUrl = "%2F%3Famount%3D45%2520100%26language%3Dde-DE%26send%3Demail%23send";

    [Fact]
    public async Task A_new_visitor_gets_a_confirmed_account_and_returns_where_they_were()
    {
        var provider = new FakeFmIdProvider();
        using var host = Create(provider);
        var browser = Browser(host);

        var challenge = await StartAsync(browser);
        Assert.Equal(HttpStatusCode.Redirect, challenge.StatusCode);
        var authorize = challenge.Headers.Location!;
        Assert.StartsWith(FakeFmIdProvider.Issuer + "connect/authorize?", authorize.ToString());
        var query = QueryHelpers.ParseQuery(authorize.Query);
        Assert.Equal(FakeFmIdProvider.ClientId, query["client_id"]);
        Assert.Equal("code", query["response_type"]);
        Assert.False(query.ContainsKey("response_mode")); // query is the default of the code flow, so it is left out
        Assert.Equal("S256", query["code_challenge_method"]);
        Assert.Equal("openid profile email", query["scope"]);
        Assert.Equal("https://localhost/signin-fmid", query["redirect_uri"]);
        Assert.Equal("de-DE", query["ui_locales"]);

        var landing = await FinishAsync(browser, provider, query);

        Assert.Equal(ReturnUrl, landing);
        Assert.Equal(FakeFmIdProvider.ClientSecret, provider.LastTokenRequest["client_secret"]);
        Assert.False(string.IsNullOrEmpty(provider.LastTokenRequest["code_verifier"]));
        var account = await browser.GetFromJsonAsync<AccountResponse>("/api/account/me");
        Assert.True(account!.IsSignedIn);
        Assert.Equal("visitor@example.com", account.Email);
        Assert.True(account.EmailConfirmed);
        Assert.True(account.SignedInWithFmId);

        using var scope = host.Factory.Services.CreateScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>().FindByLoginAsync(FmIdOptions.Scheme, provider.Subject);
        Assert.NotNull(user);
        Assert.False(await scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>().HasPasswordAsync(user));
        var profile = await scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().UserProfiles.SingleAsync(row => row.UserId == user.Id);
        Assert.Equal("de-DE", profile.PreferredLocale);
    }

    [Fact]
    public async Task The_subject_finds_the_same_account_even_after_the_address_changes_at_the_provider()
    {
        var provider = new FakeFmIdProvider();
        using var host = Create(provider);
        await SignInAsync(Browser(host), provider);

        provider.Email = "new-address@example.com";
        var browser = Browser(host);
        await SignInAsync(browser, provider);

        var account = await browser.GetFromJsonAsync<AccountResponse>("/api/account/me");
        Assert.Equal("visitor@example.com", account!.Email);
        using var scope = host.Factory.Services.CreateScope();
        Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().Users.CountAsync());
    }

    [Fact]
    public async Task A_confirmed_local_account_with_the_same_address_is_linked_and_keeps_its_password()
    {
        var provider = new FakeFmIdProvider();
        using var host = Create(provider);
        var local = await host.ConfirmedAccountAsync("visitor@example.com", "a long enough password");

        await SignInAsync(Browser(host), provider);

        using var scope = host.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var linked = await users.FindByLoginAsync(FmIdOptions.Scheme, provider.Subject);
        Assert.Equal((await users.FindByEmailAsync("visitor@example.com"))!.Id, linked!.Id);
        var password = await local.PostAsync("/api/account/sign-in", new { email = "visitor@example.com", password = "a long enough password" });
        Assert.Equal(HttpStatusCode.OK, password.StatusCode);
    }

    [Fact]
    public async Task An_unconfirmed_local_account_with_the_same_address_is_taken_over()
    {
        var provider = new FakeFmIdProvider();
        using var host = Create(provider);
        var squatter = await host.ClientAsync();
        (await squatter.PostAsync("/api/account/register", new { email = "visitor@example.com", password = "the squatter's password", uiLocale = "en-US" })).EnsureSuccessStatusCode();

        await SignInAsync(Browser(host), provider);

        var retry = await (await host.ClientAsync()).PostAsync("/api/account/sign-in", new { email = "visitor@example.com", password = "the squatter's password" });
        Assert.Equal(HttpStatusCode.BadRequest, retry.StatusCode);
        using var scope = host.Factory.Services.CreateScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>().FindByEmailAsync("visitor@example.com");
        Assert.True(user!.EmailConfirmed);
    }

    [Fact]
    public async Task An_unverified_address_is_refused()
    {
        var provider = new FakeFmIdProvider { EmailVerified = false };
        using var host = Create(provider);
        var browser = Browser(host);

        var landing = await SignInAsync(browser, provider);

        Assert.Equal($"/sign-in?error=no-access&returnUrl={EscapedReturnUrl}", landing);
        Assert.False((await browser.GetFromJsonAsync<AccountResponse>("/api/account/me"))!.IsSignedIn);
    }

    [Fact]
    public async Task A_refusal_by_the_provider_returns_to_the_sign_in_page()
    {
        var provider = new FakeFmIdProvider();
        using var host = Create(provider);
        var browser = Browser(host);
        var query = QueryHelpers.ParseQuery((await StartAsync(browser)).Headers.Location!.Query);

        var callback = await browser.GetAsync($"/signin-fmid?error=access_denied&state={Uri.EscapeDataString(query["state"]!)}");

        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        Assert.Equal($"/sign-in?error=refused-by-provider&returnUrl={EscapedReturnUrl}", callback.Headers.Location!.ToString());
    }

    [Fact]
    public async Task A_callback_that_was_not_started_here_is_an_invalid_response()
    {
        using var host = Create(new FakeFmIdProvider());

        var callback = await Browser(host).GetAsync("/signin-fmid?code=forged&state=forged");

        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        Assert.Equal("/sign-in?error=invalid-response&returnUrl=%2F", callback.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Starting_requires_the_antiforgery_token()
    {
        using var host = Create(new FakeFmIdProvider());

        var challenge = await StartAsync(Browser(host), token: "forged");

        Assert.Equal($"/sign-in?error=invalid-response&returnUrl={EscapedReturnUrl}", challenge.Headers.Location!.ToString());
    }

    [Fact]
    public async Task The_registration_cap_applies_to_new_accounts_from_fmid()
    {
        var provider = new FakeFmIdProvider();
        using var host = Create(provider, settings => settings["Limits:RegistrationsPerClientPerDay"] = "0");

        var landing = await SignInAsync(Browser(host), provider);

        Assert.Equal($"/sign-in?error=registration-limit&returnUrl={EscapedReturnUrl}", landing);
    }

    [Fact]
    public async Task Signing_out_of_an_fmid_session_ends_the_provider_session_too()
    {
        var provider = new FakeFmIdProvider();
        using var host = Create(provider);
        var browser = Browser(host);
        await SignInAsync(browser, provider);

        var signOut = await PostFormAsync(browser, "/api/account/fmid/sign-out", "/infrastructure");

        Assert.Equal(HttpStatusCode.Redirect, signOut.StatusCode);
        var logout = signOut.Headers.Location!;
        Assert.StartsWith(FakeFmIdProvider.Issuer + "connect/logout?", logout.ToString());
        var query = QueryHelpers.ParseQuery(logout.Query);
        Assert.Equal("https://localhost/signout-fmid", query["post_logout_redirect_uri"]);
        Assert.Equal(FakeFmIdProvider.ClientId, query["client_id"]);
        Assert.False((await browser.GetFromJsonAsync<AccountResponse>("/api/account/me"))!.IsSignedIn);

        var back = await browser.GetAsync($"/signout-fmid?state={Uri.EscapeDataString(query["state"]!)}");
        Assert.Equal("/infrastructure", back.Headers.Location!.ToString());
    }

    [Fact]
    public async Task A_password_session_signs_out_locally_only()
    {
        var provider = new FakeFmIdProvider();
        using var host = Create(provider);
        await host.ConfirmedAccountAsync("visitor@example.com", "a long enough password");
        var browser = Browser(host);
        var token = (await browser.GetFromJsonAsync<AntiforgeryResponse>("/api/account/antiforgery"))!.Token;
        using (var signIn = new HttpRequestMessage(HttpMethod.Post, "/api/account/sign-in"))
        {
            signIn.Headers.Add("X-XSRF-TOKEN", token);
            signIn.Content = JsonContent.Create(new { email = "visitor@example.com", password = "a long enough password" });
            (await browser.SendAsync(signIn)).EnsureSuccessStatusCode();
        }

        Assert.False((await browser.GetFromJsonAsync<AccountResponse>("/api/account/me"))!.SignedInWithFmId);

        var signOut = await PostFormAsync(browser, "/api/account/fmid/sign-out", "/infrastructure");

        Assert.Equal("/infrastructure", signOut.Headers.Location!.ToString());
        Assert.False((await browser.GetFromJsonAsync<AccountResponse>("/api/account/me"))!.IsSignedIn);
    }

    [Fact]
    public async Task The_provider_is_an_allowed_form_target_when_configured()
    {
        using var host = Create(new FakeFmIdProvider());

        var response = await Browser(host).GetAsync("/api/app-info");

        Assert.Contains("form-action 'self' https://fmid.test;", string.Join(' ', response.Headers.GetValues("Content-Security-Policy")) + ";");
        var info = await response.Content.ReadFromJsonAsync<AppInfoResponse>();
        Assert.True(info!.Features.ExternalLogin);
    }

    [Fact]
    public async Task Without_fmid_settings_the_feature_is_off_and_the_form_says_so()
    {
        using var host = MessagingTestHost.Create();
        var browser = Browser(host);

        var info = await browser.GetFromJsonAsync<AppInfoResponse>("/api/app-info");
        var challenge = await StartAsync(browser);

        Assert.False(info!.Features.ExternalLogin);
        Assert.Equal($"/sign-in?error=unavailable&returnUrl={EscapedReturnUrl}", challenge.Headers.Location!.ToString());
    }

    private static MessagingTestHost Create(FakeFmIdProvider provider, Action<Dictionary<string, string>>? configure = null) =>
        MessagingTestHost.Create(
            settings =>
            {
                FakeFmIdProvider.Configure(settings);
                configure?.Invoke(settings);
            },
            provider.Install);

    // A browser: keeps cookies and shows every redirect instead of following it.
    private static HttpClient Browser(MessagingTestHost host) =>
        host.Factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    private static async Task<HttpResponseMessage> StartAsync(HttpClient browser, string? token = null) =>
        await PostFormAsync(browser, "/api/account/fmid/sign-in", ReturnUrl, token);

    private static async Task<HttpResponseMessage> PostFormAsync(HttpClient browser, string path, string returnUrl, string? token = null)
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

    private static async Task<string> SignInAsync(HttpClient browser, FakeFmIdProvider provider)
    {
        var challenge = await StartAsync(browser);
        return await FinishAsync(browser, provider, QueryHelpers.ParseQuery(challenge.Headers.Location!.Query));
    }

    // The provider's half: it redirects back with a code and the state it received.
    private static async Task<string> FinishAsync(HttpClient browser, FakeFmIdProvider provider, Dictionary<string, Microsoft.Extensions.Primitives.StringValues> authorize)
    {
        provider.Nonce = authorize["nonce"];
        var callback = await browser.GetAsync($"/signin-fmid?code=test-code&state={Uri.EscapeDataString(authorize["state"]!)}");
        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        var complete = await browser.GetAsync(callback.Headers.Location);
        Assert.Equal(HttpStatusCode.Redirect, complete.StatusCode);
        return complete.Headers.Location!.ToString();
    }
}
