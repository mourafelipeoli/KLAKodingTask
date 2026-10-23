# AiOptions

Bound from the `Ai` section. The AI engine is off unless `ModelId` and `Region` are set.

- **`ModelId`** is anything Amazon Bedrock's Converse API accepts: a cross-region inference
  profile id (`us.anthropic.claude-haiku-5-5`) or the ARN of an *application* inference
  profile. The hosted instance uses an application profile created for this app: its usage
  is tagged and billed separately, and the task role may invoke that profile and nothing
  else. The value contains an account id, so it lives in the deployment's environment, not
  in this repository.
- **`Effort`** is empty by default, which leaves the model at its default (with its own
  short thinking). Measured on the hosted model: with `low`, Belgian French came back with
  "quatre-vingt-dix" instead of "nonante"; with the default it was right. Cost per call stays
  around a fiftieth of a cent either way.
- **No temperature or sampling settings**: current Claude models reject them, and an
  amount in words has one right answer anyway.
- **`MaxOutputTokens`** leaves room for the model's thinking, which counts as output.
- **`Pricing`** defaults to the published per-million-token list prices of Claude Haiku 5.5
  and is only used to estimate spend for the monthly budget. Set it to the prices of the
  model and platform you actually use.
- **`MaxConcurrency`** bounds simultaneous model calls per container, so a burst queues
  briefly or is refused instead of fanning out.
