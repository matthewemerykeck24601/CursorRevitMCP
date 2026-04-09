/**
 * Revit shared-parameters file (*.txt) → name/GUID catalog for MCP and DA payloads.
 *
 * Default file: config/Shared_Params_2015_v01.txt (shipped with MCP).
 * Override path: process.env.SHARED_PARAMETERS_FILE_PATH
 *
 * Future: primary loader can be Shared Parameters Service. MCP tool
 * `shared_parameters_lookup` may call the Autodesk Parameters API when the file
 * has no match (see parametersApiClient.ts). DA workitems use parameter *names* in
 * `set` / marks; include `sharedParameterGuidMap` only when Revit exposes duplicate
 * definition names on an element (see Revit bundle ResolveParameter).
 */

import { existsSync, readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

export type SharedParamRow = {
  guid: string;
  name: string;
  datatype: string;
  dataCategory: string;
  groupId: number;
  groupName?: string;
  visible: boolean;
  description: string;
  userModifiable: boolean;
  hideWhenNoValue: boolean;
};

type CatalogIndexes = {
  byNameLower: Map<string, SharedParamRow>;
  byGuidLower: Map<string, SharedParamRow>;
  groups: Map<number, string>;
  rows: SharedParamRow[];
  sourcePath: string;
  loadedAt: string;
};

let cache: CatalogIndexes | null = null;

function defaultCatalogPath(): string {
  const envPath = process.env.SHARED_PARAMETERS_FILE_PATH?.trim();
  if (envPath && existsSync(envPath)) return envPath;
  const root = dirname(fileURLToPath(import.meta.url));
  return join(root, "../../config/Shared_Params_2015_v01.txt");
}

function parseBool01(s: string): boolean {
  return s === "1" || s.toLowerCase() === "true";
}

/** Parse Revit shared parameter file content (tab-separated PARAM / GROUP lines). */
export function parseSharedParametersText(content: string): {
  groups: Map<number, string>;
  params: SharedParamRow[];
} {
  const groups = new Map<number, string>();
  const params: SharedParamRow[] = [];
  const lines = content.split(/\r?\n/);

  for (const line of lines) {
    const t = line.trim();
    if (!t || t.startsWith("#") || t.startsWith("*")) continue;

    const parts = line.split("\t");
    const tag = parts[0]?.trim();
    if (tag === "GROUP" && parts.length >= 3) {
      const id = Number.parseInt(parts[1]!, 10);
      const name = parts.slice(2).join("\t").trim();
      if (Number.isFinite(id) && name) groups.set(id, name);
      continue;
    }

    if (tag === "PARAM" && parts.length >= 10) {
      const guid = parts[1]!.trim();
      const name = parts[2]!.trim();
      const datatype = parts[3]!.trim();
      const dataCategory = parts[4]!.trim();
      const groupId = Number.parseInt(parts[5]!, 10);
      const visible = parseBool01(parts[6]!.trim());
      const description = parts[7]!.trim().replace(/^"(.*)"$/, "$1");
      const userModifiable = parseBool01(parts[8]!.trim());
      const hideWhenNoValue = parseBool01(parts[9]!.trim());

      if (!guid || !name || !Number.isFinite(groupId)) continue;

      params.push({
        guid,
        name,
        datatype,
        dataCategory,
        groupId,
        visible,
        description,
        userModifiable,
        hideWhenNoValue,
      });
    }
  }

  for (const p of params) {
    p.groupName = groups.get(p.groupId);
  }

  return { groups, params };
}

export function getSharedParametersCatalog(): CatalogIndexes {
  if (cache) return cache;
  const path = defaultCatalogPath();
  if (!existsSync(path)) {
    cache = {
      byNameLower: new Map(),
      byGuidLower: new Map(),
      groups: new Map(),
      rows: [],
      sourcePath: path,
      loadedAt: new Date().toISOString(),
    };
    return cache;
  }
  const text = readFileSync(path, "utf-8");
  const { groups, params } = parseSharedParametersText(text);
  const rows = params;
  cache = {
    byNameLower: new Map(),
    byGuidLower: new Map(),
    groups,
    rows,
    sourcePath: path,
    loadedAt: new Date().toISOString(),
  };
  for (const r of rows) {
    const nk = r.name.toLowerCase();
    if (!cache.byNameLower.has(nk)) cache.byNameLower.set(nk, r);
    cache.byGuidLower.set(r.guid.toLowerCase(), r);
  }
  return cache;
}

/** Clear in-memory catalog (e.g. after swapping SHARED_PARAMETERS_FILE_PATH). */
export function clearSharedParametersCatalogCache(): void {
  cache = null;
}

export function getSharedParameterByName(name: string): SharedParamRow | undefined {
  const cat = getSharedParametersCatalog();
  return cat.byNameLower.get(name.trim().toLowerCase());
}

export function getSharedParameterByGuid(guid: string): SharedParamRow | undefined {
  const cat = getSharedParametersCatalog();
  return cat.byGuidLower.get(guid.trim().toLowerCase());
}

export type SharedParamLookupMatch = SharedParamRow & { matchKind: "exact" | "prefix" | "substring" };

export function lookupSharedParameters(options: {
  query: string;
  limit?: number;
  mode?: "exact" | "contains" | "prefix";
}): SharedParamLookupMatch[] {
  const q = options.query.trim();
  const limit = Math.min(Math.max(options.limit ?? 30, 1), 200);
  const mode = options.mode ?? "contains";
  const cat = getSharedParametersCatalog();
  if (!q) return [];

  if (mode === "exact") {
    const row = cat.byNameLower.get(q.toLowerCase());
    return row ? [{ ...row, matchKind: "exact" as const }] : [];
  }

  const ql = q.toLowerCase();
  const out: SharedParamLookupMatch[] = [];
  for (const row of cat.rows) {
    const nl = row.name.toLowerCase();
    if (mode === "prefix" ? nl.startsWith(ql) : nl.includes(ql)) {
      out.push({
        ...row,
        matchKind: nl === ql ? "exact" : mode === "prefix" ? "prefix" : "substring",
      });
      if (out.length >= limit) break;
    }
  }
  return out;
}

/**
 * Optional helper: name → GUID from the bundled .txt (first row per name).
 * Do not auto-attach to DA payloads; use only to build `shared_parameter_guid_map`
 * when you detect duplicate definition names on elements.
 */
export function mapParameterNamesToSharedGuids(
  names: Iterable<string>,
): Record<string, string> {
  const cat = getSharedParametersCatalog();
  const out: Record<string, string> = {};
  for (const raw of names) {
    const name = raw.trim();
    if (!name) continue;
    const row = cat.byNameLower.get(name.toLowerCase());
    if (row) out[name] = row.guid;
  }
  return out;
}

export function getSharedParametersCatalogStats(): {
  parameterCount: number;
  groupCount: number;
  sourcePath: string;
  fileExists: boolean;
  loadedAt: string;
} {
  const path = defaultCatalogPath();
  const cat = getSharedParametersCatalog();
  return {
    parameterCount: cat.rows.length,
    groupCount: cat.groups.size,
    sourcePath: cat.sourcePath,
    fileExists: existsSync(path),
    loadedAt: cat.loadedAt,
  };
}
