# IAiModelClient

The only boundary between the AI conversion logic and a model provider: one system prompt
and one user message in, text and token counts out.

Keeping it this small lets `AiConversionService` (limits, budget, logging, comparison) be
tested with a fake, without any AWS call, and keeps every Bedrock-specific type inside
`BedrockModelClient`.
