export type JsonObject = Record<string, unknown>;

const DEFAULT_BASE_URL =
  process.env.REVIT_2024_BRIDGE_URL?.trim() || "http://127.0.0.1:8764";

interface GatewayEnvelope<T> {
  success: boolean;
  message: string;
  data?: T;
}

// Ceiling on a single gateway round-trip. Must exceed the longest C# dispatcher timeout
// (save/load family = 120s) so we never abort a call the gateway is still legitimately
// running; this only trips on a genuinely hung request.
const REQUEST_TIMEOUT_MS = 180_000;

async function postJson<T>(route: string, payload: unknown, timeoutMs = REQUEST_TIMEOUT_MS): Promise<T> {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);

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
        `Gateway request to ${route} timed out after ${timeoutMs / 1000}s (no response from Revit).`,
      );
    }
    throw new Error(
      `Gateway request to ${route} failed: ${error instanceof Error ? error.message : String(error)}. Is Revit running with the bridge add-in started?`,
    );
  } finally {
    clearTimeout(timer);
  }

  let parsed: GatewayEnvelope<T> | undefined;
  try {
    parsed = (await response.json()) as GatewayEnvelope<T>;
  } catch {
    // Non-JSON body (e.g. an HttpListener error page). Don't let the parse
    // failure mask the real failure — fall through to statusText below.
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
  const response = await fetch(`${DEFAULT_BASE_URL}/health`, {
    method: "GET",
  });
  const parsed = (await response.json()) as GatewayEnvelope<JsonObject>;
  if (!response.ok || !parsed.success) {
    throw new Error(parsed.message || `Gateway health failed (${response.status})`);
  }

  return parsed.data ?? {};
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

// ─── Toolbox expansion ────────────────────────────────────────────────────
// Shared { elementId, success } shape for single-element creation actions.
// Routes are domain-prefixed (/api/family/*, /api/project/*) so colliding leaf
// names across the Family Editor and Project toolsets stay distinct.
export interface ElementCreationResult extends JsonObject {
  elementId: number;
  success: boolean;
}

export async function createExtrusion(input: JsonObject): Promise<ElementCreationResult> {
  return postJson<ElementCreationResult>("/api/family/create_extrusion", input);
}

export async function createReferencePlane(input: JsonObject): Promise<ElementCreationResult> {
  return postJson<ElementCreationResult>("/api/family/create_reference_plane", input);
}

// ─── Group 6: Project — Model Element Creation ──────────────────────────────
// Each returns a tool-specific record (id + descriptive fields); the gateway
// converts mm -> Revit feet and resolves level/type references by id.
export async function createWall(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_wall", input);
}

export async function createLevel(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_level", input);
}

export async function createGrid(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_grid", input);
}

export async function createFloor(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_floor", input);
}

export async function createRoom(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_room", input);
}

export async function createStructuralColumn(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_structural_column", input);
}

export async function createBeam(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_beam", input);
}

// Note: routed to /api/project/create_point_based_element, but exposed as the MCP
// tool `place_point_based_element` to avoid colliding with the existing batch
// `create_point_based_element` tool (/api/create_point_based_element).
export async function placePointBasedElement(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_point_based_element", input);
}

export async function createOpeningByBoundary(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_opening_by_boundary", input);
}

// ─── Group 7: Project — Modify & Edit ───────────────────────────────────────
export async function moveElements(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/move_elements", input);
}

export async function rotateElements(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/rotate_elements", input);
}

export async function mirrorElements(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/mirror_elements", input);
}

export async function copyElementsToLevel(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/copy_elements_to_level", input);
}

export async function setElementParameters(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/set_element_parameters", input);
}

export async function deleteElements(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/delete_elements", input);
}

// ─── Group 8: Project — Views, Sheets & Sheet Audit ─────────────────────────
export async function createFloorPlanView(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_floor_plan_view", input);
}

export async function createReflectedCeilingPlan(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_reflected_ceiling_plan", input);
}

export async function createSectionView(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_section_view", input);
}

export async function create3dView(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_3d_view", input);
}

export async function createSheet(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_sheet", input);
}

export async function placeViewOnSheet(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/place_view_on_sheet", input);
}

export async function setViewCropRegion(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/set_view_crop_region", input);
}

export async function getSheets(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_sheets", input);
}

export async function getSheetContents(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_sheet_contents", input);
}

export async function openSheet(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/open_sheet", input);
}

export async function getViewContents(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_view_contents", input);
}

export async function compareSheetToTemplate(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/compare_sheet_to_template", input);
}

// Shared { success } shape for actions that don't return a new element id.
export interface OperationResult extends JsonObject {
  success: boolean;
}

export async function createBlend(input: JsonObject): Promise<ElementCreationResult> {
  return postJson<ElementCreationResult>("/api/family/create_blend", input);
}

export async function createRevolve(input: JsonObject): Promise<ElementCreationResult> {
  return postJson<ElementCreationResult>("/api/family/create_revolve", input);
}

export async function createSweep(input: JsonObject): Promise<ElementCreationResult> {
  return postJson<ElementCreationResult>("/api/family/create_sweep", input);
}

export async function createSweptBlend(input: JsonObject): Promise<ElementCreationResult> {
  return postJson<ElementCreationResult>("/api/family/create_swept_blend", input);
}

export async function setGeometrySolidVoid(input: JsonObject): Promise<OperationResult> {
  return postJson<OperationResult>("/api/family/set_geometry_solid_void", input);
}

export async function createDimension(input: JsonObject): Promise<ElementCreationResult> {
  return postJson<ElementCreationResult>("/api/family/create_dimension", input);
}

export async function setDimensionLabel(input: JsonObject): Promise<OperationResult> {
  return postJson<OperationResult>("/api/family/set_dimension_label", input);
}

export async function lockConstraint(input: JsonObject): Promise<OperationResult> {
  return postJson<OperationResult>("/api/family/lock_constraint", input);
}

// ─── Family Editor: Parameters & Types ─────────────────────────────────────
export interface AddFamilyParameterResult extends JsonObject {
  parameterId: string;
  name: string;
  success: boolean;
}

export interface SaveFamilyResult extends JsonObject {
  filePath: string;
  success: boolean;
}

export interface LoadFamilyResult extends JsonObject {
  familyId: number;
  success: boolean;
}

export interface FamilyDocumentInfo extends JsonObject {
  familyName: string;
  category: string;
  isConceptual: boolean;
  isFaceBased: boolean;
  hostType: string;
  typeCount: number;
  filePath: string;
}

export async function addFamilyParameter(input: JsonObject): Promise<AddFamilyParameterResult> {
  return postJson<AddFamilyParameterResult>("/api/family/add_parameter", input);
}

export async function setFamilyParameterValue(input: JsonObject): Promise<OperationResult> {
  return postJson<OperationResult>("/api/family/set_parameter_value", input);
}

export async function addFamilyType(input: JsonObject): Promise<OperationResult> {
  return postJson<OperationResult>("/api/family/add_type", input);
}

export async function renameFamilyType(input: JsonObject): Promise<OperationResult> {
  return postJson<OperationResult>("/api/family/rename_type", input);
}

export async function deleteFamilyType(input: JsonObject): Promise<OperationResult> {
  return postJson<OperationResult>("/api/family/delete_type", input);
}

export async function setFormula(input: JsonObject): Promise<OperationResult> {
  return postJson<OperationResult>("/api/family/set_formula", input);
}

// ─── Family Editor: Document Management ─────────────────────────────────────
export async function saveFamily(input: JsonObject): Promise<SaveFamilyResult> {
  return postJson<SaveFamilyResult>("/api/family/save", input);
}

export async function saveFamilyAs(input: JsonObject): Promise<SaveFamilyResult> {
  return postJson<SaveFamilyResult>("/api/family/save_as", input);
}

export async function loadFamilyIntoProject(input: JsonObject): Promise<LoadFamilyResult> {
  return postJson<LoadFamilyResult>("/api/family/load_into_project", input);
}

export async function getFamilyDocumentInfo(input: JsonObject): Promise<FamilyDocumentInfo> {
  return postJson<FamilyDocumentInfo>("/api/family/get_document_info", input);
}

// ─── Group 5: Geometry Read & In-Place Extraction (project context) ─────────
// Read-only. The gateway resolves the active document live, so these also work
// while Revit is inside an Edit-In-Place session.
export async function getActiveDocumentContext(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_active_context", input);
}

export async function getElementGeometry(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_element_geometry", input);
}

export async function getInplaceElements(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_inplace_elements", input);
}

export async function reconstructProfileForExtrusion(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/reconstruct_profile", input);
}

// ─── Group 11: read-only project query tools ────────────────────────────────
export async function getLevels(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_levels", input);
}

export async function getGrids(input: JsonObject = {}): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_grids", input);
}

export async function getElementTypes(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_element_types", input);
}

export async function queryElements(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/query_elements", input);
}

export async function getElementById(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/get_element_by_id", input);
}

// ─── Parameter-binding export (parity verification for the 2024 rebuild) ────
// The C# route already exists in the v20 gateway (shared source). Writes the
// full binding + allSharedParameterElements inventory to disk; only the summary
// returns. Used to GUID-verify the 2024 template against the extracted snapshot.
export async function exportParameterBindings(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/export_parameter_bindings", input);
}

// ─── v20: Instance Import (Revit 2024 rebuild driver) ───────────────────────
// Long-running route. The C# gateway reads instances.json (and sibling datasets)
// directly and writes a report + remap file to disk; only the summary returns.
// Execute mode can run up to ~540s server-side (chunked, resumable), so this
// call gets a 620s client ceiling — well above the default 180s.
const IMPORT_REQUEST_TIMEOUT_MS = 620_000;

export async function importInstances(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/import_instances", input, IMPORT_REQUEST_TIMEOUT_MS);
}

// v20.1: batch-create levels + grids from levels_grids.json (gateway reads the file).
export async function createDatums(input: JsonObject): Promise<JsonObject> {
  return postJson<JsonObject>("/api/project/create_datums", input, 180_000);
}
