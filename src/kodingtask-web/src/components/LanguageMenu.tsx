import { matchLocale } from "../config/locales";
import { useText } from "../i18n/useText";

// The languages are the server's: one added to its InterfaceLanguage enum appears here.
export default function LanguageMenu() {
  const { t, locale, languages, setLocale } = useText();

  return (
    <label className="language-menu">
      <i className="bi bi-globe2 language-menu__icon" aria-hidden="true" />
      <span className="visually-hidden">{t("Language")}</span>
      <select
        className="language-menu__select"
        value={locale}
        onChange={(event) => {
          const next = matchLocale(event.target.value, languages);
          if (next) {
            setLocale(next);
          }
        }}
      >
        {languages.map((option) => (
          <option key={option.code} value={option.code} lang={option.code}>
            {option.nativeName}
          </option>
        ))}
      </select>
    </label>
  );
}
