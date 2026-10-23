import { textKey } from "../i18n/textKey";

export const AI_UNAVAILABLE = "AiUnavailable";

// Why the AI engine did not answer. The rules engine always did, so every message says
// where the words on screen come from.
const MESSAGES: Readonly<Record<string, string>> = {
  AiPaused: textKey(
    "The AI engine is paused right now. The words above come from the rules engine.",
  ),
  AiMonthlyBudgetReached: textKey(
    "The AI engine has used its budget for this month. The words above come from the rules engine.",
  ),
  AiDailyLimitReached: textKey(
    "The AI engine has reached today's limit. The words above come from the rules engine.",
  ),
  AiClientLimitReached: textKey(
    "You have reached today's limit of AI conversions. The words above come from the rules engine.",
  ),
  AiBusy: textKey("The AI engine is busy. Try again in a few seconds."),
  AiFailed: textKey(
    "The AI engine could not answer this time. The words above come from the rules engine.",
  ),
};

export function aiErrorKey(code: string): string {
  return MESSAGES[code] ?? MESSAGES.AiFailed ?? code;
}
