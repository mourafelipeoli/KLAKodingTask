const UNITS: readonly (readonly [Intl.RelativeTimeFormatUnit, number])[] = [
  ["day", 86_400],
  ["hour", 3_600],
  ["minute", 60],
];

// "5 minutes ago", "vor 2 Stunden", "hier": how long before `now` (milliseconds since the
// epoch) the ISO date was, in the visitor's language.
export function formatTimeAgo(isoDate: string, now: number, locale: string): string {
  const seconds = Math.min(0, Math.round((Date.parse(isoDate) - now) / 1000));
  const format = new Intl.RelativeTimeFormat(locale, { numeric: "auto" });
  for (const [unit, size] of UNITS) {
    if (Math.abs(seconds) >= size) {
      return format.format(Math.round(seconds / size), unit);
    }
  }
  return format.format(0, "second");
}
