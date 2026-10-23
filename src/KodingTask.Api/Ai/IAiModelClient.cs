namespace KodingTask.Api.Ai;

public interface IAiModelClient
{
    Task<AiModelReply> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken);
}

public sealed record AiModelReply(
    string Text,
    string? StopReason,
    int InputTokens,
    int OutputTokens,
    int CacheReadTokens,
    int CacheWriteTokens);
