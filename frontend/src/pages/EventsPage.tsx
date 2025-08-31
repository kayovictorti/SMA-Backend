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

