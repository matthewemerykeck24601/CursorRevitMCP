import type { InventorParameterRecord, ParameterSyncRequest } from "../types/contracts.js";

export interface IParameterSource {
  readonly sourceName: string;
  pull(request: ParameterSyncRequest): Promise<InventorParameterRecord[]>;
  push(request: ParameterSyncRequest): Promise<{ updated: number }>;
}
