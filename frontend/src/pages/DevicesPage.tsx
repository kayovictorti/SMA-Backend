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

