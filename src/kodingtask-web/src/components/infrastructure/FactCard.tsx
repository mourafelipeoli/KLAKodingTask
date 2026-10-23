import type { ReactNode } from "react";

interface FactCardProps {
  icon: string;
  title: string;
  children: ReactNode;
}

export default function FactCard({ icon, title, children }: FactCardProps) {
  return (
    <article className="card fact-card">
      <h3 className="fact-card__title">
        <i className={`bi ${icon} fact-card__icon`} aria-hidden="true" />
        {title}
      </h3>
      {children}
    </article>
  );
}
