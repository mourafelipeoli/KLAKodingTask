using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace KodingTask.Api.Accounts;

public static class FmIdServiceCollectionExtensions
{
    public const string ReturnUrlItem = "returnUrl";
    public const string UiLocaleItem = "uiLocale";
    public const string PromptItem = "prompt";
    public const string SilentPrompt = "none";

    public const string RetryItem = "retry";

    private static readonly string[] SilentFailures = ["login_required", "interaction_required", "consent_required", "account_selection_required"];

    /// <summary>Sign-in with fmID when the provider and the client are configured; returns the settings read.</summary>
    public static FmIdOptions AddFmIdSignIn(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(FmIdOptions.SectionName);
        var settings = section.Get<FmIdOptions>() ?? new FmIdOptions();
        services.Configure<FmIdOptions>(section);
        services.AddScoped<FmIdAccounts>();
        if (!settings.IsConfigured)
            return settings;

        services.AddAuthentication().AddOpenIdConnect(FmIdOptions.Scheme, FmIdOptions.Scheme, options =>
        {
            options.Authority = settings.Authority;
            options.ClientId = settings.ClientId;
            options.ClientSecret = settings.ClientSecret;
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.CallbackPath = FmIdOptions.CallbackPath;
            options.SignedOutCallbackPath = FmIdOptions.SignedOutCallbackPath;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.ResponseMode = OpenIdConnectResponseMode.Query;
            options.UsePkce = true;
            options.RemoteAuthenticationTimeout = TimeSpan.FromHours(1);
            options.SaveTokens = false;
            options.GetClaimsFromUserInfoEndpoint = false;
            options.MapInboundClaims = false;
            options.DisableTelemetry = true;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("phone");
            options.TokenValidationParameters.NameClaimType = "name";
            options.TokenValidationParameters.ValidateIssuer = true;
            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    if (context.Properties.Items.TryGetValue(UiLocaleItem, out var uiLocale) && !string.IsNullOrEmpty(uiLocale))
                        context.ProtocolMessage.UiLocales = uiLocale;
                    if (context.Properties.Items.TryGetValue(PromptItem, out var prompt) && !string.IsNullOrEmpty(prompt))
                        context.ProtocolMessage.Prompt = prompt;
                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProviderForSignOut = context =>
                {
                    context.ProtocolMessage.ClientId = settings.ClientId;
                    return Task.CompletedTask;
                },
                OnAccessDenied = context =>
                {
                    context.Response.Redirect(SignInPage(FmIdErrors.RefusedByProvider, context.Properties));
                    context.HandleResponse();
                    return Task.CompletedTask;
                },
                OnRemoteFailure = async context =>
                {
                    var error = (context.Failure as OpenIdConnectProtocolException)?.Data["error"] as string;
                    var wasSilent = context.Properties?.Items.TryGetValue(PromptItem, out var prompt) == true && prompt == SilentPrompt;
                    var isOurs = error is null && context.Properties is not null;
                    var (outcome, target) =
                        wasSilent && error is not null && SilentFailures.Contains(error)
                            ? ("normal sign-in", AccountLinks.SignInAndStartPath(ReturnUrlOf(context.Properties)))
                        : isOurs && (await context.HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme)).Succeeded
                            ? ("already signed in", ReturnUrls.Sanitize(ReturnUrlOf(context.Properties)))
                        : isOurs && !context.Properties!.Items.ContainsKey(RetryItem)
                            ? ("restart", AccountLinks.SignInAndStartPath(ReturnUrlOf(context.Properties), isRetry: true))
                            : ("error page", SignInPage(error == "access_denied" ? FmIdErrors.RefusedByProvider : FmIdErrors.InvalidResponse, context.Properties));
                    var headers = context.Request.Headers;
                    context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(FmIdServiceCollectionExtensions)).LogWarning(
                        "[fmID] Callback failed ({Failure}, provider error {Error}); went on with: {Outcome}. Sec-Fetch-Mode {Mode}, Sec-Fetch-Dest {Destination}, Sec-Purpose {Purpose}, Purpose {LegacyPurpose}, User-Agent {UserAgent}.",
                        context.Failure?.Message, error ?? "none", outcome,
                        headers["Sec-Fetch-Mode"].ToString(), headers["Sec-Fetch-Dest"].ToString(), headers["Sec-Purpose"].ToString(), headers["Purpose"].ToString(), headers.UserAgent.ToString());
                    context.Response.Redirect(target);
                    context.HandleResponse();
                },
            };
        });
        return settings;
    }

    private static string SignInPage(string error, AuthenticationProperties? properties) =>
        AccountLinks.SignInPath(error, ReturnUrlOf(properties));

    private static string? ReturnUrlOf(AuthenticationProperties? properties) =>
        properties?.Items.TryGetValue(ReturnUrlItem, out var returnUrl) == true ? returnUrl : null;
}
