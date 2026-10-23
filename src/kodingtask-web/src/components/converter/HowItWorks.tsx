import { Link } from "react-router-dom";
import { useText } from "../../i18n/useText";
import { textKey } from "../../i18n/textKey";
import { ROUTES } from "../../routes/routePaths";

const STEPS = [
  {
    icon: "bi-shield-check",
    title: textKey("Validated on the server"),
    body: textKey(
      "The server reads the amount exactly as the task defines it: spaces between thousands and a comma before the cents. Anything else gets a precise explanation.",
    ),
  },
  {
    icon: "bi-spellcheck",
    title: textKey("Spelled by grammar rules"),
    body: textKey(
      "Each language has its own speller: German compound words, Portuguese conjunctions, French plural rules, and septante and nonante in Belgium and Switzerland.",
    ),
  },
  {
    icon: "bi-check2-all",
    title: textKey("Checked by tests"),
    body: textKey(
      "The six examples of the task are tested in every language, together with the grammar rules of each one and thousands of generated amounts.",
    ),
  },
] as const;

export default function HowItWorks() {
  const { t } = useText();

  return (
    <section className="container how-it-works">
      <h2 className="how-it-works__title">{t("How it works")}</h2>
      <div className="how-it-works__grid">
        {STEPS.map((step) => (
          <article key={step.title} className="card how-it-works__step">
            <i className={`bi ${step.icon} how-it-works__icon`} aria-hidden="true" />
            <h3 className="how-it-works__step-title">{t(step.title)}</h3>
            <p className="how-it-works__step-body">{t(step.body)}</p>
          </article>
        ))}
      </div>
      <Link className="link-cta how-it-works__more" to={ROUTES.infrastructure}>
        {t("See where this runs")}
      </Link>
    </section>
  );
}
