import { useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useListDevices, useCreateDevice, useUpdateDevice, useDeleteDevice } from "../hooks/useDevices";
import DeviceForm from "../components/DeviceForm";
import { Device } from "../types";
import Swal from "sweetalert2";
import type { AxiosError } from "axios";

function extractErrorMessage(err: any): string {
  const ax = err as AxiosError<any>;
  const data: any = ax?.response?.data;
  if (data) {
    if (typeof data === "string") return data;
    if (data.title && data.detail) return `${data.title}: ${data.detail}`;
    if (data.title) return data.title;
    if (data.message) return data.message;
    if (data.errors && typeof data.errors === "object") {
      const messages = Object.values<any>(data.errors).flat().join("; ");
      if (messages) return messages;
    }
  }
  return ax?.message || "Ocorreu um erro. Tente novamente.";
}

function isIotConnectivityError(err: any): boolean {
  const msg = (extractErrorMessage(err) || "").toLowerCase();
  return (
    msg.includes("localhost:5000") ||
    msg.includes("socketexception") ||
    msg.includes("httprequestexception") ||
    msg.includes("iot")
  );
}

export default function DevicesPage() {
  const { data: devices, isLoading } = useListDevices();
  const createM = useCreateDevice();
  const updateM = useUpdateDevice();
  const deleteM = useDeleteDevice();
  const qc = useQueryClient();

  const [editing, setEditing] = useState<Device | null>(null);
  const list = useMemo(()=> devices ?? [], [devices]);

  return (
    <div className="page">
      <h1 className="text-2xl font-semibold">Dispositivos</h1>

      <div className="grid md:grid-cols-2 gap-4">
        <div className="card">
          <h2 className="font-semibold mb-2">Cadastrar novo</h2>
          <DeviceForm onSubmit={async (dto) => {
            await createM.mutateAsync(dto, {
              onSuccess: async (created) => {
                const integrationId = (created as any)?.integrationId as string | null | undefined;
                if (!integrationId) {
                  await Swal.fire({
                    title: "Salvo (sem integração)",
                    text: "Dispositivo salvo, mas não houve comunicação com a API externa (IoT).",
                    icon: "info",
                    confirmButtonText: "OK",
                  });
                } else {
                  await Swal.fire({
                    title: "Sucesso",
                    text: "Dispositivo cadastrado com sucesso.",
                    icon: "success",
                    confirmButtonText: "OK",
                  });
                }
              },
              onError: async (err) => {
                if (isIotConnectivityError(err)) {
                  await Swal.fire({
                    title: "Salvo (sem integração)",
                    text: "Dispositivo salvo, mas não houve comunicação com a API externa (IoT).",
                    icon: "info",
                    confirmButtonText: "OK",
                  });
                  await qc.invalidateQueries({ queryKey: ["devices"] });
                } else {
                  await Swal.fire({
                    title: "Erro ao cadastrar",
                    text: extractErrorMessage(err),
                    icon: "error",
                    confirmButtonText: "OK",
                  });
                }
              },
            });
          }} />
        </div>

        <div className="card">
          <h2 className="font-semibold mb-2">Editar</h2>
          {editing ? (
            <DeviceForm
              initial={editing}
              onSubmit={async (dto) => {
                await updateM.mutateAsync({ ...editing, ...dto } as Device, {
                  onSuccess: async () => {
                    setEditing(null);
                    await Swal.fire({
                      title: "Atualizado",
                      text: "Dispositivo atualizado com sucesso.",
                      icon: "success",
                      confirmButtonText: "OK",
                    });
                  },
                  onError: async (err) => {
                    await Swal.fire({
                      title: "Erro ao atualizar",
                      text: extractErrorMessage(err),
                      icon: "error",
                      confirmButtonText: "OK",
                    });
                  },
                });
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
                <th className="text-center">ID</th>
                <th className="text-center">Nome</th>
                <th className="text-center">Localização</th>
                <th className="text-center">IntegrationId</th>
                <th className="text-center w-32">Ação</th>
              </tr>
            </thead>
            <tbody>
              {list.map(d => (
                <tr key={d.id} className="border-b text-center">
                  <td>{d.id}</td>
                  <td>{d.name}</td>
                  <td>{d.location}</td>
                  <td className="text-xs text-gray-500">{d.integrationId ?? "-"}</td>
                  <td className="text-center">
                    <div className="flex justify-center gap-2">
                      <button className="btn-outline" onClick={()=>setEditing(d)}>Editar</button>
                      <button className="btn" onClick={async ()=>{
                      const res = await Swal.fire({
                        title: "Excluir dispositivo?",
                        text: `ID ${d.id} - ${d.name}`,
                        icon: "warning",
                        showCancelButton: true,
                        confirmButtonText: "Sim, excluir",
                        cancelButtonText: "Cancelar",
                      });
                      if (res.isConfirmed) {
                        await deleteM.mutateAsync(d.id!, {
                          onSuccess: async () => {
                            await Swal.fire({
                              title: "Excluído",
                              text: "Dispositivo removido com sucesso.",
                              icon: "success",
                              confirmButtonText: "OK",
                            });
                          },
                          onError: async (err) => {
                            await Swal.fire({
                              title: "Erro ao excluir",
                              text: extractErrorMessage(err),
                              icon: "error",
                              confirmButtonText: "OK",
                            });
                          },
                        });
                      }
                    }}>Excluir</button>
                    </div>
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

