export type JsonObject = Record<string, unknown>;

// The C# gateway listens on 127.0.0.1:14001 (distinct from the 2025 bridge's 14000).
// Override with REVIT_2024_BRIDGE_URL (preferred, matches the 2025 bridge naming) or the
// legacy REVIT_2024_DIAG_URL if the add-in is configured on another port.
const DEFAULT_BASE_URL =
  process.env.REVIT_2024_BRIDGE_URL?.trim() ||
  process.env.REVIT_2024_DIAG_URL?.trim() ||
  "http://127.0.0.1:14001";

interface GatewayEnvelope<T> {
  success: boolean;
  message: string;
  data?: T;
}

// Ceiling on a single gateway round-trip. Must comfortably exceed the longest C# dispatcher
// timeout (query_elements / get_schedules = 180s on large models) so the gateway's own
// timeout trips first with a clean message; this only fires on a genuinely hung request.
const REQUEST_TIMEOUT_MS = 200_000;

async function postJson<T>(route: string, payload: unknown): Promise<T> {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  let response: Response;
  try {
    response = await fetch(`${DEFAULT_BASE_URL}${route}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload ?? {}),
      signal: controller.signal,
    });
  } catch (error) {
    if (controller.signal.aborted) {
      throw new Error(
        `Gateway request to ${route} timed out after ${REQUEST_TIMEOUT_MS / 1000}s (no response from Revit 2024).`,
      );
    }
    throw new Error(
      `Gateway request to ${route} failed: ${error instanceof Error ? error.message : String(error)}. Is Revit 2024 running with the RevitDiag2024Bridge add-in started?`,
    );
  } finally {
    clearTimeout(timer);
  }

  let parsed: GatewayEnvelope<T> | undefined;
  try {
    parsed = (await response.json()) as GatewayEnvelope<T>;
  } catch {
    parsed = undefined;
  }

  if (!response.ok || !parsed || !parsed.success) {
    throw new Error(
      parsed?.message || response.statusText || `Gateway request failed (${response.status})`,
    );
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

// ─── Core read tools ────────────────────────────────────────────────────────
export async function getDocumentMetadata(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_document_metadata", input);
}

export async function getActiveDocumentContext(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_active_document_context", input);
}

export async function getElementById(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_element_by_id", input);
}

export async function getElementMetadata(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_element_metadata", input);
}

export async function getElementTypes(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_element_types", input);
}

export async function getLevels(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_levels", input);
}

export async function getGrids(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_grids", input);
}

export async function queryElements(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/query_elements", input);
}

export async function getCurrentViewElements(input: JsonObject): Promise<JsonObject[]> {
  return postJson<JsonObject[]>("/api/get_current_view_elements", input);
}

// ─── Schedule reading (net-new) ──────────────────────────────────────────────
export async function getSchedules(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_schedules", input);
}

export async function getScheduleData(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/get_schedule_data", input);
}

// ─── Diagnostic tools (net-new) ──────────────────────────────────────────────
export async function getWarnings(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/diag/get_warnings", input);
}

export async function getMissingLinks(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/diag/get_missing_links", input);
}

export async function getJournalTail(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/diag/get_journal_tail", input);
}

export async function getCorruptElements(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/diag/get_corrupt_elements", input);
}
