import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// In development the browser does NOT open this port. It opens the API host at
// https://localhost:7162, which serves the API and forwards everything else to Vite
// (see Program.cs, MapForwarder). That keeps one origin, exactly like production, so
// cookies, the sign-in callback and the content security policy behave the same.
//
// Hot reload has its own path so the websocket is easy to tell apart from application
// routes when it travels through the API host's forwarder.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5179,
    strictPort: true,
    hmr: {
      path: "/__vite_hmr",
    },
  },
  build: {
    outDir: "dist",
    sourcemap: false,
    chunkSizeWarningLimit: 600,
  },
});
