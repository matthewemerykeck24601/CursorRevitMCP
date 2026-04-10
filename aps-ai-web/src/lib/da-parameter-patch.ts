/**
 * Data-driven edits for Design Automation: one Revit implementation applies all
 * `set` key/value pairs per element group — no new C# per parameter name.
 *
 * Workitem JSON may include `parameterPatches` at the root, or under
 * `additional_updates.parameterPatches` (MCP merge).
 *
 * Identity: resolve elements via External ID parameter (same as mark workflow).
 *
 * Optional root field `sharedParameterGuidMap`: only when Revit has more than one
 * parameter with the same definition name on an element — then set
 * `{ "PARAM_NAME": "<revit-shared-guid>" }` to disambiguate (see MarkWorkitemApp).
 */

export type DaParameterPatch = {
  /** Revit "External ID" values (AECDM / Forge externalId), one patch row per group. */
  externalIds: string[];
  /** Instance parameter names → values. Use "" or null to clear text params where allowed. */
  set: Record<string, string | number | boolean | null>;
};

export type DaParameterPatchPayload = {
  version?: number;
  parameterPatches?: DaParameterPatch[];
};

/** Merge patches into workitem arguments (DA bundle reads root or additional_updates). */
export function mergeParameterPatchesIntoWorkitemArgs(
  workitemArguments: Record<string, unknown>,
  patches: DaParameterPatch[],
): Record<string, unknown> {
  if (patches.length === 0) return workitemArguments;
  const existing = workitemArguments.parameterPatches;
  const prior = Array.isArray(existing)
    ? (existing as DaParameterPatch[])
    : [];
  return {
    ...workitemArguments,
    parameterPatches: [...prior, ...patches],
  };
}

/** Build one patch row from cached selection external IDs and a single set map. */
export function parameterPatchForExternalIds(
  externalIds: string[],
  set: Record<string, string | number | boolean | null>,
): DaParameterPatch {
  const ids = externalIds.map((s) => s.trim()).filter(Boolean);
  return { externalIds: ids, set };
}

/**
 * Parse `parameter_patches` from tool/API JSON (e.g. AI-built payload).
 * Ignores invalid rows; only string | number | boolean | null values in `set`.
 */
export function parseDaParameterPatchesFromRequest(raw: unknown): DaParameterPatch[] {
  if (!Array.isArray(raw)) return [];
  const out: DaParameterPatch[] = [];
  for (const row of raw) {
    if (!row || typeof row !== "object") continue;
    const r = row as Record<string, unknown>;
    const ext = r.externalIds;
    const set = r.set;
    if (!Array.isArray(ext) || !set || typeof set !== "object" || Array.isArray(set)) continue;
    const ids = ext.filter((x): x is string => typeof x === "string").map((s) => s.trim()).filter(Boolean);
    const setObj: Record<string, string | number | boolean | null> = {};
    for (const [k, v] of Object.entries(set as Record<string, unknown>)) {
      if (typeof v === "string" || typeof v === "number" || typeof v === "boolean" || v === null) {
        setObj[k] = v;
      }
    }
    if (ids.length > 0 && Object.keys(setObj).length > 0) {
      out.push({ externalIds: ids, set: setObj });
    }
  }
  return out;
}

/** Structured clear/set/toggle rows for the Revit dispatcher (`parameter_updates`). */
export type DaParameterUpdateRow = {
  externalIds: string[];
  paramName: string;
  action: "clear" | "set" | "toggle";
  value?: string | number | boolean | null;
};

export function parseDaParameterUpdatesFromRequest(raw: unknown): DaParameterUpdateRow[] {
  if (!Array.isArray(raw)) return [];
  const out: DaParameterUpdateRow[] = [];
  for (const row of raw) {
    if (!row || typeof row !== "object") continue;
    const r = row as Record<string, unknown>;
    const ext = r.externalIds;
    const paramName = typeof r.paramName === "string" ? r.paramName.trim() : "";
    const actionRaw = typeof r.action === "string" ? r.action.trim().toLowerCase() : "";
    if (!Array.isArray(ext) || !paramName) continue;
    if (actionRaw !== "clear" && actionRaw !== "set" && actionRaw !== "toggle") continue;
    const ids = ext.filter((x): x is string => typeof x === "string").map((s) => s.trim()).filter(Boolean);
    if (ids.length === 0) continue;
    const action = actionRaw as DaParameterUpdateRow["action"];
    const v = r.value;
    const value =
      typeof v === "string" || typeof v === "number" || typeof v === "boolean" || v === null
        ? v
        : undefined;
    out.push({ externalIds: ids, paramName, action, value });
  }
  return out;
}

/** Viewer/chat-friendly: one selection + many param actions (mirrors Revit add-in `cached_selection` + `updates`). */
export type DaCachedSelectionPayload = {
  cache_id?: string;
  externalIds: string[];
  aecElementIds?: string[];
  provenance?: Record<string, unknown>;
};

export type DaModifyUpdateSpec = {
  paramName: string;
  action: "clear" | "set" | "toggle";
  value?: string | number | boolean | null;
};

/**
 * Expands `cached_selection.externalIds` × `updates[]` into `parameter_updates` rows for the DA bundle.
 * The C# dispatcher performs the same expansion if the payload is sent as-is.
 */
/** True when patches or expanded parameter_updates rows exist (same gate as DA skip_analysis validation). */
export function hasExecutableDirectEdits(raw: Record<string, unknown>): boolean {
  const parameterPatches = parseDaParameterPatchesFromRequest(raw.parameter_patches);
  const fromCachedSelection = expandParameterUpdatesFromCachedSelection(
    raw.cached_selection,
    raw.updates,
  );
  const parameterUpdates = [
    ...fromCachedSelection,
    ...parseDaParameterUpdatesFromRequest(raw.parameter_updates),
  ];
  return parameterPatches.length > 0 || parameterUpdates.length > 0;
}

export function expandParameterUpdatesFromCachedSelection(
  cached_selection: unknown,
  updates: unknown,
): DaParameterUpdateRow[] {
  if (!cached_selection || typeof cached_selection !== "object" || Array.isArray(cached_selection)) {
    return [];
  }
  if (!Array.isArray(updates) || updates.length === 0) return [];

  const sel = cached_selection as Record<string, unknown>;
  const extRaw = sel.externalIds;
  if (!Array.isArray(extRaw)) return [];
  const externalIds = extRaw
    .filter((x): x is string => typeof x === "string")
    .map((s) => s.trim())
    .filter(Boolean);
  if (externalIds.length === 0) return [];

  const out: DaParameterUpdateRow[] = [];
  for (const row of updates) {
    if (!row || typeof row !== "object") continue;
    const r = row as Record<string, unknown>;
    const paramName = typeof r.paramName === "string" ? r.paramName.trim() : "";
    const actionRaw = typeof r.action === "string" ? r.action.trim().toLowerCase() : "";
    if (!paramName) continue;
    if (actionRaw !== "clear" && actionRaw !== "set" && actionRaw !== "toggle") continue;
    const action = actionRaw as DaModifyUpdateSpec["action"];
    const v = r.value;
    const value =
      typeof v === "string" || typeof v === "number" || typeof v === "boolean" || v === null
        ? v
        : undefined;
    out.push({ externalIds, paramName, action, value });
  }
  return out;
}
