export type JsonObject = Record<string, unknown>;

export interface GatewayEnvelope<T> {
  success: boolean;
  message: string;
  data?: T;
}

export interface InventorParameterRecord {
  name: string;
  units?: string;
  expression?: string;
  value?: string | number | boolean;
  isKey?: boolean;
  comment?: string;
}

export interface RevitFamilyParameterRecord {
  name: string;
  dataType?: string;
  unitType?: string;
  group?: string;
  formula?: string;
  isInstance?: boolean;
  value?: string | number | boolean;
}

export interface RevitFamilyParameterPayload {
  source: "revit-public-mcp" | "revit-2026-community-mcp" | "revit-2027-community-mcp" | "unknown";
  familyName?: string;
  typeName?: string;
  parameters: RevitFamilyParameterRecord[];
}

export interface ParameterSyncRequest {
  documentName?: string;
  assemblyName?: string;
  parameterSetId: string;
  parameters?: InventorParameterRecord[];
}

export interface PublishPrepResult {
  documentName: string;
  validation: {
    hasParameters: boolean;
    hasModelStateHint: boolean;
    hasRunLabel: boolean;
  };
  checklist: string[];
}
