using System.Security.Claims;
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
        services.AddIdentityCore<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 10;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<KodingTaskDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

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

        // The periodic security-stamp check rebuilds the session from the user record and
        // would forget how the visitor signed in; sign-out with fmID depends on it.
        services.Configure<SecurityStampValidatorOptions>(options => options.OnRefreshingPrincipal = context =>
        {
            var method = context.CurrentPrincipal?.FindFirst(ClaimTypes.AuthenticationMethod);
            if (method is not null && context.NewPrincipal?.Identity is ClaimsIdentity identity
                && !identity.HasClaim(claim => claim.Type == ClaimTypes.AuthenticationMethod))
            {
                identity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, method.Value));
            }

            return Task.CompletedTask;
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
        services.AddScoped<AccountEmails>();
        services.AddScoped<PhoneVerification>();
        return services;
    }
}
