using System.Security.Cryptography;
using System.Text;

namespace KodingTask.Api.Translations;

public static class TermText
{
    public static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}
