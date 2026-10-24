using System.Net;
using System.Text.Json;
using KodingTask.Api.Translations;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Tests;

public sealed class DeepLTranslatorTests
{
    [Fact]
    public async Task Sends_one_request_per_batch_and_keeps_placeholders_and_markup_intact()
    {
        var handler = new ScriptedHandler(_ => Answer(HttpStatusCode.OK, """{"translations":[{"text":"Angemeldet als <x>{{email}}</x>"},{"text":"A &amp; B &lt;i&gt;"}]}"""));
        var translator = Translator(handler, "secret:fx");

        var texts = await translator.TranslateAsync(["Signed in as {{email}}", "A & B <i>"], "de-DE", CancellationToken.None);

        Assert.Equal(["Angemeldet als {{email}}", "A & B <i>"], texts);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api-free.deepl.com/v2/translate", request.Uri);
        Assert.Equal("DeepL-Auth-Key secret:fx", request.Authorization);
        using var body = JsonDocument.Parse(request.Body);
        Assert.Equal("DE", body.RootElement.GetProperty("target_lang").GetString());
        Assert.Equal("EN", body.RootElement.GetProperty("source_lang").GetString());
        Assert.Equal("xml", body.RootElement.GetProperty("tag_handling").GetString());
        Assert.Equal("x", body.RootElement.GetProperty("ignore_tags")[0].GetString());
        Assert.Equal("Signed in as <x>{{email}}</x>", body.RootElement.GetProperty("text")[0].GetString());
        Assert.Equal("A &amp; B &lt;i&gt;", body.RootElement.GetProperty("text")[1].GetString());
    }

    [Fact]
    public async Task Waits_and_tries_again_when_deepl_is_busy()
    {
        var attempts = 0;
        var handler = new ScriptedHandler(_ => ++attempts < 3
            ? Answer(HttpStatusCode.TooManyRequests, "{}", retryAfterSeconds: 0)
            : Answer(HttpStatusCode.OK, """{"translations":[{"text":"Hallo"}]}"""));

        var texts = await Translator(handler, "secret").TranslateAsync(["Hello"], "de-DE", CancellationToken.None);

        Assert.Equal(["Hallo"], texts);
        Assert.Equal(3, attempts);
        Assert.Equal("https://api.deepl.com/v2/translate", handler.Requests[0].Uri);
    }

    [Fact]
    public async Task Stops_when_the_quota_is_used_up()
    {
        var handler = new ScriptedHandler(_ => Answer((HttpStatusCode)456, "{}"));

        var failure = await Assert.ThrowsAsync<MachineTranslationException>(() => Translator(handler, "secret:fx").TranslateAsync(["Hello"], "de-DE", CancellationToken.None));

        Assert.True(failure.QuotaExceeded);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Gives_up_after_the_last_retry()
    {
        var handler = new ScriptedHandler(_ => Answer(HttpStatusCode.ServiceUnavailable, "{}"));

        await Assert.ThrowsAsync<MachineTranslationException>(() => Translator(handler, "secret:fx").TranslateAsync(["Hello"], "de-DE", CancellationToken.None));

        Assert.Equal(5, handler.Requests.Count);
    }

    private static DeepLTranslator Translator(ScriptedHandler handler, string key) =>
        new(new HttpClient(handler), Options.Create(new DeepLOptions { ApiKey = key, RetryDelaysSeconds = [0, 0, 0, 0] }), NullLogger<DeepLTranslator>.Instance);

    private static HttpResponseMessage Answer(HttpStatusCode status, string json, int? retryAfterSeconds = null)
    {
        var response = new HttpResponseMessage(status) { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
        if (retryAfterSeconds is { } seconds)
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(seconds));
        return response;
    }

    private sealed class ScriptedHandler(Func<HttpRequestMessage, HttpResponseMessage> answer) : HttpMessageHandler
    {
        public List<(string Uri, string? Authorization, string Body)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add((request.RequestUri!.ToString(), request.Headers.Authorization?.ToString(), await request.Content!.ReadAsStringAsync(cancellationToken)));
            return answer(request);
        }
    }
}
