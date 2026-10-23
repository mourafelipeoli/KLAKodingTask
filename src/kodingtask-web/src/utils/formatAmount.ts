// Shows how the server understood the input, written the way the visitor's locale writes
// money: "$45,100.00" in English, "45.100,00 $" in German, "45 100,00 $US" in French.
export function formatUsd(dollars: number, cents: number, locale: string): string {
  return new Intl.NumberFormat(locale, {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(dollars + cents / 100);
}

export const MAXIMUM_DOLLARS = 999_999_999;
export const MAXIMUM_CENTS = 99;
