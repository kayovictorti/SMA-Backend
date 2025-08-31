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

