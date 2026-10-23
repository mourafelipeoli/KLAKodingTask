interface AuthorWordmarkProps {
  name: string;
}

// The author's own mark, as on their portfolio: a command-line prompt, the first name in a
// light weight, the surname in a heavy one, and a blinking cursor. "Felipe Moura" becomes
// ">_ felipemoura_". Purely visual: the link around it carries the accessible name.
export default function AuthorWordmark({ name }: AuthorWordmarkProps) {
  const [first = "", ...rest] = name.trim().toLowerCase().split(/\s+/);

  return (
    <span className="author-wordmark" aria-hidden="true">
      <span className="author-wordmark__prompt">&gt;_</span>
      <span className="author-wordmark__name">
        <span className="author-wordmark__light">{first}</span>
        <span className="author-wordmark__strong">{rest.join("")}</span>
        <span className="author-wordmark__cursor">_</span>
      </span>
    </span>
  );
}
