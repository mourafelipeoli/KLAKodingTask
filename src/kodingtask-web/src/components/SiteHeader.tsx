import { Link, NavLink } from "react-router-dom";
import logoWhite from "../assets/brand/koding-task-logo-white.svg";
import logo from "../assets/brand/koding-task-logo.svg";
import { useText } from "../i18n/useText";
import { ROUTES } from "../routes/routePaths";
import LanguageMenu from "./LanguageMenu";
import ThemeToggle from "./ThemeToggle";
import "./SiteHeader.css";

export default function SiteHeader() {
  const { t } = useText();

  return (
    <header className="site-header">
      <div className="container site-header__inner">
        <Link
          to={ROUTES.home}
          className="site-header__brand"
          aria-label={t("Koding Task home")}
        >
          <img
            className="site-header__logo site-header__logo--light"
            src={logo}
            alt=""
            width={916}
            height={150}
          />
          <img
            className="site-header__logo site-header__logo--dark"
            src={logoWhite}
            alt=""
            width={916}
            height={150}
          />
        </Link>

        <div className="site-header__tools">
          <NavLink
            to={ROUTES.infrastructure}
            className="site-header__nav-link"
            aria-label={t("Where this runs")}
            title={t("Where this runs")}
          >
            <i className="bi bi-hdd-network" aria-hidden="true" />
            <span className="site-header__nav-text">{t("Where this runs")}</span>
          </NavLink>
          <LanguageMenu />
          <ThemeToggle />
        </div>
      </div>
    </header>
  );
}
