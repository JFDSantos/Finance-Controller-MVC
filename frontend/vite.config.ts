/// <reference types="vitest/config" />
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// The API's https dev URL (Finance.API/Properties/launchSettings.json, "https" profile).
// Override with VITE_API_PROXY_TARGET when the API runs elsewhere.
const apiTarget = process.env.VITE_API_PROXY_TARGET ?? "https://localhost:7113";

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: "../Finance.API/wwwroot",
    emptyOutDir: true,
  },
  server: {
    proxy: {
      "/api": {
        target: apiTarget,
        changeOrigin: true,
        secure: false,
      },
    },
  },
  test: {
    environment: "jsdom",
    setupFiles: ["./src/test/setup.ts"],
    restoreMocks: true,
  },
});
