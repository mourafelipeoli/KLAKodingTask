using KodingTask.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace KodingTask.Api.Accounts;

public static class AccountServiceCollectionExtensions
{
    public const string SessionCookie = "__Host-kodingtask";
    public const string ExternalCookie = "__Host-kodingtask-external";
    public const string AntiforgeryCookie = "__Host-kodingtask-xsrf";
    public const string AntiforgeryHeader = "X-XSRF-TOKEN";

    public static IServiceCollection AddAccounts(this IServiceCollection services)
    {
        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.AddAuthorization();
        services.AddIdentityCore<IdentityUser>(options => options.User.RequireUniqueEmail = true)
            .AddEntityFrameworkStores<KodingTaskDbContext>()
            .AddSignInManager();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = SessionCookie;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // The short-lived cookie between the redirect back from fmID and the local session.
        services.Configure<CookieAuthenticationOptions>(IdentityConstants.ExternalScheme, options =>
        {
            options.Cookie.Name = ExternalCookie;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        services.AddAntiforgery(options =>
        {
            options.HeaderName = AntiforgeryHeader;
            options.Cookie.Name = AntiforgeryCookie;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        services.AddSingleton<AccountLinks>();
        services.AddScoped<UserProfiles>();
        return services;
    }
}
