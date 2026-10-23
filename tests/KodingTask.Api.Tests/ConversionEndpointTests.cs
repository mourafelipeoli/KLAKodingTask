using System.Net;
using System.Net.Http.Json;
using KodingTask.Api.Contracts;
using KodingTask.Conversion;

namespace KodingTask.Api.Tests;

public sealed class ConversionEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Converts_with_a_json_body()
    {
        var response = await _client.PostAsJsonAsync("/api/conversions", new { amount = "45 100", language = "de-DE" });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ConversionResponse>();
        Assert.NotNull(result);
        Assert.Equal("fünfundvierzigtausendeinhundert Dollar", result.Words);
        Assert.Equal("45 100,00", result.NormalizedAmount);
        Assert.Equal(45_100, result.Dollars);
        Assert.Equal(0, result.Cents);
        Assert.Equal("de-DE", result.Language);
        Assert.Equal("Deterministic", result.Engine);
    }

    [Fact]
    public async Task Converts_from_the_query_string()
    {
        var result = await _client.GetFromJsonAsync<ConversionResponse>("/api/conversions?amount=25,1&language=fr-BE");
        Assert.Equal("vingt-cinq dollars et dix cents", result?.Words);
    }

    [Fact]
    public async Task Defaults_to_english()
    {
        var response = await _client.PostAsJsonAsync("/api/conversions", new { amount = "0,01" });
        var result = await response.Content.ReadFromJsonAsync<ConversionResponse>();
        Assert.Equal("zero dollars and one cent", result?.Words);
        Assert.Equal("en-US", result?.Language);
    }

    [Theory]
    [InlineData("25.10", "en-US", "DecimalPointNotAllowed")]
    [InlineData("1 000 000 000", "en-US", "DollarsOutOfRange")]
    [InlineData("", "en-US", "Empty")]
    [InlineData("5", "es-ES", "UnsupportedLanguage")]
    public async Task Refuses_with_an_error_code(string amount, string language, string expectedCode)
    {
        var response = await _client.PostAsJsonAsync("/api/conversions", new { amount, language });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(expectedCode, error?.Error);
        Assert.False(string.IsNullOrWhiteSpace(error?.Message));
    }

    [Fact]
    public async Task Converts_into_every_language_at_once()
    {
        var response = await _client.PostAsJsonAsync("/api/conversions/all-languages", new { amount = "80" });

        response.EnsureSuccessStatusCode();
        var results = await response.Content.ReadFromJsonAsync<List<ConversionResponse>>();
        Assert.NotNull(results);
        Assert.Equal(SpellerCatalog.Default.Languages.Count, results.Count);
        Assert.Equal("huitante dollars", results.Single(result => result.Language == "fr-CH").Words);
        Assert.Equal("quatre-vingts dollars", results.Single(result => result.Language == "fr-BE").Words);
        Assert.Equal("八十美元", results.Single(result => result.Language == "zh-CN").Words);
        Assert.Equal("팔십 달러", results.Single(result => result.Language == "ko-KR").Words);
    }

    [Fact]
    public async Task Lists_the_languages()
    {
        var languages = await _client.GetFromJsonAsync<List<LanguageResponse>>("/api/conversions/languages");
        Assert.Equal(SpellerCatalog.Default.Languages.Count, languages?.Count);
        Assert.Equal("日本語", languages?.Single(language => language.Code == "ja-JP").NativeName);
        Assert.Equal("Français (Belgique)", languages?.Single(language => language.Code == "fr-BE").NativeName);
    }

    [Fact]
    public async Task Answers_unknown_api_paths_with_json_404()
    {
        var response = await _client.GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("NotFound", error?.Error);
    }
}
