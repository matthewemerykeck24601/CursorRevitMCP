import { resolveAecProjectId } from "@/lib/aps";
import { resolveAecdmDesignId } from "@/lib/aecdm-design-id";

export type AecdmProductPrefix = "WPA" | "WPB" | "CLA" | "COLUMN" | "ALL";

/** Aligns with mcp-server get_elements_by_category Metromont filters. */
export function metromontAecdmQueryFilters(
  category?: string,
  family?: string,
  product_prefix?: AecdmProductPrefix,
): { filterCategory?: string; filterFamily?: string } {
  let filterCategory = category;
  let filterFamily = family;
  const catLower = (category ?? "").toLowerCase();
  const columnIntent =
    product_prefix === "COLUMN" ||
    (!!category && catLower.includes("column"));

  if (
    (product_prefix && product_prefix !== "ALL") ||
    (!!category && catLower.includes("column"))
  ) {
    filterCategory = "Structural Framing";
    if (columnIntent) {
      filterFamily = family?.trim() ? family : "COLUMN";
    } else if (
      product_prefix &&
      ["WPA", "WPB", "CLA"].includes(product_prefix)
    ) {
      filterFamily = family?.trim() ? family : product_prefix;
    }
  }

  return { filterCategory, filterFamily };
}

export type AecdmElementListRow = {
  dbId: number | null;
  category?: unknown;
  family?: unknown;
  type?: unknown;
  controlMark?: unknown;
  externalId?: string | null;
};

/**
 * AEC Data Model REST: design elements (same endpoint as MCP get_elements_by_category).
 */
export async function fetchAecdmElementsByCategory(params: {
  accessToken: string;
  hubId: string;
  dmProjectId: string;
  modelUrn: string;
  category?: string;
  family?: string;
  type?: string;
  limit?: number;
  product_prefix?: AecdmProductPrefix;
}): Promise<{ success: true; count: number; elements: AecdmElementListRow[] }> {
  // Keep GraphQL AEC project resolution as a readiness probe, but AECDM REST
  // /aecdm/v1/projects/{id}/designs/{id}/elements expects the DM project id.
  await resolveAecProjectId(
    params.accessToken,
    params.hubId,
    params.dmProjectId,
  );
  const designId = resolveAecdmDesignId(params.modelUrn);
  if (!designId) {
    throw new Error(
      "AECDM elements query failed: could not resolve design id from selected model identifier.",
    );
  }

  const { filterCategory, filterFamily } = metromontAecdmQueryFilters(
    params.category,
    params.family,
    params.product_prefix,
  );

  const url = new URL(
    `https://developer.api.autodesk.com/aecdm/v1/projects/${encodeURIComponent(params.dmProjectId)}/designs/${encodeURIComponent(designId)}/elements`,
  );
  url.searchParams.set("limit", String(params.limit ?? 500));
  if (filterCategory) url.searchParams.set("filter[category]", filterCategory);
  if (filterFamily) url.searchParams.set("filter[family]", filterFamily);
  if (params.type) url.searchParams.set("filter[type]", params.type);

  const res = await fetch(url.toString(), {
    headers: { Authorization: `Bearer ${params.accessToken}` },
    cache: "no-store",
  });
  if (!res.ok) {
    const t = await res.text();
    throw new Error(`AECDM elements query failed ${res.status}: ${t}`);
  }

  const raw = (await res.json()) as { elements?: unknown[] };
  const list = Array.isArray(raw.elements) ? raw.elements : [];

  const elements: AecdmElementListRow[] = list.map((el: unknown) => {
    const row = el as {
      id?: unknown;
      category?: unknown;
      family?: unknown;
      type?: unknown;
      properties?: Record<string, unknown>;
    };
    const props = row.properties ?? {};
    const extRaw =
      props["External ID"] ?? props.ExternalId ?? props.externalId;
    const idRaw = row.id;
    const n =
      typeof idRaw === "number"
        ? idRaw
        : typeof idRaw === "string"
          ? Number(idRaw)
          : NaN;
    return {
      dbId: Number.isFinite(n) ? Math.trunc(n) : null,
      category: row.category,
      family: row.family,
      type: row.type,
      controlMark: props.CONTROL_MARK ?? null,
      externalId:
        extRaw == null || extRaw === ""
          ? null
          : String(extRaw).trim() || null,
    };
  });

  return { success: true, count: elements.length, elements };
}
