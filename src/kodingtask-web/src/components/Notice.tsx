import type { ReactNode } from "react";

export type NoticeTone = "info" | "success" | "warning" | "danger";

const ICONS: Record<NoticeTone, string> = {
  info: "bi-info-circle",
  success: "bi-check-circle",
  warning: "bi-exclamation-triangle",
  danger: "bi-x-octagon",
};

interface NoticeProps {
  tone?: NoticeTone;
  role?: "status" | "alert";
  children: ReactNode;
}

export default function Notice({ tone = "info", role, children }: NoticeProps) {
  return (
    <div className={`notice notice--${tone}`} role={role}>
      <i className={`bi ${ICONS[tone]} notice__icon`} aria-hidden="true" />
      <div className="notice__body">{children}</div>
    </div>
  );
}
