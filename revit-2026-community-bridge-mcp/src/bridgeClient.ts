export type JsonObject = Record<string, unknown>;

const DEFAULT_BASE_URL =
  process.env.REVIT_2026_BRIDGE_URL?.trim() || "http://127.0.0.1:8766";

interface GatewayEnvelope<T> {
  success: boolean;
  message: string;
  data?: T;
}

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

export async function getDocumentMetadata(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_document_metadata", input);
}

export async function getElementMetadata(input: JsonObject): Promise<JsonObject[]> {
  return postJson<JsonObject[]>("/api/get_element_metadata", input);
}

export async function getFamilyParameters(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_family_parameters", input);
}

export async function gatewayHealth(): Promise<JsonObject> {
  const response = await fetch(`${DEFAULT_BASE_URL}/health`, {
    method: "GET",
  });
  const parsed = (await response.json()) as GatewayEnvelope<JsonObject>;
  if (!response.ok || !parsed.success) {
    throw new Error(parsed.message || `Gateway health failed (${response.status})`);
  }

  return parsed.data ?? {};
}

export async function getCurrentViewElements(input: JsonObject): Promise<JsonObject[]> {
  return postJson<JsonObject[]>("/api/get_current_view_elements", input);
}

export async function getAvailableFamilyTypes(input: JsonObject): Promise<JsonObject[]> {
  return postJson<JsonObject[]>("/api/get_available_family_types", input);
}

export async function createPointBasedElement(input: JsonObject): Promise<JsonObject[]> {
  return postJson<JsonObject[]>("/api/create_point_based_element", input);
}

export async function openSelectedFamilyEditor(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/open_selected_family_editor", input);
}

export async function openFamilyEditorByElementId(input: {
  elementId: number;
}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/open_family_editor_by_element_id", input);
}

export async function ensureSharedParameters(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/ensure_shared_parameters", input);
}

export async function bindSharedParameters(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/bind_shared_parameters", input);
}

export async function searchFamilyLibrary(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/search_family_library", input);
}

export async function openFamilyFromLibrary(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/open_family_from_library", input);
}

export async function loadFamilyFromLibrary(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/load_family_from_library", input);
}

export async function upgradeFamilyLibraryVersion(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/upgrade_family_library_version", input);
}

export async function extractFamilyLibraryParameters(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/extract_family_library_parameters", input);
}

export async function addSharedParamsToFamilyLibrary(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/add_shared_parameters_to_family_library", input);
}

export async function extractFamilyDescVariants(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/extract_family_desc_variants", input);
}
