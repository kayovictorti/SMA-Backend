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

