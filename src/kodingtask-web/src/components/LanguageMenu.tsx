import { LOCALES, matchLocale } from "../config/locales";
import { useText } from "../i18n/useText";

export default function LanguageMenu() {
  const { t, locale, setLocale } = useText();

  return (
    <label className="language-menu">
      <i className="bi bi-globe2 language-menu__icon" aria-hidden="true" />
      <span className="visually-hidden">{t("Language")}</span>
      <select
        className="language-menu__select"
        value={locale}
        onChange={(event) => {
          const next = matchLocale(event.target.value);
          if (next) {
            setLocale(next);
          }
        }}
      >
        {LOCALES.map((option) => (
          <option key={option.code} value={option.code} lang={option.code}>
            {option.nativeName}
          </option>
        ))}
      </select>
    </label>
  );
}
