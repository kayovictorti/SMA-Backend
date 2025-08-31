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

