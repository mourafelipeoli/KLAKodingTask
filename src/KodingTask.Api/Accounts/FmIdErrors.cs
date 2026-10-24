namespace KodingTask.Api.Accounts;

/// <summary>The codes the sign-in page receives in <c>?error=</c> when a sign-in with fmID does not end in a session.</summary>
public static class FmIdErrors
{
    public const string RefusedByProvider = "refused-by-provider";
    public const string InvalidResponse = "invalid-response";
    public const string NoAccess = "no-access";
    public const string Unavailable = "unavailable";
}
