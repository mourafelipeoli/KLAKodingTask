import type { ReactNode } from "react";
import { useText } from "../../i18n/useText";

export interface Fact {
  label: string;
  value: ReactNode | null;
}

interface FactListProps {
  facts: readonly Fact[];
}

// A definition list of label and value pairs. A null value is a fact this instance could
// not read (a local run has no AWS, or one AWS call failed), shown as "Not available".
export default function FactList({ facts }: FactListProps) {
  const { t } = useText();

  return (
    <dl className="fact-list">
      {facts.map((fact) => (
        <div key={fact.label} className="fact-list__row">
          <dt className="fact-list__label">{fact.label}</dt>
          <dd className="fact-list__value">
            {fact.value ?? (
              <span className="fact-list__unknown">{t("Not available")}</span>
            )}
          </dd>
        </div>
      ))}
    </dl>
  );
}
