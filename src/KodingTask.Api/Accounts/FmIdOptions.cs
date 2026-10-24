namespace KodingTask.Api.Accounts;

public sealed class FmIdOptions
{
    public const string SectionName = "FmId";
    public const string Scheme = "fmID";
    public const string CallbackPath = "/signin-fmid";
    public const string SignedOutCallbackPath = "/signout-fmid";

    public string Authority { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>The provider's account page, where people add and confirm their WhatsApp number. Optional.</summary>
    public string AccountUrl { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Authority)
        && !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ClientSecret);

    /// <summary>The provider's origin, which the content security policy must allow as a form target.</summary>
    public string? AuthorityOrigin =>
        Uri.TryCreate(Authority, UriKind.Absolute, out var authority) ? authority.GetLeftPart(UriPartial.Authority) : null;
}
