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

