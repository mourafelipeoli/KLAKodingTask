import { useEffect, useRef, useState } from "react";

interface CopyButtonProps {
  text: string;
  label: string;
  copiedLabel: string;
  icon: string;
}

const CONFIRMATION_MS = 2000;

export default function CopyButton({ text, label, copiedLabel, icon }: CopyButtonProps) {
  const [isCopied, setIsCopied] = useState(false);
  const timer = useRef<number | undefined>(undefined);

  useEffect(() => () => window.clearTimeout(timer.current), []);

  async function handleCopy() {
    try {
      await navigator.clipboard.writeText(text);
    } catch {
      // Clipboard blocked (insecure context or browser policy): nothing to confirm.
      return;
    }
    setIsCopied(true);
    window.clearTimeout(timer.current);
    timer.current = window.setTimeout(() => setIsCopied(false), CONFIRMATION_MS);
  }

  return (
    <button
      type="button"
      className="button button--ghost button--small"
      onClick={handleCopy}
    >
      <i className={`bi ${isCopied ? "bi-check2" : icon}`} aria-hidden="true" />
      <span aria-live="polite">{isCopied ? copiedLabel : label}</span>
    </button>
  );
}
