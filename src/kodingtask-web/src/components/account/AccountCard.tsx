import type { ReactNode } from "react";

interface AccountCardProps {
  title: string;
  lead?: string;
  children: ReactNode;
}

// The frame of every account page: one centred card, as narrow as a form wants to be.
export default function AccountCard({ title, lead, children }: AccountCardProps) {
  return (
    <section className="container account-page">
      <div className="card card--accent account-card">
        <h1 className="account-card__title">{title}</h1>
        {lead && <p className="account-card__lead">{lead}</p>}
        {children}
      </div>
    </section>
  );
}
