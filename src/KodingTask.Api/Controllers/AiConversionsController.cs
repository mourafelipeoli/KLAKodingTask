using KodingTask.Api.Ai;
using KodingTask.Api.Contracts;
using KodingTask.Api.Hosting;
using KodingTask.Conversion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Controllers;

[ApiController]
[Route("api/conversions/ai")]
[Produces("application/json")]
public sealed class AiConversionsController(
    CurrencyConverter converter,
    AiConversionService ai,
    ClientFingerprint fingerprint,
    IOptions<AiOptions> options) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting(RequestRateLimits.Ai)]
    [ProducesResponseType<AiConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AiConversionResponse>> Convert([FromBody] ConversionRequest request, CancellationToken cancellationToken)
    {
        var language = string.IsNullOrWhiteSpace(request.Language) ? ConversionsController.DefaultLanguage : request.Language;
        var result = converter.Convert(request.Amount, language);
        if (!result.IsSuccess)
            return BadRequest(ErrorResponse.For(result.Error.Value));

        var deterministic = ConversionResponse.From(request.Amount!, result);
        var languageInfo = converter.Languages.Single(candidate => candidate.Code == result.LanguageCode);
        var outcome = await ai.ConvertAsync(languageInfo, result.Amount, result.Words, fingerprint.Of(HttpContext), cancellationToken);

        var answer = outcome.Words is null
            ? null
            : new AiAnswerResponse(outcome.Words, outcome.Agrees, options.Value.ModelLabel, outcome.LatencyMs);
        return new AiConversionResponse(deterministic, answer, outcome.ErrorCode);
    }
}
