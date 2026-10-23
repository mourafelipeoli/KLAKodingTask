import { useTheme } from "../hooks/useTheme";
import { useText } from "../i18n/useText";

export default function ThemeToggle() {
  const { t } = useText();
  const { theme, toggleTheme } = useTheme();
  const label = theme === "dark" ? t("Use light theme") : t("Use dark theme");

  return (
    <button
      type="button"
      className="icon-button"
      onClick={toggleTheme}
      aria-label={label}
      title={label}
    >
      <i
        className={theme === "dark" ? "bi bi-sun" : "bi bi-moon-stars"}
        aria-hidden="true"
      />
    </button>
  );
}
