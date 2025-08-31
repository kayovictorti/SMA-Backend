import axios from "axios";

const baseURL = import.meta.env.DEV
  ? "" // usa proxy do Vite em dev (chamadas relativas: /api)
  : (import.meta.env.VITE_API_URL ?? "http://localhost:5200");

export const api = axios.create({
  baseURL,
  headers: { "Content-Type": "application/json" },
});

export function setAuth(token?: string) {
  if (token) api.defaults.headers.common["Authorization"] = `Bearer ${token}`;
  else delete api.defaults.headers.common["Authorization"];
}
