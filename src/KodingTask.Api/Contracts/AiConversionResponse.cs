namespace KodingTask.Api.Contracts;

public sealed record AiConversionResponse(
    ConversionResponse Deterministic,
    AiAnswerResponse? Ai,
    string? AiError);

public sealed record AiAnswerResponse(string Words, bool Agrees, string Model, int LatencyMs);
