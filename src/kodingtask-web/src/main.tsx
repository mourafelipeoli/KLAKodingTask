import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
// Fonts are bundled with the app, not loaded from a CDN: the page works on a closed
// network and does not report visitors to a third party.
import "@fontsource-variable/open-sans/wght.css";
import "@fontsource/ubuntu/300.css";
import "@fontsource/ubuntu/400.css";
import "@fontsource/ubuntu/500.css";
import "@fontsource/ubuntu/700.css";
import "./index.css";
import App from "./App.tsx";

const rootElement = document.getElementById("root");
if (!rootElement) {
  throw new Error("The #root element is missing from index.html.");
}

createRoot(rootElement).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
