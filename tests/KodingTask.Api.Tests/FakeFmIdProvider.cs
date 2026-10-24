using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KodingTask.Api.Accounts;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace KodingTask.Api.Tests;

/// <summary>
/// Stands in for fmID: a fixed discovery document and a token endpoint that answers with an
/// id_token signed by a test key. The browser legs (authorize, logout) are only redirects, so
/// the tests read them from the Location header.
/// </summary>
internal sealed class FakeFmIdProvider : HttpMessageHandler
{
    public const string Issuer = "https://fmid.test/";
    public const string ClientId = "kodingtask-test";
    public const string ClientSecret = "test-client-secret";

    private readonly RsaSecurityKey _signingKey = new(RSA.Create(2048)) { KeyId = "test-key" };

    public FakeFmIdProvider()
    {
        Configuration = new OpenIdConnectConfiguration
        {
            Issuer = Issuer,
            AuthorizationEndpoint = Issuer + "connect/authorize",
            TokenEndpoint = Issuer + "connect/token",
            EndSessionEndpoint = Issuer + "connect/logout",
        };
        Configuration.SigningKeys.Add(_signingKey);
    }

    public OpenIdConnectConfiguration Configuration { get; }

    public string Subject { get; set; } = "fmid-user-1";

    public string Email { get; set; } = "visitor@example.com";

    public bool EmailVerified { get; set; } = true;

    /// <summary>E.164, as the provider emits it; null when the person has no number there.</summary>
    public string? PhoneNumber { get; set; }

    public bool PhoneVerified { get; set; }

    /// <summary>Copied from the authorize redirect, as the real provider would echo it.</summary>
    public string? Nonce { get; set; }

    public Dictionary<string, string> LastTokenRequest { get; private set; } = [];

    public static void Configure(Dictionary<string, string> settings)
    {
        settings["FmId:Authority"] = Issuer;
        settings["FmId:ClientId"] = ClientId;
        settings["FmId:ClientSecret"] = ClientSecret;
    }

    public void Install(IServiceCollection services) =>
        services.Configure<OpenIdConnectOptions>(FmIdOptions.Scheme, options =>
        {
            options.Configuration = Configuration;
            options.BackchannelHttpHandler = this;
        });

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Assert.Equal(Configuration.TokenEndpoint, request.RequestUri!.ToString());
        var body = await request.Content!.ReadAsStringAsync(cancellationToken);
        LastTokenRequest = QueryHelpers.ParseQuery(body).ToDictionary(pair => pair.Key, pair => pair.Value.ToString());

        var now = DateTimeOffset.UtcNow;
        var claims = new Dictionary<string, object>
        {
            ["sub"] = Subject,
            ["name"] = "Test Visitor",
            ["email"] = Email,
            ["email_verified"] = EmailVerified,
            ["phone_number_verified"] = PhoneVerified,
            ["auth_time"] = now.ToUnixTimeSeconds(),
            ["nonce"] = Nonce ?? string.Empty,
        };
        if (PhoneNumber is not null)
            claims["phone_number"] = PhoneNumber;
        var idToken = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = ClientId,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = now.AddMinutes(15).UtcDateTime,
            Claims = claims,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256),
        });
        var json = JsonSerializer.Serialize(new { access_token = "opaque", token_type = "Bearer", expires_in = 900, id_token = idToken });
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}
