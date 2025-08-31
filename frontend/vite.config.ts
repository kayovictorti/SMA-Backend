import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

const target = "https://localhost:7006"; // alinhado ao Swagger em https://localhost:7006

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/api": {
        target,
        changeOrigin: true,
        secure: false,
      },
      // Opcional: SignalR/WebSockets
      "/hubs": {
        target,
        ws: true,
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
