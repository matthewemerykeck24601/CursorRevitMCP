import fs from "node:fs";
import path from "node:path";
import { env } from "@/lib/env";
import { getDesignAutomationAccessToken } from "@/lib/da-workitems";
import { buildVersionOssGetArgument } from "@/lib/aps";
import {
  getHubProjectsWithCache,
  normalizeProjectNumber,
} from "@/lib/aps-admin";

const APS_BASE = "https://developer.api.autodesk.com";
const TEMPLATE_CONFIG_PATH = path.join(
  process.cwd(),
  "config",
  "revit-cloud-model-templates.json",
);
const DEFAULT_TARGET_FOLDER_PATH_CANDIDATES = [
  ["Project Files", "001 - Models", "01 - Model"],
  ["Project Files", "01 - Models", "001 - Model"],
];

type RevitCloudTemplate = {
  key: string;
  label: string;
  template_model_guid?: string;
  template_project_guid?: string;
  template_source_project_id?: string;
  template_source_folder_id?: string;
  template_source_folder_urn?: string;
  template_name_contains?: string[];
  resolve_template_guid_on_call?: boolean;
  default_target_folder_id?: string;
  region?: "US" | "EMEA";
};

type TemplateConfigFile = {
  templates?: unknown[];
};

export type CreateRevitCloudModelParams = {
  accessToken: string;
  hubId: string;
  projectNumber?: string;
  projectId?: string;
  folderId?: string;
  modelName?: string;
  templateKey?: string;
  templateModelGuid?: string;
  templateProjectGuid?: string;
  region?: "US" | "EMEA";
  enableWorksharing?: boolean;
  activityId?: string;
  dryRun?: boolean;
};

function asTrimmed(v: unknown): string {
  return typeof v === "string" ? v.trim() : "";
}

function normalizeKey(v: string): string {
  return v.trim().toUpperCase().replace(/\s+/g, "_");
}


function stripBPrefix(id: string): string {
  return id.startsWith("b.") ? id.slice(2) : id;
}

function normalizeFolderName(v: string): string {
  return v.trim().toLowerCase().replace(/\s+/g, " ");
}

function sanitizeModelNamePart(v: string): string {
  return v
    .replace(/[\\/:*?"<>|]/g, " ")
    .replace(/\s+/g, " ")
    .trim();
}

function inferRevitYearSuffix(
  activityId: string,
  template: RevitCloudTemplate,
): string {
  const candidates = [
    template.label,
    ...(template.template_name_contains ?? []),
    activityId,
  ];
  for (const candidate of candidates) {
    const match = candidate.match(/\b(20\d{2})\b/);
    if (!match) continue;
    const year = Number.parseInt(match[1] ?? "", 10);
    if (!Number.isFinite(year)) continue;
    if (year < 2000 || year > 2099) continue;
    return `_${String(year % 100).padStart(2, "0")}`;
  }
  return "";
}

function normalizeComparableProjectNumber(value: string): string {
  const n = normalizeProjectNumber(value);
  if (!n) return "";
  if (/^\d+$/.test(n)) return n.replace(/^0+/, "") || "0";
  return n;
}

function parseCsvRow(line: string): string[] {
  const out: string[] = [];
  let current = "";
  let inQuotes = false;
  for (let i = 0; i < line.length; i += 1) {
    const ch = line[i];
    if (ch === '"') {
      if (inQuotes && line[i + 1] === '"') {
        current += '"';
        i += 1;
      } else {
        inQuotes = !inQuotes;
      }
      continue;
    }
    if (ch === "," && !inQuotes) {
      out.push(current);
      current = "";
      continue;
    }
    current += ch;
  }
  out.push(current);
  return out;
}

function looksGuid(v: string): boolean {
  const t = v.trim();
  if (!t) return false;
  if (/^[0-9a-fA-F-]{32,40}$/.test(t)) return true;
  // APS lineage/model GUID tokens are often URL-safe base64-like ids.
  return /^[A-Za-z0-9_-]{16,80}$/.test(t);
}

function normalizeFolderIdForDataApi(raw: string): string {
  const v = raw.trim();
  if (!v) return "";
  if (v.startsWith("urn:adsk.wipprod:fs.folder:")) return v;
  if (v.startsWith("co.")) return `urn:adsk.wipprod:fs.folder:${v}`;
  const m = v.match(/(co\.[A-Za-z0-9_-]+)/);
  if (!m?.[1]) return "";
  return `urn:adsk.wipprod:fs.folder:${m[1]}`;
}

function extractTemplateModelGuidFromItemId(itemIdRaw: string): string {
  const itemId = itemIdRaw.trim();
  if (!itemId) return "";
  const lineagePrefix = "urn:adsk.wipprod:dm.lineage:";
  if (itemId.startsWith(lineagePrefix)) {
    const suffix = itemId.slice(lineagePrefix.length).split("?")[0] ?? "";
    return suffix.trim();
  }
  const stripped = stripBPrefix(itemId);
  if (looksGuid(stripped)) return stripped;
  return "";
}

function toDataUrl(payload: unknown): string {
  return `data:application/json;base64,${Buffer.from(
    JSON.stringify(payload),
    "utf8",
  ).toString("base64")}`;
}

async function getDaActivityParameterNames(
  daToken: string,
  activityIdRaw: string,
): Promise<string[] | null> {
  const activityId = activityIdRaw.trim();
  if (!activityId) return null;
  try {
    const res = await fetch(
      `${daBaseUrl()}/activities/${encodeURIComponent(activityId)}`,
      {
        method: "GET",
        headers: {
          Authorization: `Bearer ${daToken}`,
          "Content-Type": "application/json",
        },
        cache: "no-store",
      },
    );
    if (!res.ok) return null;
    const json = (await res.json()) as {
      parameters?: Record<string, unknown>;
    };
    if (!json.parameters || typeof json.parameters !== "object") return [];
    return Object.keys(json.parameters);
  } catch {
    return null;
  }
}

function daBaseUrl(): string {
  const region = (env.daRegion || "us-east").trim().toLowerCase();
  if (region === "emea" || region === "eu") {
    return "https://developer.api.autodesk.com/da/eu/v3";
  }
  if (region === "apac" || region === "asia") {
    return "https://developer.api.autodesk.com/da/apac/v3";
  }
  return "https://developer.api.autodesk.com/da/us-east/v3";
}

function loadTemplateConfig(): RevitCloudTemplate[] {
  try {
    if (!fs.existsSync(TEMPLATE_CONFIG_PATH)) return [];
    const raw = fs.readFileSync(TEMPLATE_CONFIG_PATH, "utf8");
    const parsed = JSON.parse(raw) as TemplateConfigFile;
    const rows = Array.isArray(parsed.templates) ? parsed.templates : [];
    return rows
      .map((entry) => {
        if (!entry || typeof entry !== "object" || Array.isArray(entry)) {
          return null;
        }
        const rec = entry as Record<string, unknown>;
        const key = normalizeKey(asTrimmed(rec.key));
        const label = asTrimmed(rec.label) || key;
        if (!key) return null;
        const regionRaw = normalizeKey(asTrimmed(rec.region));
        const region = regionRaw === "EMEA" ? "EMEA" : "US";
        return {
          key,
          label,
          template_model_guid: asTrimmed(rec.template_model_guid),
          template_project_guid: asTrimmed(rec.template_project_guid),
          template_source_project_id: asTrimmed(rec.template_source_project_id),
          template_source_folder_id: asTrimmed(rec.template_source_folder_id),
          template_source_folder_urn: asTrimmed(rec.template_source_folder_urn),
          template_name_contains: Array.isArray(rec.template_name_contains)
            ? rec.template_name_contains
                .map((v) => asTrimmed(v))
                .filter((v) => v.length > 0)
            : undefined,
          resolve_template_guid_on_call:
            rec.resolve_template_guid_on_call === true,
          default_target_folder_id: asTrimmed(rec.default_target_folder_id),
          region,
        } satisfies RevitCloudTemplate;
      })
      .filter((v): v is RevitCloudTemplate => Boolean(v));
  } catch {
    return [];
  }
}

async function findProjectByJobNumberAdminApi(params: {
  accessToken: string;
  hubId: string;
  projectNumber: string;
  region?: "US" | "EMEA";
}): Promise<{ projectId: string; projectName: string; projectNumber: string } | null> {
  const accountId = stripBPrefix(params.hubId);
  const requested = normalizeProjectNumber(params.projectNumber);
  const requestedComparable = normalizeComparableProjectNumber(requested);
  if (!requested || !accountId) return null;
  const attempts = Array.from(
    new Set(
      [requested, requestedComparable].filter(
        (v) => typeof v === "string" && v.trim().length > 0,
      ),
    ),
  );
  for (const value of attempts) {
    const query = new URLSearchParams();
    query.set("filter[jobNumber]", value);
    query.set("filterTextMatch", "equals");
    query.set("limit", "200");
    const res = await fetch(
      `${APS_BASE}/construction/admin/v1/accounts/${encodeURIComponent(accountId)}/projects?${query.toString()}`,
      {
        method: "GET",
        headers: {
          Authorization: `Bearer ${params.accessToken}`,
          "Content-Type": "application/json",
          Region: params.region === "EMEA" ? "EMEA" : "US",
        },
        cache: "no-store",
      },
    );
    if (!res.ok) continue;
    const json = (await res.json()) as {
      results?: Array<{ id?: string; name?: string; jobNumber?: string }>;
    };
    const rows = Array.isArray(json.results) ? json.results : [];
    const hit = rows.find((row) => {
      const id = asTrimmed(row.id);
      const job = normalizeProjectNumber(String(row.jobNumber ?? ""));
      if (!id || !job) return false;
      return (
        job === requested ||
        normalizeComparableProjectNumber(job) === requestedComparable
      );
    });
    if (hit) {
      const rawId = asTrimmed(hit.id);
      const projectId = rawId.startsWith("b.") ? rawId : `b.${rawId}`;
      return {
        projectId,
        projectName: asTrimmed(hit.name) || projectId,
        projectNumber: normalizeProjectNumber(String(hit.jobNumber ?? requested)),
      };
    }
  }
  return null;
}

async function resolveProjectByNumber(
  accessToken: string,
  hubId: string,
  projectNumber: string,
  region?: "US" | "EMEA",
): Promise<{
  hit: { projectId: string; projectName: string; projectNumber: string } | null;
  debug: Record<string, unknown>;
}> {
  const normalized = normalizeProjectNumber(projectNumber);
  if (!normalized) {
    return {
      hit: null,
      debug: {
        normalized_project_number: normalized,
        source: "invalid_project_number",
      },
    };
  }
  const adminApiHit = await findProjectByJobNumberAdminApi({
    accessToken,
    hubId,
    projectNumber: normalized,
    region,
  });
  if (adminApiHit) {
    return {
      hit: adminApiHit,
      debug: {
        normalized_project_number: normalized,
        source: "acc_admin_projects_filter_jobnumber",
      },
    };
  }
  const cache = await getHubProjectsWithCache({
    accessToken,
    hubId,
    requestedProjectNumbers: [normalized],
  });
  const cacheHit = cache.projectsByNumber.get(normalized) ?? null;
  if (cacheHit) {
    return {
      hit: cacheHit,
      debug: {
        normalized_project_number: normalized,
        source: "hub_cache_or_live",
        cache_source: cache.cache_source,
        cache_updated: cache.cache_updated,
        project_cache_object_key: cache.project_cache_object_key,
      },
    };
  }

  const localCsvHit = (() => {
    const hubKey = stripBPrefix(hubId);
    const candidates = [
      path.join(process.cwd(), "admin_projects.csv"),
      path.join(process.cwd(), "..", "admin_projects.csv"),
    ];
    for (const file of candidates) {
      try {
        if (!fs.existsSync(file)) continue;
        const raw = fs.readFileSync(file, "utf8");
        const lines = raw
          .replace(/^\uFEFF/, "")
          .split(/\r?\n/)
          .map((l) => l.trim())
          .filter((l) => l.length > 0);
        if (lines.length < 2) continue;
        const headers = parseCsvRow(lines[0]).map((h) => h.trim().toLowerCase());
        const idIdx = headers.findIndex((h) => h === "id");
        const accountIdx = headers.findIndex((h) => h === "account_id");
        const numberIdx = headers.findIndex((h) => h === "job_number");
        const nameIdx = headers.findIndex((h) => h === "name");
        if (idIdx < 0 || accountIdx < 0 || numberIdx < 0) continue;
        for (let i = 1; i < lines.length; i += 1) {
          const cols = parseCsvRow(lines[i]);
          if (cols.length <= Math.max(idIdx, accountIdx, numberIdx)) continue;
          const accountId = cols[accountIdx]?.trim() ?? "";
          const job = normalizeProjectNumber(cols[numberIdx] ?? "");
          const idRaw = cols[idIdx]?.trim() ?? "";
          if (!idRaw || stripBPrefix(accountId) !== hubKey || job !== normalized) continue;
          const projectId = idRaw.startsWith("b.") ? idRaw : `b.${idRaw}`;
          const projectName =
            nameIdx >= 0 && cols.length > nameIdx ? (cols[nameIdx]?.trim() ?? "") : "";
          return { projectId, projectName, projectNumber: normalized, sourceFile: file };
        }
      } catch {
        // continue
      }
    }
    return null;
  })();

  return {
    hit: localCsvHit,
    debug: {
      normalized_project_number: normalized,
      source: localCsvHit ? "local_admin_projects_csv" : "not_found",
      cache_source: cache.cache_source,
      cache_updated: cache.cache_updated,
      project_cache_object_key: cache.project_cache_object_key,
      ...(localCsvHit ? { local_csv_file: localCsvHit.sourceFile } : {}),
    },
  };
}

async function listFolderChildren(params: {
  accessToken: string;
  projectId: string;
  folderId: string;
}): Promise<Array<{ id: string; name: string }>> {
  const out: Array<{ id: string; name: string }> = [];
  let nextPath: string | null = `/data/v1/projects/${encodeURIComponent(params.projectId)}/folders/${encodeURIComponent(params.folderId)}/contents`;
  while (nextPath) {
    const res = await fetch(`${APS_BASE}${nextPath}`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(`Folder contents failed (${res.status}): ${text}`);
    }
    const json = (await res.json()) as {
      data?: Array<{ id?: string; type?: string; attributes?: { name?: string } }>;
      links?: { next?: { href?: string } };
    };
    for (const row of json.data ?? []) {
      if (row.type !== "folders") continue;
      const id = asTrimmed(row.id);
      const name = asTrimmed(row.attributes?.name);
      if (!id || !name) continue;
      out.push({ id, name });
    }
    const href = asTrimmed(json.links?.next?.href);
    if (!href) {
      nextPath = null;
    } else {
      const u = new URL(href);
      nextPath = `${u.pathname}${u.search}`;
    }
  }
  return out;
}

async function resolveFolderIdByPath(params: {
  accessToken: string;
  hubId: string;
  projectId: string;
  folderPath: string[];
}): Promise<string | null> {
  if (params.folderPath.length === 0) return null;
  const topRes = await fetch(
    `${APS_BASE}/project/v1/hubs/${encodeURIComponent(params.hubId)}/projects/${encodeURIComponent(params.projectId)}/topFolders`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    },
  );
  if (!topRes.ok) {
    const text = await topRes.text();
    throw new Error(`Top folders failed (${topRes.status}): ${text}`);
  }
  const topJson = (await topRes.json()) as {
    data?: Array<{ id?: string; attributes?: { name?: string } }>;
  };
  const wantedTop = normalizeFolderName(params.folderPath[0]!);
  const topHit = (topJson.data ?? [])
    .map((row) => ({
      id: asTrimmed(row.id),
      name: asTrimmed(row.attributes?.name),
    }))
    .find((row) => row.id && normalizeFolderName(row.name) === wantedTop);
  if (!topHit) return null;
  let currentId = topHit.id;
  for (const segment of params.folderPath.slice(1)) {
    const wanted = normalizeFolderName(segment);
    const children = await listFolderChildren({
      accessToken: params.accessToken,
      projectId: params.projectId,
      folderId: currentId,
    });
    const hit = children.find((c) => normalizeFolderName(c.name) === wanted);
    if (!hit) return null;
    currentId = hit.id;
  }
  return currentId;
}

async function resolveDefaultTargetFolder(params: {
  accessToken: string;
  hubId: string;
  projectId: string;
}): Promise<{ folderId: string; folderPath: string[] } | null> {
  for (const folderPath of DEFAULT_TARGET_FOLDER_PATH_CANDIDATES) {
    const folderId = await resolveFolderIdByPath({
      accessToken: params.accessToken,
      hubId: params.hubId,
      projectId: params.projectId,
      folderPath,
    });
    if (folderId) return { folderId, folderPath };
  }
  return null;
}

async function listFolderItems(params: {
  accessToken: string;
  projectId: string;
  folderId: string;
}): Promise<Array<{ id: string; name: string; extensionType: string }>> {
  const out: Array<{ id: string; name: string; extensionType: string }> = [];
  let nextPath: string | null = `/data/v1/projects/${encodeURIComponent(params.projectId)}/folders/${encodeURIComponent(params.folderId)}/contents`;
  while (nextPath) {
    const res = await fetch(`${APS_BASE}${nextPath}`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(`Template folder list failed (${res.status}): ${text}`);
    }
    const json = (await res.json()) as {
      data?: Array<{
        id?: string;
        type?: string;
        attributes?: { name?: string; displayName?: string; extension?: { type?: string } };
      }>;
      links?: { next?: { href?: string } };
    };
    for (const row of json.data ?? []) {
      if (row.type !== "items") continue;
      const id = asTrimmed(row.id);
      const name = asTrimmed(row.attributes?.displayName) || asTrimmed(row.attributes?.name);
      const extensionType = asTrimmed(row.attributes?.extension?.type).toLowerCase();
      if (!id || !name) continue;
      out.push({ id, name, extensionType });
    }
    const href = asTrimmed(json.links?.next?.href);
    if (!href) {
      nextPath = null;
    } else {
      const u = new URL(href);
      nextPath = `${u.pathname}${u.search}`;
    }
  }
  return out;
}

async function resolveTemplateModelGuidFromSource(params: {
  accessToken: string;
  template: RevitCloudTemplate;
}): Promise<{
  modelGuid: string;
  sourceItemId: string;
  sourceItemName: string;
  sourceProjectId: string;
} | null> {
  const projectId = asTrimmed(params.template.template_source_project_id);
  const folderId =
    normalizeFolderIdForDataApi(asTrimmed(params.template.template_source_folder_urn)) ||
    normalizeFolderIdForDataApi(asTrimmed(params.template.template_source_folder_id));
  if (!projectId || !folderId) return null;
  const items = await listFolderItems({
    accessToken: params.accessToken,
    projectId,
    folderId,
  });
  const revitItems = items.filter((it) => {
    const nameLower = it.name.toLowerCase();
    return (
      nameLower.endsWith(".rvt") ||
      it.extensionType.includes("revitcloudmodel") ||
      it.extensionType.includes("c4r")
    );
  });
  if (revitItems.length === 0) return null;
  const rawHints =
    params.template.template_name_contains && params.template.template_name_contains.length > 0
      ? params.template.template_name_contains
      : [params.template.label];
  const hints = rawHints.map((h) => h.toLowerCase()).filter(Boolean);
  const matched = revitItems.filter((it) => {
    const hay = it.name.toLowerCase();
    return hints.every((hint) => hay.includes(hint));
  });
  const picked = (matched.length > 0 ? matched : revitItems)[0]!;
  const modelGuid = extractTemplateModelGuidFromItemId(picked.id);
  if (!modelGuid) return null;
  return {
    modelGuid,
    sourceItemId: picked.id,
    sourceItemName: picked.name,
    sourceProjectId: projectId,
  };
}

async function getItemTipVersionId(params: {
  accessToken: string;
  projectId: string;
  itemId: string;
}): Promise<string> {
  const res = await fetch(
    `${APS_BASE}/data/v1/projects/${encodeURIComponent(params.projectId)}/items/${encodeURIComponent(params.itemId)}/tip`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    },
  );
  if (!res.ok) return "";
  const json = (await res.json()) as { data?: { id?: string } };
  return asTrimmed(json.data?.id);
}

async function resolveBootstrapInputFile(params: {
  accessToken: string;
  candidateSources: Array<{ projectId: string; folderId: string }>;
}): Promise<
  | {
      url: string;
      headers: Record<string, string>;
      sourceProjectId: string;
      sourceFolderId: string;
      sourceItemId: string;
    }
  | undefined
> {
  for (const source of params.candidateSources) {
    if (!source.projectId || !source.folderId) continue;
    let items: Array<{ id: string; name: string; extensionType: string }> = [];
    try {
      items = await listFolderItems({
        accessToken: params.accessToken,
        projectId: source.projectId,
        folderId: source.folderId,
      });
    } catch {
      continue;
    }
    const prioritized = [...items].sort((a, b) => {
      const aScore =
        (a.name.toLowerCase().includes("template") ? 10 : 0) +
        (a.name.toLowerCase().endsWith(".rvt") ? 5 : 0);
      const bScore =
        (b.name.toLowerCase().includes("template") ? 10 : 0) +
        (b.name.toLowerCase().endsWith(".rvt") ? 5 : 0);
      return bScore - aScore;
    });
    for (const item of prioritized) {
      const tipVersionId = await getItemTipVersionId({
        accessToken: params.accessToken,
        projectId: source.projectId,
        itemId: item.id,
      });
      if (!tipVersionId) continue;
      const signed = await buildVersionOssGetArgument({
        accessToken: params.accessToken,
        projectId: source.projectId,
        versionId: tipVersionId,
      });
      if (signed?.url) {
        return {
          url: signed.url,
          headers: signed.headers,
          sourceProjectId: source.projectId,
          sourceFolderId: source.folderId,
          sourceItemId: item.id,
        };
      }
    }
  }
  return undefined;
}

function extractWorkitemId(raw: unknown): string {
  if (!raw || typeof raw !== "object" || Array.isArray(raw)) return "";
  const o = raw as Record<string, unknown>;
  const id = asTrimmed(o.id) || asTrimmed(o.workitemId) || asTrimmed(o.workItemId);
  return id;
}

export async function createRevitCloudWorksharedModel(
  params: CreateRevitCloudModelParams,
): Promise<Record<string, unknown>> {
  const templates = loadTemplateConfig();
  const templateOptions = templates.map((t) => ({ key: t.key, label: t.label }));
  const requestedTemplateKey = normalizeKey(params.templateKey ?? "");

  if (!requestedTemplateKey) {
    return {
      success: false,
      requires_template_selection: true,
      template_options: templateOptions,
      message:
        "No template was specified. Choose one of: Metromont Working, CEG, or Devita.",
    };
  }

  const template = templates.find((t) => t.key === requestedTemplateKey);
  if (!template) {
    return {
      success: false,
      requires_template_selection: true,
      requested_template: requestedTemplateKey,
      template_options: templateOptions,
      message: `Template "${requestedTemplateKey}" is not configured.`,
    };
  }

  let templateModelGuid =
    asTrimmed(params.templateModelGuid) || asTrimmed(template.template_model_guid);
  let templateModelResolvedFrom: Record<string, unknown> | undefined;
  const shouldResolveTemplateOnCall =
    template.resolve_template_guid_on_call === true ||
    (!templateModelGuid &&
      (Boolean(template.template_source_project_id) ||
        Boolean(template.template_source_folder_id) ||
        Boolean(template.template_source_folder_urn)));
  if (shouldResolveTemplateOnCall) {
    const resolved = await resolveTemplateModelGuidFromSource({
      accessToken: params.accessToken,
      template,
    });
    if (resolved?.modelGuid) {
      templateModelGuid = resolved.modelGuid;
      templateModelResolvedFrom = {
        source: "template_source_folder_lookup",
        item_id: resolved.sourceItemId,
        item_name: resolved.sourceItemName,
        sourceProjectId: resolved.sourceProjectId,
      };
    }
  }
  if (!templateModelGuid) {
    return {
      success: false,
      requires_template_setup: true,
      template: { key: template.key, label: template.label },
      message:
        "Template model GUID could not be resolved. Configure template_model_guid or template_source_project_id + template_source_folder_id/urn.",
    };
  }

  if (!looksGuid(stripBPrefix(templateModelGuid))) {
    return {
      success: false,
      template: { key: template.key, label: template.label },
      message: `template_model_guid is not a GUID-looking value: ${templateModelGuid}`,
    };
  }

  let projectId = asTrimmed(params.projectId);
  let resolvedProjectName = "";
  const requestedProjectNumber = asTrimmed(params.projectNumber).toUpperCase();
  let projectLookupDebug: Record<string, unknown> | undefined;
  if (!projectId) {
    if (!requestedProjectNumber) {
      return {
        success: false,
        message: "project_number or project_id is required.",
      };
    }
    const resolvedProject = await resolveProjectByNumber(
      params.accessToken,
      params.hubId,
      requestedProjectNumber,
      params.region ?? template.region ?? "US",
    );
    projectLookupDebug = resolvedProject.debug;
    if (!resolvedProject.hit) {
      return {
        success: false,
        message: `No project matched project number ${requestedProjectNumber} in selected hub (checked cache + live refresh).`,
        project_lookup_debug: projectLookupDebug,
      };
    }
    projectId = resolvedProject.hit.projectId;
    resolvedProjectName = asTrimmed(resolvedProject.hit.projectName);
  }

  const folderId =
    asTrimmed(params.folderId) || asTrimmed(template.default_target_folder_id);
  const resolvedDefaultFolder = folderId
    ? null
    : await resolveDefaultTargetFolder({
        accessToken: params.accessToken,
        hubId: params.hubId,
        projectId,
      });
  const resolvedFolderId = folderId || resolvedDefaultFolder?.folderId || "";
  const resolvedFolderPath = resolvedDefaultFolder?.folderPath;
  if (!resolvedFolderId) {
    return {
      success: false,
      requires_folder_id: true,
      template: { key: template.key, label: template.label },
      project_id: projectId,
      message:
        `Unable to resolve target folder path. Tried: ${DEFAULT_TARGET_FOLDER_PATH_CANDIDATES.map((p) => p.join(" > ")).join(" OR ")}. Provide folder_id or configure default_target_folder_id for the template.`,
    };
  }

  const activityId =
    asTrimmed(params.activityId) ||
    asTrimmed(env.daActivityIdCreateModel) ||
    asTrimmed(env.daActivityId);
  if (!activityId) {
    return {
      success: false,
      message:
        "No create-model Design Automation activity is configured. Set DA_ACTIVITY_ID_CREATE_MODEL.",
    };
  }

  const accountId = stripBPrefix(params.hubId);
  const targetProjectGuid = stripBPrefix(projectId);
  const templateProjectGuid =
    stripBPrefix(asTrimmed(params.templateProjectGuid)) ||
    stripBPrefix(asTrimmed(template.template_project_guid)) ||
    targetProjectGuid;
  const region = params.region ?? template.region ?? "US";
  const explicitModelName = asTrimmed(params.modelName);
  const inferredYearSuffix = inferRevitYearSuffix(activityId, template);
  const inferredProjectName = sanitizeModelNamePart(resolvedProjectName);
  const inferredFallbackProjectName = sanitizeModelNamePart(requestedProjectNumber);
  const defaultModelNameBase =
    inferredProjectName || inferredFallbackProjectName || "New Revit Model";
  const modelName =
    explicitModelName ||
    `${defaultModelNameBase}${inferredYearSuffix}`.trim() ||
    `New Revit Model ${Date.now()}`;
  const enableWorksharing = params.enableWorksharing !== false;

  const revitModel = {
    region,
    projectGuid: templateProjectGuid,
    modelGuid: stripBPrefix(templateModelGuid),
    toolName: "create_model",
    save: true,
  };
  const toolInputs = {
    accountId,
    projectId: targetProjectGuid,
    folderId: resolvedFolderId,
    enableWorksharing,
    modelName,
  };

  let inputFileArgument:
    | {
        url: string;
        headers?: Record<string, string>;
      }
    | undefined;
  const bootstrapSourceCandidates: Array<{ projectId: string; folderId: string }> =
    [];
  const templateSourceProjectId = asTrimmed(template.template_source_project_id);
  const templateSourceFolderId =
    normalizeFolderIdForDataApi(asTrimmed(template.template_source_folder_urn)) ||
    normalizeFolderIdForDataApi(asTrimmed(template.template_source_folder_id));
  if (templateSourceProjectId && templateSourceFolderId) {
    bootstrapSourceCandidates.push({
      projectId: templateSourceProjectId,
      folderId: templateSourceFolderId,
    });
  }
  if (projectId && resolvedFolderId) {
    bootstrapSourceCandidates.push({
      projectId,
      folderId: resolvedFolderId,
    });
  }
  const bootstrapInput = await resolveBootstrapInputFile({
    accessToken: params.accessToken,
    candidateSources: bootstrapSourceCandidates,
  });
  if (bootstrapInput?.url) {
    inputFileArgument = {
      url: bootstrapInput.url,
      headers: bootstrapInput.headers,
    };
  }

  const workitemBody = {
    activityId,
    arguments: {
      adsk3LeggedToken: params.accessToken,
      ...(inputFileArgument?.url
        ? {
            inputFile: {
              url: inputFileArgument.url,
              ...(inputFileArgument.headers
                ? { headers: inputFileArgument.headers }
                : {}),
            },
          }
        : {}),
      revitmodel: { url: toDataUrl(revitModel) },
      toolinputs: { url: toDataUrl(toolInputs) },
    },
  };

  if (params.dryRun) {
    return {
      success: true,
      dry_run: true,
      message: "Dry run only. Workitem payload prepared.",
      payload_preview: workitemBody,
      resolved: {
        template: { key: template.key, label: template.label },
        project_id: projectId,
          ...(projectLookupDebug ? { project_lookup_debug: projectLookupDebug } : {}),
          ...(templateModelResolvedFrom
            ? { template_model_guid_resolved_from: templateModelResolvedFrom }
            : {}),
          ...(inputFileArgument?.url
            ? { input_file_from_template: true }
            : { input_file_from_template: false }),
          ...(bootstrapInput
            ? {
                input_file_source: {
                  project_id: bootstrapInput.sourceProjectId,
                  folder_id: bootstrapInput.sourceFolderId,
                  item_id: bootstrapInput.sourceItemId,
                },
              }
            : {}),
          folder_id: resolvedFolderId,
          ...(resolvedFolderPath ? { resolved_folder_path: resolvedFolderPath } : {}),
        model_name: modelName,
      },
    };
  }

  const daToken = await getDesignAutomationAccessToken();
  const activityParameterNames = await getDaActivityParameterNames(
    daToken,
    activityId,
  );
  if (activityParameterNames) {
    const hasInputFile = activityParameterNames.includes("inputFile");
    const hasPayload = activityParameterNames.includes("payload");
    const hasRevitModel = activityParameterNames.includes("revitmodel");
    const hasToolInputs = activityParameterNames.includes("toolinputs");
    if ((hasInputFile || hasPayload) && (!hasRevitModel || !hasToolInputs)) {
      return {
        success: false,
        activity_profile_mismatch: true,
        message:
          "Configured create-model activity appears to be mark-profile (expects inputFile/payload) instead of create-profile (expects revitmodel/toolinputs). Publish/select a create_model activity alias and set DA_ACTIVITY_ID_CREATE_MODEL.",
        activity_id: activityId,
        activity_parameters: activityParameterNames,
      };
    }
  }
  const submitRes = await fetch(`${daBaseUrl()}/workitems`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${daToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(workitemBody),
    cache: "no-store",
  });
  const submitText = await submitRes.text();
  let submitJson: unknown = submitText;
  try {
    submitJson = submitText ? (JSON.parse(submitText) as unknown) : {};
  } catch {
    // keep raw string
  }
  if (!submitRes.ok) {
    return {
      success: false,
      message: `DA submit failed (${submitRes.status}).`,
      error: submitJson,
      resolved: {
        template: { key: template.key, label: template.label },
        project_id: projectId,
        folder_id: resolvedFolderId,
      },
    };
  }

  return {
    success: true,
    workitem_submitted: true,
    workitem_id: extractWorkitemId(submitJson),
    status:
      typeof submitJson === "object" &&
      submitJson !== null &&
      typeof (submitJson as { status?: unknown }).status === "string"
        ? (submitJson as { status: string }).status
        : "submitted",
    message:
      "Create-model workitem submitted. Poll status to confirm creation and publish completion.",
    resolved: {
      template: { key: template.key, label: template.label },
      project_id: projectId,
      ...(projectLookupDebug ? { project_lookup_debug: projectLookupDebug } : {}),
      ...(templateModelResolvedFrom
        ? { template_model_guid_resolved_from: templateModelResolvedFrom }
        : {}),
      ...(inputFileArgument?.url
        ? { input_file_from_template: true }
        : { input_file_from_template: false }),
      ...(bootstrapInput
        ? {
            input_file_source: {
              project_id: bootstrapInput.sourceProjectId,
              folder_id: bootstrapInput.sourceFolderId,
              item_id: bootstrapInput.sourceItemId,
            },
          }
        : {}),
      folder_id: resolvedFolderId,
      ...(resolvedFolderPath ? { resolved_folder_path: resolvedFolderPath } : {}),
      model_name: modelName,
      enable_worksharing: enableWorksharing,
    },
    raw: submitJson,
  };
}

