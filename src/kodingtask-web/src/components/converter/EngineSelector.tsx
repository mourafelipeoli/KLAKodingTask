import { useId } from "react";
import { useText } from "../../i18n/useText";
import { Engine } from "../../types/conversion";

interface EngineSelectorProps {
  engine: Engine;
  onChange: (engine: Engine) => void;
}

export default function EngineSelector({ engine, onChange }: EngineSelectorProps) {
  const { t } = useText();
  const name = useId();
  const options = [
    { value: Engine.Rules, label: t("Rules"), icon: "bi-diagram-3" },
    { value: Engine.Ai, label: t("AI"), icon: "bi-stars" },
  ];

  return (
    <fieldset className="engine-selector">
      <legend className="engine-selector__legend">{t("Conversion engine")}</legend>
      <div className="engine-selector__options">
        {options.map((option) => (
          <label key={option.value} className="engine-selector__option">
            <input
              className="engine-selector__input"
              type="radio"
              name={name}
              value={option.value}
              checked={engine === option.value}
              onChange={() => onChange(option.value)}
            />
            <i className={`bi ${option.icon}`} aria-hidden="true" />
            {option.label}
          </label>
        ))}
      </div>
    </fieldset>
  );
}
