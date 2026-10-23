# AiPromptBuilder

Builds the prompt for the AI conversion engine.

- **One system prompt for every request**, built once at startup: the task, the output
  format, the grammar rules of each language and worked examples in all eight languages.
  Because it never changes, it is a perfect prompt-cache prefix (see `BedrockModelClient`);
  the per-request part is three short lines.
- **The examples come from the deterministic spellers** (`SpellerCatalog`), not from a
  hand-written list: the six inputs of the task statement in every language, plus 71, 80 and
  91 for the French variants, where the regions differ. The prompt therefore teaches exactly
  the conventions the deterministic engine follows, and the comparison between the two
  engines measures understanding, not a disagreement about style.
- **The user message carries only validated numbers** (dollars and cents as integers and a
  language code from the catalog). Nothing the visitor typed reaches the model, so there is
  no prompt-injection surface.
