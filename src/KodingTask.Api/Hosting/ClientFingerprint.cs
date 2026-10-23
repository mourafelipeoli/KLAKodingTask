using System.Security.Cryptography;
using System.Text;

namespace KodingTask.Api.Hosting;

public sealed class ClientFingerprint(IConfiguration configuration)
{
    private readonly byte[] _key = KeyFrom(configuration["Privacy:ClientHashKey"]);

    public string Of(HttpContext context) => Of(RequestRateLimits.ClientAddress(context));

    public string Of(string clientAddress) =>
        Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(clientAddress)))[..32].ToLowerInvariant();

    private static byte[] KeyFrom(string? configured) =>
        string.IsNullOrWhiteSpace(configured) ? RandomNumberGenerator.GetBytes(32) : Encoding.UTF8.GetBytes(configured);
}
