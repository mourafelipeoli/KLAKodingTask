using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime.Documents;
using Microsoft.Extensions.Options;

namespace KodingTask.Api.Ai;

public sealed class BedrockModelClient(IAmazonBedrockRuntime bedrock, IOptions<AiOptions> options) : IAiModelClient
{
    public async Task<AiModelReply> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var request = new ConverseRequest
        {
            ModelId = settings.ModelId,
            System =
            [
                new SystemContentBlock { Text = systemPrompt },
                new SystemContentBlock { CachePoint = new CachePointBlock { Type = CachePointType.Default } },
            ],
            Messages =
            [
                new Message
                {
                    Role = ConversationRole.User,
                    Content = [new ContentBlock { Text = userPrompt }],
                },
            ],
            InferenceConfig = new InferenceConfiguration { MaxTokens = settings.MaxOutputTokens },
        };

        if (!string.IsNullOrWhiteSpace(settings.Effort))
        {
            request.AdditionalModelRequestFields = new Document(new Dictionary<string, Document>
            {
                ["output_config"] = new Document(new Dictionary<string, Document> { ["effort"] = new Document(settings.Effort) }),
            });
        }

        var response = await bedrock.ConverseAsync(request, cancellationToken);
        var text = string.Concat((response.Output?.Message?.Content ?? []).Where(block => block.Text is not null).Select(block => block.Text));
        var usage = response.Usage;

        return new AiModelReply(
            text,
            response.StopReason?.Value,
            usage?.InputTokens ?? 0,
            usage?.OutputTokens ?? 0,
            usage?.CacheReadInputTokens ?? 0,
            usage?.CacheWriteInputTokens ?? 0);
    }
}
