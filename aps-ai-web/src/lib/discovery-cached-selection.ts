import { randomUUID } from "node:crypto";
import type { AecdmElementListRow } from "@/lib/aecdm-get-elements";

/**
 * Persisted discovery payload (Tool A) for DA `cached_selection` + chat follow-ups.
 * Extra fields `dbIds` / `element_preview` are for Viewer + UI; strip before DA if desired.
 */
export type DiscoveryCachedSelection = {
  cache_id: string;
  externalIds: string[];
  aecElementIds: string[];
  provenance: {
    analyzedAt: string;
    publishedVersionId?: string;
    modelUrn?: string;
    hubId?: string;
    projectId?: string;
    itemId?: string;
  };
  selection_rules: string;
  intended_operation: string;
  dbIds: number[];
  element_preview: Array<{
    dbId: number;
    externalId: string;
    family?: string;
    type?: string;
    controlMark?: string;
  }>;
};

export type DiscoveryProvenanceInput = {
  modelUrn?: string;
  hubId?: string;
  projectId?: string;
  itemId?: string;
  publishedVersionId?: string;
};

function str(v: unknown): string {
  return typeof v === "string" ? v.trim() : "";
}

/**
 * Optional filters from planner args (snake_case or camelCase).
 */
export function filterAecdmRowsForDiscovery(
  rows: AecdmElementListRow[],
  args: Record<string, unknown>,
): AecdmElementListRow[] {
  const nameContains =
    str(args.name_contains) ||
    str(args.nameContains) ||
    str(args.type_name_contains) ||
    str(args.typeNameContains);
  const controlMarkPrefix =
    str(args.control_mark_prefix) || str(args.controlMarkPrefix);
  const familyContains =
    str(args.family_contains) || str(args.familyContains);
  const typeContains = str(args.type_contains) || str(args.typeContains);

  if (
    !nameContains &&
    !controlMarkPrefix &&
    !familyContains &&
    !typeContains
  ) {
    return rows;
  }

  return rows.filter((r) => {
    const fam = String(r.family ?? "").toLowerCase();
    const typ = String(r.type ?? "").toLowerCase();
    const cm = String(r.controlMark ?? "");

    if (controlMarkPrefix) {
      if (!cm.toUpperCase().startsWith(controlMarkPrefix.toUpperCase())) {
        return false;
      }
    }
    if (familyContains) {
      if (!fam.includes(familyContains.toLowerCase())) return false;
    }
    if (typeContains) {
      if (!typ.includes(typeContains.toLowerCase())) return false;
    }
    if (nameContains) {
      const q = nameContains.toLowerCase();
      if (!fam.includes(q) && !typ.includes(q) && !cm.toLowerCase().includes(q)) {
        return false;
      }
    }
    return true;
  });
}

export function buildDiscoveryCachedSelection(
  rows: AecdmElementListRow[],
  provenance: DiscoveryProvenanceInput,
  selectionRules: string,
  intendedOperation = "future_edit",
): DiscoveryCachedSelection {
  const withDb = rows.filter(
    (r) => r.dbId != null && Number.isFinite(r.dbId),
  ) as Array<AecdmElementListRow & { dbId: number }>;
  const externalIds = withDb
    .map((r) => (r.externalId ? String(r.externalId).trim() : ""))
    .filter(Boolean);
  const dbIds = withDb.map((r) => Math.trunc(r.dbId as number));
  const element_preview = withDb.slice(0, 50).map((r) => ({
    dbId: Math.trunc(r.dbId as number),
    externalId: r.externalId ? String(r.externalId).trim() : "",
    family: r.family != null ? String(r.family) : undefined,
    type: r.type != null ? String(r.type) : undefined,
    controlMark: r.controlMark != null ? String(r.controlMark) : undefined,
  }));

  return {
    cache_id: randomUUID(),
    externalIds,
    aecElementIds: [],
    provenance: {
      analyzedAt: new Date().toISOString(),
      modelUrn: provenance.modelUrn,
      hubId: provenance.hubId,
      projectId: provenance.projectId,
      itemId: provenance.itemId,
      publishedVersionId:
        provenance.publishedVersionId ?? provenance.itemId,
    },
    selection_rules: selectionRules,
    intended_operation: intendedOperation,
    dbIds,
    element_preview,
  };
}

/** DA-facing slice (unknown keys ignored by Revit add-in). */
export function toDaCachedSelectionPayload(
  d: DiscoveryCachedSelection,
): Record<string, unknown> {
  return {
    cache_id: d.cache_id,
    externalIds: d.externalIds,
    aecElementIds: d.aecElementIds,
    provenance: d.provenance,
    selection_rules: d.selection_rules,
    intended_operation: d.intended_operation,
  };
}

export function parseDiscoveryCachedSelectionFromClient(
  raw: unknown,
): DiscoveryCachedSelection | null {
  if (!raw || typeof raw !== "object" || Array.isArray(raw)) return null;
  const o = raw as Record<string, unknown>;
  const cache_id = str(o.cache_id);
  const externalIds = Array.isArray(o.externalIds)
    ? o.externalIds.filter((x): x is string => typeof x === "string")
    : [];
  const dbIds = Array.isArray(o.dbIds)
    ? o.dbIds
        .map((x) => (typeof x === "number" ? x : Number(x)))
        .filter((n) => Number.isFinite(n))
        .map((n) => Math.trunc(n))
    : [];
  if (!cache_id || (externalIds.length === 0 && dbIds.length === 0)) {
    return null;
  }
  const provenanceRaw = o.provenance;
  const provenance =
    provenanceRaw && typeof provenanceRaw === "object" && !Array.isArray(provenanceRaw)
      ? (provenanceRaw as Record<string, unknown>)
      : {};
  return {
    cache_id,
    externalIds,
    aecElementIds: Array.isArray(o.aecElementIds)
      ? o.aecElementIds.filter((x): x is string => typeof x === "string")
      : [],
    provenance: {
      analyzedAt: str(provenance.analyzedAt) || new Date().toISOString(),
      publishedVersionId: str(provenance.publishedVersionId),
      modelUrn: str(provenance.modelUrn),
      hubId: str(provenance.hubId),
      projectId: str(provenance.projectId),
      itemId: str(provenance.itemId),
    },
    selection_rules: str(o.selection_rules) || "restored from session",
    intended_operation: str(o.intended_operation) || "future_edit",
    dbIds,
    element_preview: Array.isArray(o.element_preview)
      ? (o.element_preview as DiscoveryCachedSelection["element_preview"])
      : [],
  };
}

export function subsetDiscoveryByDbIds(
  d: DiscoveryCachedSelection,
  dbIds: number[],
): DiscoveryCachedSelection {
  const want = new Set(dbIds.map((n) => Math.trunc(n)));
  const prev = d.element_preview.filter((e) => want.has(e.dbId));
  let dbOut = prev.map((e) => e.dbId);
  let extOut = prev.map((e) => e.externalId).filter(Boolean);
  if (prev.length === 0 && d.dbIds.length > 0) {
    const idxs = d.dbIds
      .map((id, i) => (want.has(Math.trunc(id)) ? i : -1))
      .filter((i) => i >= 0);
    dbOut = idxs.map((i) => Math.trunc(d.dbIds[i]!));
    extOut = idxs
      .map((i) => (d.externalIds[i] ? String(d.externalIds[i]) : ""))
      .filter(Boolean);
  }
  const preview =
    prev.length > 0
      ? prev
      : dbOut.map((dbId, i) => ({
          dbId,
          externalId: extOut[i] ?? "",
        }));
  return {
    ...d,
    cache_id: randomUUID(),
    dbIds: dbOut,
    externalIds: extOut,
    element_preview: preview,
    selection_rules: `${d.selection_rules} (subset: ${dbOut.length} dbIds)`,
  };
}

export function mergeRowsIntoDiscovery(
  base: DiscoveryCachedSelection,
  rows: AecdmElementListRow[],
  dbIds: number[],
): DiscoveryCachedSelection {
  const want = new Set(dbIds.map((n) => Math.trunc(n)));
  const matched = rows.filter(
    (r) => r.dbId != null && want.has(Math.trunc(r.dbId as number)),
  );
  if (matched.length === 0) {
    return subsetDiscoveryByDbIds(base, dbIds);
  }
  return buildDiscoveryCachedSelection(
    matched,
    {
      modelUrn: base.provenance.modelUrn,
      hubId: base.provenance.hubId,
      projectId: base.provenance.projectId,
      itemId: base.provenance.itemId,
      publishedVersionId: base.provenance.publishedVersionId,
    },
    `${base.selection_rules} (refined via select_elements)`,
    base.intended_operation,
  );
}
