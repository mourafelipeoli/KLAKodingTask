using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace KodingTask.Api.Accounts;

public static class FmIdServiceCollectionExtensions
{
    public const string ReturnUrlItem = "returnUrl";
    public const string UiLocaleItem = "uiLocale";

    /// <summary>Sign-in with fmID when the provider and the client are configured; returns the settings read.</summary>
    public static FmIdOptions AddFmIdSignIn(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(FmIdOptions.SectionName).Get<FmIdOptions>() ?? new FmIdOptions();
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
            options.SaveTokens = false;
            options.GetClaimsFromUserInfoEndpoint = false;
            options.MapInboundClaims = false;
            options.DisableTelemetry = true;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.TokenValidationParameters.NameClaimType = "name";
            options.TokenValidationParameters.ValidateIssuer = true;
            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    if (context.Properties.Items.TryGetValue(UiLocaleItem, out var uiLocale) && !string.IsNullOrEmpty(uiLocale))
                        context.ProtocolMessage.UiLocales = uiLocale;
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
                OnRemoteFailure = context =>
                {
                    var refused = context.Failure is OpenIdConnectProtocolException failure
                        && failure.Data["error"] as string == "access_denied";
                    context.Response.Redirect(SignInPage(refused ? FmIdErrors.RefusedByProvider : FmIdErrors.InvalidResponse, context.Properties));
                    context.HandleResponse();
                    return Task.CompletedTask;
                },
            };
        });
        return settings;
    }

    private static string SignInPage(string error, AuthenticationProperties? properties) =>
        AccountLinks.SignInPath(error, properties?.Items.TryGetValue(ReturnUrlItem, out var returnUrl) == true ? returnUrl : null);
}
