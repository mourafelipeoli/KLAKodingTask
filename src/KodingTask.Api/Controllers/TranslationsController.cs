using KodingTask.Api.Contracts;
using KodingTask.Api.Hosting;
using KodingTask.Api.Translations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/i18n")]
[Produces("application/json")]
public sealed class TranslationsController(TranslationIndex index, TermRegistry registry, ClientFingerprint fingerprint) : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting(RequestRateLimits.Conversion)]
    public Task<IActionResult> ForVisitor(CancellationToken cancellationToken) =>
        DictionaryAsync(LocaleResolver.Resolve(HttpContext), cancellationToken);

    [HttpGet("languages")]
    public IEnumerable<InterfaceLanguageResponse> Languages() =>
        TranslationLocales.Languages.Select(language => new InterfaceLanguageResponse(language.Code, language.NativeName, language.VariantOf));

    [HttpGet("{locale}")]
    [EnableRateLimiting(RequestRateLimits.Conversion)]
    public async Task<IActionResult> ForLocale(string locale, CancellationToken cancellationToken) =>
        TranslationLocales.Canonical(locale) is { } canonical
            ? await DictionaryAsync(canonical, cancellationToken)
            : NotFound(ApiErrors.UnknownLocale);

    [HttpPost("terms")]
    [EnableRateLimiting(RequestRateLimits.Translations)]
    [ValidateAntiforgeryHeader]
    public async Task<IActionResult> Register([FromBody] TermRegistrationRequest request, CancellationToken cancellationToken)
    {
        var registration = await registry.RegisterAsync(request.Texts, fingerprint.Of(HttpContext), cancellationToken);
        return registration.Error is { } error
            ? StatusCode(ApiErrors.StatusOf(error), error)
            : Accepted(new TermRegistrationResponse(registration.Added));
    }

    private async Task<IActionResult> DictionaryAsync(string locale, CancellationToken cancellationToken)
    {
        var dictionary = await index.GetAsync(locale, cancellationToken);
        var etag = $"\"{dictionary.Version}\"";
        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.ContentLanguage = locale;
        if (Request.Headers.IfNoneMatch.Any(value => value is not null && value.Split(',').Any(tag => tag.Trim() == etag)))
            return StatusCode(StatusCodes.Status304NotModified);
        return Ok(new TranslationDictionaryResponse(locale, dictionary.Version, dictionary.Entries));
    }
}
