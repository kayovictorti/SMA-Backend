# =========================
# SMA-Monitoring-Frontend - Setup Automático (PowerShell)
# Executar NA RAIZ do repositório (ex.: ...\SMA-Monitoring\)
# =========================

# Falhar em erros
$ErrorActionPreference = "Stop"

# 0) Variáveis
$FrontendDir = "frontend"
$ApiUrl = "http://localhost:5200"  # ajuste se sua API .NET rodar em outra porta

# 1) Criar app React + TS com Vite (se pasta não existir)
if (!(Test-Path $FrontendDir)) {
  Write-Host ">> Criando projeto Vite React + TS em '$FrontendDir'..."
  $created = $false
  try {
    npm create vite@latest $FrontendDir -- --template react-ts | Out-Null
  } catch {}
  # aguardar criação
  for ($i=0; $i -lt 20 -and -not (Test-Path $FrontendDir); $i++) { Start-Sleep -Milliseconds 250 }
  if (!(Test-Path $FrontendDir)) {
    try {
      npx --yes create-vite@latest $FrontendDir -- --template react-ts | Out-Null
    } catch {}
  }
  for ($i=0; $i -lt 20 -and -not (Test-Path $FrontendDir); $i++) { Start-Sleep -Milliseconds 250 }
  if (!(Test-Path $FrontendDir)) {
    throw "Falha ao criar projeto Vite. Verifique sua conexão e o npm."
  }
} else {
  Write-Host ">> Pasta '$FrontendDir' já existe. Continuando configuração..."
}

# 2) Instalar dependências
Set-Location $FrontendDir
Write-Host ">> Instalando dependências..."
npm i axios @tanstack/react-query react-router-dom zod date-fns | Out-Null

# 3) Tailwind (opcional, já incluso para visual leve)
Write-Host ">> Instalando e inicializando Tailwind..."
npm i -D tailwindcss postcss autoprefixer | Out-Null
npx tailwindcss init -p | Out-Null

# 4) Ajustar tailwind.config.js
@'
export default {
  content: ["./index.html","./src/**/*.{ts,tsx}"],
  theme: { extend: {} },
  plugins: [],
}
'@ | Set-Content -Encoding utf8 tailwind.config.js

# 5) Criar .env com URL da API
@"
VITE_API_URL=$ApiUrl
# (opcional) habilite SSE quando o backend expor stream:
# VITE_EVENTS_SSE_URL=http://localhost:5200/api/events/stream
# (opcional) habilite SignalR quando houver hub:
# VITE_SIGNALR_URL=http://localhost:5200/hubs/events
"@ | Set-Content -Encoding utf8 .env

# 6) Estilos base (src/index.css)
New-Item -ItemType Directory -Force -Path "src" | Out-Null
@'
@tailwind base;
@tailwind components;
@tailwind utilities;

/* utilidades simples */
.table th, .table td { @apply px-3 py-2; }
.btn { @apply px-3 py-2 rounded bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50; }
.btn-outline { @apply px-3 py-2 rounded border border-gray-300 hover:bg-gray-100; }
.input { @apply border border-gray-300 rounded px-3 py-2 w-full; }
.card { @apply bg-white rounded shadow p-4; }
.page { @apply max-w-5xl mx-auto p-4 space-y-4; }
.badge { @apply inline-flex items-center rounded px-2 py-0.5 text-sm; }
.badge-danger { @apply bg-red-100 text-red-700; }
.badge-ok { @apply bg-green-100 text-green-700; }
'@ | Set-Content -Encoding utf8 src/index.css

# 7) Biblioteca HTTP (src/lib/api.ts)
New-Item -ItemType Directory -Force -Path "src/lib" | Out-Null
@'
import axios from "axios";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5200",
  headers: { "Content-Type": "application/json" },
});

export function setAuth(token?: string) {
  if (token) api.defaults.headers.common["Authorization"] = `Bearer ${token}`;
  else delete api.defaults.headers.common["Authorization"];
}
'@ | Set-Content -Encoding utf8 src/lib/api.ts

# 8) Tipos (src/types.ts)
@'
import { z } from "zod";

export const DeviceSchema = z.object({
  id: z.number().int().optional(),
  name: z.string().min(1),
  location: z.string().min(1),
  integrationId: z.string().optional().nullable(),
});
export type Device = z.infer<typeof DeviceSchema>;

export const EventSchema = z.object({
  id: z.number().int(),
  deviceId: z.number().int(),
  timestamp: z.string(), // ISO
  temperature: z.number().nullable().optional(),
  humidity: z.number().nullable().optional(),
  isAlarm: z.boolean().default(false),
  payload: z.any().optional(),
});
export type EventItem = z.infer<typeof EventSchema>;
'@ | Set-Content -Encoding utf8 src/types.ts

# 9) Hooks (src/hooks)
New-Item -ItemType Directory -Force -Path "src/hooks" | Out-Null

# 9.1) useDevices.ts
@'
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../lib/api";
import { Device, DeviceSchema } from "../types";

const key = (x="all") => ["devices", x];

export function useListDevices() {
  return useQuery({
    queryKey: key(),
    queryFn: async (): Promise<Device[]> => {
      const { data } = await api.get("/api/devices");
      return data;
    },
  });
}

export function useCreateDevice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (dto: Omit<Device,"id"|"integrationId">) => {
      DeviceSchema.omit({ id: true, integrationId: true }).parse(dto);
      const { data } = await api.post("/api/devices", dto);
      return data as Device;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: key() }),
  });
}

export function useUpdateDevice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (d: Device) => {
      const { id, ...body } = d;
      if (!id) throw new Error("Id ausente");
      DeviceSchema.partial().parse(d);
      const { data } = await api.put(`/api/devices/${id}`, body);
      return data as Device;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: key() }),
  });
}

export function useDeleteDevice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/api/devices/${id}`);
      return id;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: key() }),
  });
}
'@ | Set-Content -Encoding utf8 src/hooks/useDevices.ts

# 9.2) useEvents.ts
@'
import { useEffect, useMemo, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "../lib/api";
import { EventItem, EventSchema } from "../types";

type Strategy = "polling" | "sse" | "signalr";

export function useEvents(opts?: { strategy?: Strategy; limit?: number; pollMs?: number }) {
  const strategy = opts?.strategy ?? (import.meta.env.VITE_EVENTS_SSE_URL ? "sse" : "polling");
  const limit = opts?.limit ?? 100;
  const pollMs = opts?.pollMs ?? 3000;

  const sseUrl = import.meta.env.VITE_EVENTS_SSE_URL as string | undefined;

  // Polling
  const polling = useQuery({
    queryKey: ["events", "polling", limit],
    queryFn: async (): Promise<EventItem[]> => {
      const { data } = await api.get(`/api/events`, { params: { limit } });
      return (data as any[]).map(x => EventSchema.parse(x));
    },
    refetchInterval: strategy === "polling" ? pollMs : false,
    enabled: strategy === "polling",
  });

  // SSE
  const [sseData, setSseData] = useState<EventItem[]>([]);
  const sseRef = useRef<EventSource | null>(null);

  useEffect(() => {
    if (strategy !== "sse" || !sseUrl) return;
    const es = new EventSource(sseUrl);
    sseRef.current = es;
    es.onmessage = (evt) => {
      try {
        const item = EventSchema.parse(JSON.parse(evt.data));
        setSseData(prev => [item, ...prev].slice(0, limit));
      } catch {}
    };
    es.onerror = () => { /* opcional: logar erro */ };
    return () => { es.close(); sseRef.current = null; };
  }, [strategy, sseUrl, limit]);

  const data = useMemo(() => {
    if (strategy === "sse") return sseData;
    return polling.data ?? [];
  }, [strategy, sseData, polling.data]);

  return {
    data,
    isLoading: strategy === "sse" ? false : polling.isLoading,
    error: strategy === "sse" ? undefined : polling.error,
    strategy,
  };
}
'@ | Set-Content -Encoding utf8 src/hooks/useEvents.ts

# 10) Componentes (src/components)
New-Item -ItemType Directory -Force -Path "src/components" | Out-Null

@'
import { useEffect, useState } from "react";
import { Device } from "../types";

type Props = {
  initial?: Partial<Device>;
  onSubmit: (dto: { name: string; location: string }) => void | Promise<void>;
  onCancel?: () => void;
};

export default function DeviceForm({ initial, onSubmit, onCancel }: Props) {
  const [name, setName] = useState(initial?.name ?? "");
  const [location, setLocation] = useState(initial?.location ?? "");

  useEffect(() => {
    setName(initial?.name ?? "");
    setLocation(initial?.location ?? "");
  }, [initial]);

  return (
    <form className="space-y-3" onSubmit={async (e) => {
      e.preventDefault();
      await onSubmit({ name, location });
    }}>
      <div>
        <label className="block text-sm font-medium mb-1">Nome</label>
        <input className="input" value={name} onChange={e=>setName(e.target.value)} required />
      </div>
      <div>
        <label className="block text-sm font-medium mb-1">Localização</label>
        <input className="input" value={location} onChange={e=>setLocation(e.target.value)} required />
      </div>
      <div className="flex gap-2">
        <button className="btn" type="submit">Salvar</button>
        {onCancel && <button type="button" className="btn-outline" onClick={onCancel}>Cancelar</button>}
      </div>
    </form>
  );
}
'@ | Set-Content -Encoding utf8 src/components/DeviceForm.tsx

# 11) Páginas (src/pages)
New-Item -ItemType Directory -Force -Path "src/pages" | Out-Null

# 11.1) DevicesPage.tsx
@'
import { useMemo, useState } from "react";
import { useListDevices, useCreateDevice, useUpdateDevice, useDeleteDevice } from "../hooks/useDevices";
import DeviceForm from "../components/DeviceForm";
import { Device } from "../types";

export default function DevicesPage() {
  const { data: devices, isLoading } = useListDevices();
  const createM = useCreateDevice();
  const updateM = useUpdateDevice();
  const deleteM = useDeleteDevice();

  const [editing, setEditing] = useState<Device | null>(null);
  const list = useMemo(()=> devices ?? [], [devices]);

  return (
    <div className="page">
      <h1 className="text-2xl font-semibold">Dispositivos</h1>

      <div className="grid md:grid-cols-2 gap-4">
        <div className="card">
          <h2 className="font-semibold mb-2">Cadastrar novo</h2>
          <DeviceForm onSubmit={async (dto) => {
            await createM.mutateAsync(dto);
          }} />
        </div>

        <div className="card">
          <h2 className="font-semibold mb-2">Editar</h2>
          {editing ? (
            <DeviceForm
              initial={editing}
              onSubmit={async (dto) => {
                await updateM.mutateAsync({ ...editing, ...dto } as Device);
                setEditing(null);
              }}
              onCancel={() => setEditing(null)}
            />
          ) : (
            <p className="text-gray-500">Selecione um dispositivo para editar</p>
          )}
        </div>
      </div>

      <div className="card">
        <div className="flex items-center justify-between mb-2">
          <h2 className="font-semibold">Lista</h2>
          {isLoading && <span className="text-sm text-gray-500">Carregando...</span>}
        </div>
        <div className="overflow-x-auto">
          <table className="table w-full">
            <thead className="bg-gray-50">
              <tr>
                <th>ID</th>
                <th>Nome</th>
                <th>Localização</th>
                <th>IntegrationId</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {list.map(d => (
                <tr key={d.id} className="border-b">
                  <td>{d.id}</td>
                  <td>{d.name}</td>
                  <td>{d.location}</td>
                  <td className="text-xs text-gray-500">{d.integrationId ?? "-"}</td>
                  <td className="flex gap-2">
                    <button className="btn-outline" onClick={()=>setEditing(d)}>Editar</button>
                    <button className="btn" onClick={()=>deleteM.mutateAsync(d.id!)}>Excluir</button>
                  </td>
                </tr>
              ))}
              {list.length === 0 && !isLoading && (
                <tr><td colSpan={5} className="text-center text-gray-500 py-6">Nenhum dispositivo</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
'@ | Set-Content -Encoding utf8 src/pages/DevicesPage.tsx

# 11.2) EventsPage.tsx
@'
import { format } from "date-fns";
import ptBR from "date-fns/locale/pt-BR";
import { useEvents } from "../hooks/useEvents";

export default function EventsPage() {
  const { data: events, strategy } = useEvents({ strategy: undefined, limit: 100, pollMs: 3000 });

  return (
    <div className="page">
      <h1 className="text-2xl font-semibold">Eventos {strategy === "polling" ? "(atualização periódica)" : "(tempo real)"}</h1>

      <div className="card">
        <div className="overflow-x-auto">
          <table className="table w-full">
            <thead className="bg-gray-50">
              <tr>
                <th>Data</th>
                <th>DeviceId</th>
                <th>Temp (°C)</th>
                <th>Umid (%)</th>
                <th>Alarme</th>
              </tr>
            </thead>
            <tbody>
              {events.map(ev => {
                const alarm = !!ev.isAlarm;
                return (
                  <tr key={ev.id} className={`border-b ${alarm ? "bg-red-50" : ""}`}>
                    <td>{format(new Date(ev.timestamp), "dd/MM/yyyy HH:mm:ss", { locale: ptBR })}</td>
                    <td>{ev.deviceId}</td>
                    <td>{ev.temperature ?? "-"}</td>
                    <td>{ev.humidity ?? "-"}</td>
                    <td>
                      {alarm
                        ? <span className="badge badge-danger">ALARME</span>
                        : <span className="badge badge-ok">OK</span>}
                    </td>
                  </tr>
                );
              })}
              {events.length === 0 && (
                <tr><td colSpan={5} className="text-center text-gray-500 py-6">Sem eventos</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
      <p className="text-sm text-gray-500">Eventos com <b>isAlarm=true</b> aparecem destacados.</p>
    </div>
  );
}
'@ | Set-Content -Encoding utf8 src/pages/EventsPage.tsx

# 12) App e main
@'
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Link, Route, Routes, Navigate } from "react-router-dom";
import DevicesPage from "./pages/DevicesPage";
import EventsPage from "./pages/EventsPage";

const qc = new QueryClient();

export default function App() {
  return (
    <QueryClientProvider client={qc}>
      <BrowserRouter>
        <div className="bg-gray-100 min-h-screen">
          <nav className="bg-white shadow">
            <div className="max-w-5xl mx-auto px-4 py-3 flex gap-4">
              <Link to="/devices" className="font-semibold">Dispositivos</Link>
              <Link to="/events" className="font-semibold">Eventos</Link>
            </div>
          </nav>
          <Routes>
            <Route path="/" element={<Navigate to="/devices" />} />
            <Route path="/devices" element={<DevicesPage />} />
            <Route path="/events" element={<EventsPage />} />
          </Routes>
        </div>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
'@ | Set-Content -Encoding utf8 src/App.tsx

@'
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./index.css";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
'@ | Set-Content -Encoding utf8 src/main.tsx

# 13) Pequeno README no frontend
@"
# SMA-Monitoring-Frontend

SPA em React + TypeScript (Vite). Integra com o backend (.NET 8).

## Rodar
1. Crie um arquivo .env (já criado pelo script) com:
   VITE_API_URL=$ApiUrl

2. Instale deps e rode:
   npm install
   npm run dev

## Telas
- Dispositivos: CRUD usando @tanstack/react-query contra /api/devices.
- Eventos: polling em GET /api/events?limit=100 (ou SSE se VITE_EVENTS_SSE_URL estiver definido).

## Estrutura
src/
  lib/api.ts
  hooks/useDevices.ts, useEvents.ts
  components/DeviceForm.tsx
  pages/DevicesPage.tsx, EventsPage.tsx
  App.tsx, main.tsx, index.css

"@ | Set-Content -Encoding utf8 README.md

Write-Host "`n======================="
Write-Host "✅ Frontend criado em '$PWD'"
Write-Host "👉 Para iniciar: npm run dev"
Write-Host "👉 Certifique-se que o backend esteja rodando em $ApiUrl"
Write-Host "=======================`n"
