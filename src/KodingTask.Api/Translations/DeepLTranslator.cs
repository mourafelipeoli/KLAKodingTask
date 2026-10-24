using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Translations;

public sealed class DeepLTranslator(HttpClient http, IOptions<DeepLOptions> options, ILogger<DeepLTranslator> logger) : IMachineTranslator
{
    private const int QuotaExceeded = 456;
    private static readonly TimeSpan LongestWait = TimeSpan.FromSeconds(60);

    public async Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> texts, string locale, CancellationToken cancellationToken)
    {
        if (texts.Count == 0)
            return [];

        var settings = options.Value;
        var target = TranslationLocales.MachineTargets[locale];
        var body = new DeepLRequest([.. texts.Select(Placeholders.Protect)], "EN", target, "xml", ["x"]);
        var delays = settings.RetryDelays;
        for (var attempt = 0; ; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, settings.EffectiveBaseUrl + "/v2/translate")
            {
                Content = JsonContent.Create(body),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("DeepL-Auth-Key", settings.ApiKey);

            using var response = await http.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var answer = await response.Content.ReadFromJsonAsync<DeepLResponse>(cancellationToken);
                var translations = answer?.Translations ?? [];
                if (translations.Length != texts.Count)
                    throw new MachineTranslationException("DeepL answered with a different number of texts.");
                return [.. translations.Select(translation => Placeholders.Restore(translation.Text))];
            }

            var status = (int)response.StatusCode;
            if (status == QuotaExceeded)
                throw new MachineTranslationException("The DeepL character quota is used up.", quotaExceeded: true);
            if ((status != 429 && status < 500) || attempt >= delays.Count)
                throw new MachineTranslationException($"DeepL answered {status}.");

            var wait = RetryAfter(response) ?? delays[attempt];
            if (wait > LongestWait)
                throw new MachineTranslationException($"DeepL asked to wait {wait.TotalSeconds:0} s; trying again on the next run.");
            logger.LogWarning("[DeepL] Answered {Status}; trying again in {Seconds} s.", status, wait.TotalSeconds);
            await Task.Delay(wait, cancellationToken);
        }
    }

    private static TimeSpan? RetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is { } delta)
            return delta;
        if (retryAfter?.Date is not { } date)
            return null;
        var untilDate = date - DateTimeOffset.UtcNow;
        return untilDate > TimeSpan.Zero ? untilDate : TimeSpan.Zero;
    }

    private sealed record DeepLRequest(
        [property: JsonPropertyName("text")] string[] Text,
        [property: JsonPropertyName("source_lang")] string SourceLang,
        [property: JsonPropertyName("target_lang")] string TargetLang,
        [property: JsonPropertyName("tag_handling")] string TagHandling,
        [property: JsonPropertyName("ignore_tags")] string[] IgnoreTags);

    private sealed record DeepLResponse([property: JsonPropertyName("translations")] DeepLTranslation[]? Translations);

    private sealed record DeepLTranslation([property: JsonPropertyName("text")] string Text);
}
