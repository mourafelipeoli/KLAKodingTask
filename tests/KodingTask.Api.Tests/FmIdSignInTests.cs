using System.Net;
using System.Net.Http.Json;
using KodingTask.Api.Accounts;
using KodingTask.Api.Contracts;
using KodingTask.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KodingTask.Api.Tests;

public sealed class FmIdSignInTests
{
    private const string ReturnUrl = "/?amount=45%20100&language=de-DE&send=email#send";
    private const string EscapedReturnUrl = "%2F%3Famount%3D45%2520100%26language%3Dde-DE%26send%3Demail%23send";

    [Fact]
    public async Task A_new_visitor_gets_an_account_and_returns_where_they_were()
    {
        using var host = MessagingTestHost.Create();
        var browser = host.Browser();

        var challenge = await MessagingTestHost.StartFmIdAsync(browser, ReturnUrl);
        Assert.Equal(HttpStatusCode.Redirect, challenge.StatusCode);
        var authorize = challenge.Headers.Location!;
        Assert.StartsWith(FakeFmIdProvider.Issuer + "connect/authorize?", authorize.ToString());
        var query = QueryHelpers.ParseQuery(authorize.Query);
        Assert.Equal(FakeFmIdProvider.ClientId, query["client_id"]);
        Assert.Equal("code", query["response_type"]);
        Assert.False(query.ContainsKey("response_mode")); // query is the default of the code flow, so it is left out
        Assert.Equal("S256", query["code_challenge_method"]);
        Assert.Equal("openid profile email phone", query["scope"]);
        Assert.Equal("https://localhost/signin-fmid", query["redirect_uri"]);
        Assert.Equal("de-DE", query["ui_locales"]);

        var landing = await host.FinishFmIdAsync(browser, query);

        Assert.Equal(ReturnUrl, landing);
        Assert.Equal(FakeFmIdProvider.ClientSecret, host.FmId.LastTokenRequest["client_secret"]);
        Assert.False(string.IsNullOrEmpty(host.FmId.LastTokenRequest["code_verifier"]));
        var account = await browser.GetFromJsonAsync<AccountResponse>("/api/account/me");
        Assert.True(account!.IsSignedIn);
        Assert.Equal("visitor@example.com", account.Email);

        using var scope = host.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await users.FindByLoginAsync(FmIdOptions.Scheme, host.FmId.Subject);
        Assert.NotNull(user);
        Assert.True(user.EmailConfirmed);
        Assert.False(await users.HasPasswordAsync(user));
        var profile = await scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().UserProfiles.SingleAsync(row => row.UserId == user.Id);
        Assert.Equal("de-DE", profile.PreferredLocale);
    }

    [Fact]
    public async Task The_subject_finds_the_same_account_even_after_the_address_changes_at_the_provider()
    {
        using var host = MessagingTestHost.Create();
        await host.SignInWithFmIdAsync(host.Browser(), ReturnUrl);

        host.FmId.Email = "new-address@example.com";
        var browser = host.Browser();
        await host.SignInWithFmIdAsync(browser, ReturnUrl);

        var account = await browser.GetFromJsonAsync<AccountResponse>("/api/account/me");
        Assert.Equal("visitor@example.com", account!.Email);
        using var scope = host.Factory.Services.CreateScope();
        Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().Users.CountAsync());
    }

    [Fact]
    public async Task A_new_subject_with_a_known_verified_address_joins_that_account()
    {
        using var host = MessagingTestHost.Create();
        await host.SignInWithFmIdAsync(host.Browser(), ReturnUrl);

        host.FmId.Subject = "a-recreated-fmid-account";
        await host.SignInWithFmIdAsync(host.Browser(), ReturnUrl);

        using var scope = host.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = Assert.Single(await scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>().Users.ToListAsync());
        Assert.Equal(2, (await users.GetLoginsAsync(user)).Count);
    }

    [Fact]
    public async Task An_unverified_address_is_refused()
    {
        using var host = MessagingTestHost.Create();
        host.FmId.EmailVerified = false;
        var browser = host.Browser();

        var landing = await host.SignInWithFmIdAsync(browser, ReturnUrl);

        Assert.Equal($"/sign-in?error=no-access&returnUrl={EscapedReturnUrl}", landing);
        Assert.False((await browser.GetFromJsonAsync<AccountResponse>("/api/account/me"))!.IsSignedIn);
    }

    [Fact]
    public async Task A_refusal_by_the_provider_returns_to_the_sign_in_page()
    {
        using var host = MessagingTestHost.Create();
        var browser = host.Browser();
        var query = QueryHelpers.ParseQuery((await MessagingTestHost.StartFmIdAsync(browser, ReturnUrl)).Headers.Location!.Query);

        var callback = await browser.GetAsync($"/signin-fmid?error=access_denied&state={Uri.EscapeDataString(query["state"]!)}");

        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        Assert.Equal($"/sign-in?error=refused-by-provider&returnUrl={EscapedReturnUrl}", callback.Headers.Location!.ToString());
    }

    [Fact]
    public async Task A_callback_that_was_not_started_here_is_an_invalid_response()
    {
        using var host = MessagingTestHost.Create();

        var callback = await host.Browser().GetAsync("/signin-fmid?code=forged&state=forged");

        Assert.Equal(HttpStatusCode.Redirect, callback.StatusCode);
        Assert.Equal("/sign-in?error=invalid-response&returnUrl=%2F", callback.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Starting_requires_the_antiforgery_token()
    {
        using var host = MessagingTestHost.Create();

        var challenge = await MessagingTestHost.StartFmIdAsync(host.Browser(), ReturnUrl, token: "forged");

        Assert.Equal($"/sign-in?error=invalid-response&returnUrl={EscapedReturnUrl}", challenge.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Signing_out_ends_the_provider_session_too()
    {
        using var host = MessagingTestHost.Create();
        var browser = host.Browser();
        await host.SignInWithFmIdAsync(browser, ReturnUrl);

        var signOut = await MessagingTestHost.PostFormAsync(browser, "/api/account/fmid/sign-out", "/infrastructure");

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
    public async Task Signing_out_without_a_session_stays_on_this_site()
    {
        using var host = MessagingTestHost.Create();

        var signOut = await MessagingTestHost.PostFormAsync(host.Browser(), "/api/account/fmid/sign-out", "/infrastructure");

        Assert.Equal("/infrastructure", signOut.Headers.Location!.ToString());
    }

    [Fact]
    public async Task The_provider_is_an_allowed_form_target_when_configured()
    {
        using var host = MessagingTestHost.Create();

        var response = await host.Browser().GetAsync("/api/app-info");

        Assert.Contains("form-action 'self' https://fmid.test;", string.Join(' ', response.Headers.GetValues("Content-Security-Policy")) + ";");
        var info = await response.Content.ReadFromJsonAsync<AppInfoResponse>();
        Assert.True(info!.Features.ExternalLogin);
        Assert.True(info.Features.Accounts);
    }

    [Fact]
    public async Task Without_fmid_settings_there_is_no_sign_in()
    {
        using var host = MessagingTestHost.Create(withFmId: false);
        var browser = host.Browser();

        var info = await browser.GetFromJsonAsync<AppInfoResponse>("/api/app-info");
        var challenge = await MessagingTestHost.StartFmIdAsync(browser, ReturnUrl);

        Assert.False(info!.Features.ExternalLogin);
        Assert.False(info.Features.Accounts);
        Assert.Equal($"/sign-in?error=unavailable&returnUrl={EscapedReturnUrl}", challenge.Headers.Location!.ToString());
    }

    [Fact]
    public async Task The_number_verified_on_fmid_becomes_the_whatsapp_number_and_goes_when_it_goes_there()
    {
        using var host = MessagingTestHost.Create();
        host.FmId.PhoneNumber = "+4917012345678";
        host.FmId.PhoneVerified = true;
        var browser = host.Browser();
        await host.SignInWithFmIdAsync(browser, ReturnUrl);
        Assert.Equal("+49*******5678", (await browser.GetFromJsonAsync<AccountResponse>("/api/account/me"))!.WhatsAppNumber);

        host.FmId.PhoneVerified = false;
        await host.SignInWithFmIdAsync(browser, ReturnUrl);
        Assert.Null((await browser.GetFromJsonAsync<AccountResponse>("/api/account/me"))!.WhatsAppNumber);

        host.FmId.PhoneNumber = null;
        await host.SignInWithFmIdAsync(browser, ReturnUrl);
        using var scope = host.Factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<KodingTaskDbContext>();
        Assert.Empty(await database.VerifiedPhones.ToListAsync());
        Assert.NotNull((await database.UserProfiles.SingleAsync()).PhoneCheckedAtUtc);
    }

    [Fact]
    public async Task Refreshing_asks_fmid_silently_when_signed_in()
    {
        using var host = MessagingTestHost.Create();
        var browser = host.Browser();
        await host.SignInWithFmIdAsync(browser, ReturnUrl);

        var refresh = await MessagingTestHost.PostFormAsync(browser, "/api/account/fmid/refresh", ReturnUrl);

        var authorize = refresh.Headers.Location!;
        Assert.StartsWith(FakeFmIdProvider.Issuer + "connect/authorize?", authorize.ToString());
        Assert.Equal("none", QueryHelpers.ParseQuery(authorize.Query)["prompt"]);
        Assert.Equal(ReturnUrl, await host.FinishFmIdAsync(browser, QueryHelpers.ParseQuery(authorize.Query)));
    }

    [Fact]
    public async Task Refreshing_without_a_session_is_a_normal_sign_in()
    {
        using var host = MessagingTestHost.Create();

        var refresh = await MessagingTestHost.PostFormAsync(host.Browser(), "/api/account/fmid/refresh", ReturnUrl);

        Assert.False(QueryHelpers.ParseQuery(refresh.Headers.Location!.Query).ContainsKey("prompt"));
    }

    [Theory]
    [InlineData("login_required")]
    [InlineData("interaction_required")]
    public async Task A_silent_refresh_fmid_cannot_answer_continues_with_a_normal_sign_in(string error)
    {
        using var host = MessagingTestHost.Create();
        var browser = host.Browser();
        await host.SignInWithFmIdAsync(browser, ReturnUrl);
        var refresh = await MessagingTestHost.PostFormAsync(browser, "/api/account/fmid/refresh", ReturnUrl);
        var state = QueryHelpers.ParseQuery(refresh.Headers.Location!.Query)["state"];

        var callback = await browser.GetAsync($"/signin-fmid?error={error}&state={Uri.EscapeDataString(state!)}");

        Assert.Equal($"/sign-in?returnUrl={EscapedReturnUrl}&with=fmid", callback.Headers.Location!.ToString());
    }

    [Fact]
    public async Task The_account_page_of_the_provider_is_published_only_with_fmid()
    {
        using var host = MessagingTestHost.Create(settings => settings["FmId:AccountUrl"] = "https://fmid.test/account");
        using var without = MessagingTestHost.Create(settings => settings["FmId:AccountUrl"] = "https://fmid.test/account", withFmId: false);

        var info = await host.Browser().GetFromJsonAsync<AppInfoResponse>("/api/app-info");
        var none = await without.Browser().GetFromJsonAsync<AppInfoResponse>("/api/app-info");

        Assert.Equal("https://fmid.test/account", info!.FmIdAccountUrl);
        Assert.Null(none!.FmIdAccountUrl);
    }
}
