import { useId } from "react";

interface AccountFieldProps {
  label: string;
  type: "email" | "password" | "tel" | "text";
  value: string;
  autoComplete: string;
  hint?: string;
  inputMode?: "numeric" | "tel" | "email";
  onChange: (value: string) => void;
}

export default function AccountField({
  label,
  type,
  value,
  autoComplete,
  hint,
  inputMode,
  onChange,
}: AccountFieldProps) {
  const inputId = useId();
  const hintId = useId();

  return (
    <div className="field">
      <label className="field__label" htmlFor={inputId}>
        {label}
      </label>
      <input
        id={inputId}
        className="text-input"
        type={type}
        value={value}
        autoComplete={autoComplete}
        inputMode={inputMode}
        spellCheck={false}
        aria-describedby={hint ? hintId : undefined}
        onChange={(event) => onChange(event.target.value)}
      />
      {hint && (
        <p id={hintId} className="field__hint">
          {hint}
        </p>
      )}
    </div>
  );
}
