/**
 * Marks an English sentence as a translation key without translating it yet.
 *
 * Used where the text is chosen in one place and translated in another (error messages
 * picked by code, for instance). It changes nothing at run time; it lets
 * `npm run check:i18n` find every key, including the ones never passed to `t` literally.
 */
export function textKey(key: string): string {
  return key;
}
