using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Messaging;

public sealed class MetaWhatsAppSender(HttpClient http, IOptions<WhatsAppOptions> options) : IWhatsAppSender
{
    private const int TemplateLanguageMissing = 132001;

    public Task<string> SendResultAsync(string phoneE164, string locale, string amountText, string words, CancellationToken cancellationToken) =>
        SendTemplateAsync(phoneE164, options.Value.ResultTemplate, locale,
            [new { type = "body", parameters = new[] { Text(amountText), Text(words) } }],
            cancellationToken);

    private async Task<string> SendTemplateAsync(string phoneE164, string template, string locale, object[] components, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var language = MetaLanguages.For(locale) ?? settings.FallbackLanguage;
        try
        {
            return await PostAsync(phoneE164, template, language, components, cancellationToken);
        }
        catch (WhatsAppException exception) when (exception.Code == TemplateLanguageMissing && language != settings.FallbackLanguage)
        {
            return await PostAsync(phoneE164, template, settings.FallbackLanguage, components, cancellationToken);
        }
    }

    private async Task<string> PostAsync(string phoneE164, string template, string language, object[] components, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{settings.ApiBaseUrl.TrimEnd('/')}/{settings.PhoneNumberId}/messages")
        {
            Content = JsonContent.Create(new
            {
                messaging_product = "whatsapp",
                to = phoneE164.TrimStart('+'),
                type = "template",
                template = new { name = template, language = new { code = language }, components },
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.AccessToken);

        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw WhatsAppException.From(body, (int)response.StatusCode);

        var reply = JsonSerializer.Deserialize<MetaReply>(body);
        return reply?.Messages?.FirstOrDefault()?.Id ?? string.Empty;
    }

    private static object Text(string value) => new { type = "text", text = value };

    private sealed record MetaReply([property: JsonPropertyName("messages")] IReadOnlyList<MetaMessage>? Messages);

    private sealed record MetaMessage([property: JsonPropertyName("id")] string? Id);
}

public sealed class WhatsAppException(int code, int httpStatus)
    : Exception($"WhatsApp refused the message (error {code}, HTTP {httpStatus}).")
{
    public int Code { get; } = code;

    public int HttpStatus { get; } = httpStatus;

    internal static WhatsAppException From(string body, int httpStatus)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("error", out var error) && error.TryGetProperty("code", out var code))
                return new WhatsAppException(code.GetInt32(), httpStatus);
        }
        catch (JsonException)
        {
        }

        return new WhatsAppException(0, httpStatus);
    }
}
