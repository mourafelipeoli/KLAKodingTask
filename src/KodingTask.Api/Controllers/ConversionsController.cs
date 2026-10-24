using KodingTask.Api.Contracts;
using KodingTask.Api.History;
using KodingTask.Api.Hosting;
using KodingTask.Conversion;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/conversions")]
[Produces("application/json")]
public sealed class ConversionsController(
    CurrencyConverter converter,
    UserManager<IdentityUser> users,
    ConversionHistory history) : ControllerBase
{
    public const string DefaultLanguage = "en-US";

    [HttpGet("languages")]
    public IEnumerable<LanguageResponse> Languages() => converter.Languages.Select(LanguageResponse.From);

    [HttpGet]
    [EnableRateLimiting(RequestRateLimits.Conversion)]
    [ProducesResponseType<ConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public ActionResult<ConversionResponse> ConvertFromQuery([FromQuery] string? amount, [FromQuery] string? language) =>
        ConvertOne(amount, language);

    [HttpPost]
    [EnableRateLimiting(RequestRateLimits.Conversion)]
    [ProducesResponseType<ConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConversionResponse>> Convert([FromBody] ConversionRequest request, CancellationToken cancellationToken)
    {
        var response = ConvertOne(request.Amount, request.Language);
        if (response.Value is { } conversion && users.GetUserId(User) is { } userId)
            await history.RecordAsync(userId, conversion, cancellationToken);
        return response;
    }

    [HttpPost("all-languages")]
    [EnableRateLimiting(RequestRateLimits.Conversion)]
    [ProducesResponseType<IReadOnlyList<ConversionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyList<ConversionResponse>> ConvertToAllLanguages([FromBody] AllLanguagesRequest request)
    {
        var results = converter.Languages.Select(language => converter.Convert(request.Amount, language.Code)).ToList();
        if (results.FirstOrDefault(result => !result.IsSuccess) is { Error: { } error })
            return BadRequest(ErrorResponse.For(error));

        return results.Select(result => ConversionResponse.From(request.Amount!, result)).ToList();
    }

    private ActionResult<ConversionResponse> ConvertOne(string? amount, string? language)
    {
        var result = converter.Convert(amount, string.IsNullOrWhiteSpace(language) ? DefaultLanguage : language);
        return result.IsSuccess
            ? ConversionResponse.From(amount!, result)
            : BadRequest(ErrorResponse.For(result.Error.Value));
    }
}
