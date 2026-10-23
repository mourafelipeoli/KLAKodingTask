// "https://klakodingtask.mourafelipe.com/" -> "klakodingtask.mourafelipe.com", for link text.
export function hostOf(url: string): string {
  try {
    return new URL(url).host;
  } catch {
    return url;
  }
}
