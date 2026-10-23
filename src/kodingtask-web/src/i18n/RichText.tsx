import type { ReactNode } from "react";
import { Fragment } from "react";

interface RichTextProps {
  /** An already translated sentence with {{name}} placeholders. */
  text: string;
  /** The element that replaces each placeholder, e.g. a link. */
  slots: Readonly<Record<string, ReactNode>>;
}

// Lets a translated sentence keep its own word order around an element: German and
// Portuguese place a link at a different point of the sentence than English does.
export default function RichText({ text, slots }: RichTextProps) {
  const parts = text.split(/(\{\{\w+\}\})/g);
  return (
    <>
      {parts.map((part, index) => {
        const name = /^\{\{(\w+)\}\}$/.exec(part)?.[1];
        const slot = name === undefined ? undefined : slots[name];
        return <Fragment key={index}>{slot ?? part}</Fragment>;
      })}
    </>
  );
}
