using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Accounts;

public sealed class AdminOptions
{
    public const string SectionName = "Admin";
    public const string Policy = "Admin";

    public string[] Emails { get; set; } = [];

    public bool IsAdmin(string? email) =>
        !string.IsNullOrWhiteSpace(email) && Emails.Any(admin => string.Equals(admin.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase));
}

public sealed class AdminRequirement : IAuthorizationRequirement;

public sealed class AdminHandler(IOptions<AdminOptions> options) : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && options.Value.IsAdmin(context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
