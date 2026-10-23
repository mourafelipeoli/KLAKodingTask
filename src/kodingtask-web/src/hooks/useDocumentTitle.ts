import { useEffect } from "react";

export function useDocumentTitle(title: string): void {
  useEffect(() => {
    document.title = `Koding Task · ${title}`;
  }, [title]);
}
