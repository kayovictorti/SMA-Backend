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

