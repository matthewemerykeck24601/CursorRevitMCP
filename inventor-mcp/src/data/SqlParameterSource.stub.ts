import type { IParameterSource } from "./IParameterSource.js";
import type { InventorParameterRecord, ParameterSyncRequest } from "../types/contracts.js";

export class SqlParameterSourceStub implements IParameterSource {
  public readonly sourceName = "sql";

  public async pull(_request: ParameterSyncRequest): Promise<InventorParameterRecord[]> {
    throw new Error("SQL parameter source is not implemented yet. Start with Excel source.");
  }

  public async push(_request: ParameterSyncRequest): Promise<{ updated: number }> {
    throw new Error("SQL parameter source is not implemented yet. Start with Excel source.");
  }
}
