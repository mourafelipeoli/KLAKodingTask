namespace KodingTask.Api.Contracts;

public sealed record ConversionRequest(string? Amount, string? Language);

public sealed record AllLanguagesRequest(string? Amount);
