// Formatting for the infrastructure page, always in the UI locale: "1,234" in English,
// "1.234" in German, "1 234" in French; dates and relative times the same way.

export function formatNumber(value: number, locale: string, fractionDigits = 0): string {
  return new Intl.NumberFormat(locale, {
    minimumFractionDigits: 0,
    maximumFractionDigits: fractionDigits,
  }).format(value);
}

export function formatPercent(percent: number, locale: string): string {
  return new Intl.NumberFormat(locale, {
    style: "percent",
    maximumFractionDigits: percent < 10 ? 1 : 0,
  }).format(percent / 100);
}

// Amounts below one cent would round to "$0.00" and look like nothing was spent.
export function formatSpend(usd: number, locale: string): string {
  const format = (value: number) =>
    new Intl.NumberFormat(locale, {
      style: "currency",
      currency: "USD",
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(value);
  return usd > 0 && usd < 0.01 ? `< ${format(0.01)}` : format(usd);
}

export function formatDateTime(iso: string, locale: string): string {
  return new Intl.DateTimeFormat(locale, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(iso));
}

export function formatDate(iso: string, locale: string): string {
  return new Intl.DateTimeFormat(locale, { dateStyle: "long" }).format(new Date(iso));
}

export function formatHour(date: Date, locale: string): string {
  return new Intl.DateTimeFormat(locale, { hour: "numeric", minute: "2-digit" }).format(
    date,
  );
}

// "3 hours ago", "vor 3 Stunden", "il y a 3 heures": the largest unit that fits.
export function formatRelative(iso: string, locale: string, now: Date): string {
  const seconds = (new Date(iso).getTime() - now.getTime()) / 1000;
  const format = new Intl.RelativeTimeFormat(locale, { numeric: "auto" });
  const minutes = seconds / 60;
  if (Math.abs(minutes) < 60) return format.format(Math.round(minutes), "minute");
  const hours = minutes / 60;
  if (Math.abs(hours) < 48) return format.format(Math.round(hours), "hour");
  return format.format(Math.round(hours / 24), "day");
}
