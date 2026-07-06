import type {
  GatewayEnvelope,
  InventorParameterRecord,
  JsonObject,
  PublishPrepResult,
  RevitFamilyParameterPayload,
} from "./types/contracts.js";

const DEFAULT_BASE_URL = process.env.INVENTOR_BRIDGE_URL?.trim() || "http://127.0.0.1:8776";

async function postJson<T>(route: string, payload: unknown): Promise<T> {
  const response = await fetch(`${DEFAULT_BASE_URL}${route}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload ?? {}),
  });

  const parsed = (await response.json()) as GatewayEnvelope<T>;
  if (!response.ok || !parsed.success) {
    throw new Error(parsed.message || `Gateway request failed (${response.status})`);
  }
  return parsed.data as T;
}

export async function gatewayHealth(): Promise<JsonObject> {
  const response = await fetch(`${DEFAULT_BASE_URL}/health`, { method: "GET" });
  const parsed = (await response.json()) as GatewayEnvelope<JsonObject>;
  if (!response.ok || !parsed.success) {
    throw new Error(parsed.message || `Gateway health failed (${response.status})`);
  }
  return parsed.data ?? {};
}

export async function getActiveDocumentInfo(): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_active_document_info", {});
}

export async function runILogicRule(input: {
  ruleName: string;
  runMode?: "activeDocument" | "allReferenced";
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/run_ilogic_rule", input);
}

export async function showILogicForm(input: { formName: string }): Promise<JsonObject> {
  return postJson<JsonObject>("/api/show_ilogic_form", input);
}

export async function writeILogicRule(input: {
  ruleName: string;
  code: string;
  overwrite?: boolean;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/write_ilogic_rule", input);
}

export async function getParameters(input: {
  includeModelParameters?: boolean;
  includeUserParameters?: boolean;
}): Promise<InventorParameterRecord[]> {
  return postJson<InventorParameterRecord[]>("/api/get_parameters", input);
}

export async function setParameters(input: {
  parameters: InventorParameterRecord[];
  runRuleAfterSet?: string;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/set_parameters", input);
}

export async function replicateParametersFromRevitPayload(input: {
  payload: RevitFamilyParameterPayload;
  runRuleAfterSet?: string;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/replicate_parameters_from_revit_payload", input);
}

export async function prepareInformedDesignPublish(input: {
  runLabel?: string;
}): Promise<PublishPrepResult> {
  return postJson<PublishPrepResult>("/api/prepare_informed_design_publish", input);
}

export async function listDrawingViews(): Promise<JsonObject> {
  return postJson<JsonObject>("/api/list_drawing_views", {});
}

export async function addViewOverallDimensions(input: {
  viewIndex?: number;
  viewName?: string;
  offsetInSheet?: number;
  addWidth?: boolean;
  addHeight?: boolean;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/add_view_overall_dimensions", input);
}

export async function addAllViewsOverallDimensions(input: {
  offsetInSheet?: number;
  addWidth?: boolean;
  addHeight?: boolean;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/add_all_views_overall_dimensions", input);
}

export async function addDiameterDimension(input: {
  viewIndex?: number;
  viewName?: string;
  circleIndex?: number;
  textOffsetX?: number;
  textOffsetY?: number;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/add_diameter_dimension", input);
}

export async function addLinearDimension(input: {
  viewIndex?: number;
  viewName?: string;
  fromX: number;
  fromY: number;
  toX: number;
  toY: number;
  dimensionType?: string;
  fromCircleIndex?: number;
  toCircleIndex?: number;
  textOffsetX?: number;
  textOffsetY?: number;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/add_linear_dimension", input);
}

export async function getDocumentSettings(): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_document_settings", {});
}

export async function setLengthUnits(input: { units: string }): Promise<JsonObject> {
  return postJson<JsonObject>("/api/set_length_units", input);
}

export async function setAngleUnits(input: { units: string }): Promise<JsonObject> {
  return postJson<JsonObject>("/api/set_angle_units", input);
}

export async function setMassUnits(input: { units: string }): Promise<JsonObject> {
  return postJson<JsonObject>("/api/set_mass_units", input);
}

export async function setDimensionPrecision(input: {
  linearPrecision?: number;
  angularPrecision?: number;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/set_dimension_precision", input);
}

export async function setModelingDisplay(input: {
  displayMode?: string;
  showSketches?: boolean;
  showWorkFeatures?: boolean;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/set_modeling_display", input);
}
