import { translationService } from "../services/translationService";

// A text the code shows that no dictionary knows is sent to the server, which stores it as
// a term and has it machine-translated: the next visitor sees it in every language. Texts
// are collected for two seconds and sent in groups, each one once per page load; the server
// ignores the ones it already has and caps how many new ones a visitor can add per day.
const DELAY_MS = 2000;
const GROUP_SIZE = 20;
const MAX_LENGTH = 300;

const reported = new Set<string>();
let waiting: string[] = [];
let timer: number | undefined;

function isAcceptable(text: string): boolean {
  return (
    text.length > 0 &&
    text.length <= MAX_LENGTH &&
    text === text.trim() &&
    ![...text].some((character) => {
      const code = character.charCodeAt(0);
      return code < 32 || (code >= 127 && code < 160);
    })
  );
}

export function reportUnknownText(text: string): void {
  if (reported.has(text) || !isAcceptable(text)) {
    return;
  }
  reported.add(text);
  waiting.push(text);
  if (timer === undefined) {
    timer = window.setTimeout(send, DELAY_MS);
    window.addEventListener("pagehide", send);
  }
}

function send(): void {
  window.clearTimeout(timer);
  timer = undefined;
  window.removeEventListener("pagehide", send);
  const texts = waiting;
  waiting = [];
  for (let start = 0; start < texts.length; start += GROUP_SIZE) {
    translationService.register(texts.slice(start, start + GROUP_SIZE));
  }
}
