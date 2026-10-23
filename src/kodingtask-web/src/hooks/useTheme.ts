import { useCallback, useState } from "react";

export type Theme = "light" | "dark";

const STORAGE_KEY = "kodingtask.theme";

// public/theme-init.js has already applied the theme before React starts; this hook
// reads that decision instead of computing a second one.
function currentTheme(): Theme {
  return document.documentElement.dataset.theme === "dark" ? "dark" : "light";
}

export function useTheme(): { theme: Theme; toggleTheme: () => void } {
  const [theme, setTheme] = useState<Theme>(currentTheme);

  const toggleTheme = useCallback(() => {
    const next: Theme = currentTheme() === "dark" ? "light" : "dark";
    document.documentElement.dataset.theme = next;
    document.documentElement.style.colorScheme = next;
    try {
      window.localStorage.setItem(STORAGE_KEY, next);
    } catch {
      // Blocked storage: the choice lasts until the tab closes.
    }
    setTheme(next);
  }, []);

  return { theme, toggleTheme };
}
