# BedrockModelClient

Calls a Claude model through Amazon Bedrock's **Converse** API (`AWSSDK.BedrockRuntime`).

- **Converse, not InvokeModel**: one request shape for every model on Bedrock, token usage
  and cache counters in the response, and the same IAM action (`bedrock:InvokeModel`).
- **Credentials** come from the default AWS chain: the ECS task role when hosted, a local
  profile (`AWS_PROFILE`) on a developer machine. Nothing is configured in code.
- **Prompt caching**: a cache point follows the system prompt, which is identical for every
  request (rules plus examples in all eight languages; see `AiPromptBuilder`). When the
  prefix is long enough for the model's cache, repeated calls read it at a fraction of the
  input price; when it is not, the cache point is simply ignored.
- **Thinking blocks are skipped**: the model may return a reasoning block before the text;
  only `Text` blocks form the answer.
- **Effort** is sent through `AdditionalModelRequestFields` only when configured (see
  `AiOptions`). Temperature is never sent.
- Retries and the request timeout are configured on the SDK client at registration
  (`AiServiceCollectionExtensions`): two retries for throttling and transient errors.
