import fs from "node:fs";
import path from "node:path";
import { z } from "zod";
import { getDesignAutomationAccessTokenMcp } from "../lib/daWorkitemsMcp.js";

const APS_BASE = "https://developer.api.autodesk.com";
const DEFAULT_TARGET_FOLDER_PATH_CANDIDATES = [
  ["Project Files", "001 - Models", "01 - Model"],
  ["Project Files", "01 - Models", "001 - Model"],
];

type ToolContext = {
  accessToken?: string;
  access_token?: string;
  hubId?: string;
  hub_id?: string;
};

const createRevitCloudModelParams = z.object({
  access_token: z.string().optional(),
  accessToken: z.string().optional(),
  hub_id: z.string().optional(),
  hubId: z.string().optional(),
  project_number: z.string().optional(),
  projectNumber: z.string().optional(),
  project_id: z.string().optional(),
  projectId: z.string().optional(),
  folder_id: z.string().optional(),
  folderId: z.string().optional(),
  model_name: z.string().optional(),
  modelName: z.string().optional(),
  template_key: z.string().optional(),
  templateKey: z.string().optional(),
  template_model_guid: z.string().optional(),
  templateModelGuid: z.string().optional(),
  template_project_guid: z.string().optional(),
  templateProjectGuid: z.string().optional(),
  region: z.enum(["US", "EMEA"]).optional().default("US"),
  enable_worksharing: z.boolean().optional().default(true),
  enableWorksharing: z.boolean().optional(),
  activity_id: z.string().optional(),
  activityId: z.string().optional(),
  dry_run: z.boolean().optional().default(false),
  dryRun: z.boolean().optional(),
});

type CreateRevitCloudModelParams = z.infer<typeof createRevitCloudModelParams>;

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

function asTrimmed(v: unknown): string {
  return typeof v === "string" ? v.trim() : "";
}

function normalizeKey(v: string): string {
  return v.trim().toUpperCase().replace(/\s+/g, "_");
}

function firstProjectToken(name: string): string {
  const token = name.match(/^\s*([A-Za-z0-9._-]+)/)?.[1] ?? "";
  return token.trim().toUpperCase();
}

function inferredProjectNumbersFromName(name: string): string[] {
  const out = new Set<string>();
  const token = firstProjectToken(name);
  if (token) out.add(token);
  const numericTokens = name.match(/\b\d{3,8}\b/g) ?? [];
  for (const n of numericTokens) {
    const normalized = normalizeProjectNumber(n);
    if (normalized) out.add(normalized);
  }
  return Array.from(out);
}

function normalizeProjectNumber(value: string): string {
  return value.trim().toUpperCase();
}

function normalizeProjectIdForAdminJoin(value: string): string {
  const trimmed = value.trim();
  return trimmed.startsWith("b.") ? trimmed.slice(2) : trimmed;
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
  return out.map((v) => v.trim());
}

function normalizeComparableProjectNumber(value: string): string {
  const n = normalizeProjectNumber(value);
  if (!n) return "";
  if (/^\d+$/.test(n)) {
    return n.replace(/^0+/, "") || "0";
  }
  return n;
}

function loadLocalProjectNumbersById(
  hubId: string,
): Map<string, string> {
  const out = new Map<string, string>();
  const hubKey = stripBPrefix(hubId);
  const candidates = [
    path.join(process.cwd(), "..", "admin_projects.csv"),
    path.join(process.cwd(), "admin_projects.csv"),
    path.join(process.cwd(), "..", "..", "admin_projects.csv"),
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
      const headers = parseCsvRow(lines[0]).map((h) => h.toLowerCase().trim());
      const idIdx = headers.findIndex((h) => h === "id");
      const accountIdx = headers.findIndex((h) => h === "account_id");
      const numberIdx = headers.findIndex((h) => h === "job_number");
      if (idIdx < 0 || accountIdx < 0 || numberIdx < 0) continue;
      for (let i = 1; i < lines.length; i += 1) {
        const cols = parseCsvRow(lines[i]);
        if (cols.length <= Math.max(idIdx, accountIdx, numberIdx)) continue;
        const accountId = cols[accountIdx] ?? "";
        const idRaw = cols[idIdx] ?? "";
        const jobNumber = normalizeProjectNumber(cols[numberIdx] ?? "");
        if (!idRaw || !jobNumber) continue;
        if (normalizeProjectIdForAdminJoin(accountId) !== hubKey) continue;
        out.set(normalizeProjectIdForAdminJoin(idRaw), jobNumber);
      }
      if (out.size > 0) return out;
    } catch {
      // continue
    }
  }
  return out;
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

function looksGuid(v: string): boolean {
  const t = v.trim();
  if (!t) return false;
  if (/^[0-9a-fA-F-]{32,40}$/.test(t)) return true;
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

function daBaseUrlMcp(): string {
  const region = (process.env.DA_REGION ?? "us-east").trim().toLowerCase();
  if (region === "emea" || region === "eu") {
    return "https://developer.api.autodesk.com/da/eu/v3";
  }
  if (region === "apac" || region === "asia") {
    return "https://developer.api.autodesk.com/da/apac/v3";
  }
  return "https://developer.api.autodesk.com/da/us-east/v3";
}

function getToken(params: CreateRevitCloudModelParams, context: ToolContext): string {
  const token =
    params.access_token ??
    params.accessToken ??
    context.accessToken ??
    context.access_token;
  if (!token || !token.trim()) {
    throw new Error("access_token is required.");
  }
  return token.trim();
}

function getHubId(params: CreateRevitCloudModelParams, context: ToolContext): string {
  const hubId = params.hub_id ?? params.hubId ?? context.hubId ?? context.hub_id;
  if (!hubId || !hubId.trim()) {
    throw new Error("hub_id is required.");
  }
  return hubId.trim();
}

function toDataUrl(payload: unknown): string {
  return `data:application/json;base64,${Buffer.from(
    JSON.stringify(payload),
    "utf8",
  ).toString("base64")}`;
}

async function getDaActivityParameterNamesMcp(
  daToken: string,
  activityIdRaw: string,
): Promise<string[] | null> {
  const activityId = activityIdRaw.trim();
  if (!activityId) return null;
  try {
    const response = await fetch(
      `${daBaseUrlMcp()}/activities/${encodeURIComponent(activityId)}`,
      {
        method: "GET",
        headers: {
          Authorization: `Bearer ${daToken}`,
          "Content-Type": "application/json",
        },
        cache: "no-store",
      },
    );
    if (!response.ok) return null;
    const json = (await response.json()) as {
      parameters?: Record<string, unknown>;
    };
    if (!json.parameters || typeof json.parameters !== "object") return [];
    return Object.keys(json.parameters);
  } catch {
    return null;
  }
}

function loadTemplates(): RevitCloudTemplate[] {
  const configPath = path.resolve(
    process.cwd(),
    "..",
    "config",
    "revit-cloud-model-templates.json",
  );
  try {
    if (!fs.existsSync(configPath)) return [];
    const raw = fs.readFileSync(configPath, "utf8");
    const parsed = JSON.parse(raw) as { templates?: unknown[] };
    const rows = Array.isArray(parsed.templates) ? parsed.templates : [];
    const normalized = rows
      .map((entry): RevitCloudTemplate | null => {
        if (!entry || typeof entry !== "object" || Array.isArray(entry)) return null;
        const row = entry as Record<string, unknown>;
        const key = normalizeKey(asTrimmed(row.key));
        const label = asTrimmed(row.label) || key;
        if (!key) return null;
        return {
          key,
          label,
          template_model_guid: asTrimmed(row.template_model_guid),
          template_project_guid: asTrimmed(row.template_project_guid),
          template_source_project_id: asTrimmed(row.template_source_project_id),
          template_source_folder_id: asTrimmed(row.template_source_folder_id),
          template_source_folder_urn: asTrimmed(row.template_source_folder_urn),
          template_name_contains: Array.isArray(row.template_name_contains)
            ? row.template_name_contains
                .map((v) => asTrimmed(v))
                .filter((v) => v.length > 0)
            : undefined,
          resolve_template_guid_on_call:
            row.resolve_template_guid_on_call === true,
          default_target_folder_id: asTrimmed(row.default_target_folder_id),
          region: normalizeKey(asTrimmed(row.region)) === "EMEA" ? "EMEA" : "US",
        };
      })
      .filter((v): v is RevitCloudTemplate => Boolean(v));
    return normalized;
  } catch {
    return [];
  }
}

async function listHubProjects(accessToken: string, hubId: string) {
  const localNumberByProjectId = loadLocalProjectNumbersById(hubId);
  const rows: Array<{ id: string; name: string; projectNumbers: string[] }> = [];
  let nextUrl: string | null =
    `${APS_BASE}/project/v1/hubs/${encodeURIComponent(hubId)}/projects`;

  while (nextUrl) {
    const response = await fetch(nextUrl, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    });
    if (!response.ok) {
      const text = await response.text();
      throw new Error(`Project list failed (${response.status}): ${text}`);
    }
    const json = (await response.json()) as {
      data?: Array<{ id?: string; attributes?: { name?: string } }>;
      links?: { next?: { href?: string } };
    };
    rows.push(
      ...(json.data ?? [])
        .map((p) => {
          const id = asTrimmed(p.id);
          const name = asTrimmed(p.attributes?.name);
          const inferredNumbers = inferredProjectNumbersFromName(name);
          const fromLocal = id
            ? localNumberByProjectId.get(normalizeProjectIdForAdminJoin(id)) ?? ""
            : "";
          const projectNumbers = Array.from(
            new Set([fromLocal, ...inferredNumbers].map((v) => normalizeProjectNumber(v)).filter(Boolean)),
          );
          return { id, name, projectNumbers };
        })
        .filter((p) => p.id && p.projectNumbers.length > 0),
    );

    const href = asTrimmed(json.links?.next?.href);
    nextUrl = href || null;
  }

  return rows;
}

async function listFolderChildren(params: {
  accessToken: string;
  projectId: string;
  folderId: string;
}): Promise<Array<{ id: string; name: string }>> {
  const out: Array<{ id: string; name: string }> = [];
  let nextPath: string | null = `/data/v1/projects/${encodeURIComponent(params.projectId)}/folders/${encodeURIComponent(params.folderId)}/contents`;
  while (nextPath) {
    const response = await fetch(`${APS_BASE}${nextPath}`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    });
    if (!response.ok) {
      const text = await response.text();
      throw new Error(`Folder contents failed (${response.status}): ${text}`);
    }
    const json = (await response.json()) as {
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
  const topResponse = await fetch(
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
  if (!topResponse.ok) {
    const text = await topResponse.text();
    throw new Error(`Top folders failed (${topResponse.status}): ${text}`);
  }
  const topJson = (await topResponse.json()) as {
    data?: Array<{ id?: string; attributes?: { name?: string } }>;
  };
  const wantedTop = normalizeFolderName(params.folderPath[0]!);
  const topHit = (topJson.data ?? [])
    .map((row) => ({ id: asTrimmed(row.id), name: asTrimmed(row.attributes?.name) }))
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
    const response = await fetch(`${APS_BASE}${nextPath}`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    });
    if (!response.ok) {
      const text = await response.text();
      throw new Error(`Template folder list failed (${response.status}): ${text}`);
    }
    const json = (await response.json()) as {
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

function parseOssObjectUrn(storageUrn: string): { bucketKey: string; objectKey: string } | null {
  const prefix = "urn:adsk.objects:os.object:";
  if (!storageUrn.startsWith(prefix)) return null;
  const rest = storageUrn.slice(prefix.length);
  const slash = rest.indexOf("/");
  if (slash <= 0 || slash >= rest.length - 1) return null;
  const bucketKey = rest.slice(0, slash);
  const objectKey = rest.slice(slash + 1);
  if (!bucketKey || !objectKey) return null;
  return { bucketKey, objectKey };
}

async function getSignedS3DownloadUrl(params: {
  accessToken: string;
  bucketKey: string;
  objectKey: string;
}): Promise<string> {
  const response = await fetch(
    `${APS_BASE}/oss/v2/buckets/${encodeURIComponent(params.bucketKey)}/objects/${encodeURIComponent(params.objectKey)}/signeds3download`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    },
  );
  if (!response.ok) return "";
  const json = (await response.json()) as { url?: string; signedUrl?: string };
  return asTrimmed(json.url) || asTrimmed(json.signedUrl);
}

async function getItemTipVersionId(params: {
  accessToken: string;
  projectId: string;
  itemId: string;
}): Promise<string> {
  const response = await fetch(
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
  if (!response.ok) return "";
  const json = (await response.json()) as { data?: { id?: string } };
  return asTrimmed(json.data?.id);
}

async function buildVersionOssGetArgument(params: {
  accessToken: string;
  projectId: string;
  versionId: string;
}): Promise<{ url: string; headers?: Record<string, string> } | null> {
  const versionResponse = await fetch(
    `${APS_BASE}/data/v1/projects/${encodeURIComponent(params.projectId)}/versions/${encodeURIComponent(params.versionId)}`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    },
  );
  if (!versionResponse.ok) return null;
  const versionJson = (await versionResponse.json()) as {
    data?: { relationships?: { storage?: { data?: { id?: string } } } };
  };
  const storageUrn = asTrimmed(versionJson.data?.relationships?.storage?.data?.id);
  if (!storageUrn) return null;
  const parsed = parseOssObjectUrn(storageUrn);
  if (!parsed) return null;
  const signedUrl = await getSignedS3DownloadUrl({
    accessToken: params.accessToken,
    bucketKey: parsed.bucketKey,
    objectKey: parsed.objectKey,
  });
  if (!signedUrl) return null;
  return { url: signedUrl, headers: {} };
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
          headers: signed.headers ?? {},
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
  return asTrimmed(o.id) || asTrimmed(o.workitemId) || asTrimmed(o.workItemId);
}

export const createRevitCloudWorksharedModelTool = {
  name: "create_revit_cloud_workshared_model" as const,
  description:
    "Creates a new workshared Revit cloud model in ACC using a configured template and the DA SaveAsCloudModel workflow. Requires selected hub context.",
  parameters: createRevitCloudModelParams,
  async handler(parsed: CreateRevitCloudModelParams, context: ToolContext) {
    const accessToken = getToken(parsed, context);
    const hubId = getHubId(parsed, context);
    const templates = loadTemplates();
    const templateOptions = templates.map((t) => ({ key: t.key, label: t.label }));
    const requestedTemplateKey = normalizeKey(
      asTrimmed(parsed.template_key ?? parsed.templateKey),
    );

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
      asTrimmed(parsed.template_model_guid ?? parsed.templateModelGuid) ||
      asTrimmed(template.template_model_guid);
    let templateModelResolvedFrom: Record<string, unknown> | undefined;
    const shouldResolveTemplateOnCall =
      template.resolve_template_guid_on_call === true ||
      (!templateModelGuid &&
        (Boolean(template.template_source_project_id) ||
          Boolean(template.template_source_folder_id) ||
          Boolean(template.template_source_folder_urn)));
    if (shouldResolveTemplateOnCall) {
      const resolved = await resolveTemplateModelGuidFromSource({
        accessToken,
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

    let projectId = asTrimmed(parsed.project_id ?? parsed.projectId);
    let resolvedProjectName = "";
    const projectNumber = asTrimmed(parsed.project_number ?? parsed.projectNumber).toUpperCase();
    if (!projectId) {
      if (!projectNumber) {
        return { success: false, message: "project_number or project_id is required." };
      }
      const projects = await listHubProjects(accessToken, hubId);
      const targetComparable = normalizeComparableProjectNumber(projectNumber);
      const hit = projects.find((p) =>
        p.projectNumbers.some(
          (n) =>
            n === projectNumber ||
            normalizeComparableProjectNumber(n) === targetComparable,
        ),
      );
      if (!hit) {
        return {
          success: false,
          message: `No project matched project number ${projectNumber} in selected hub.`,
        };
      }
      projectId = hit.id;
      resolvedProjectName = asTrimmed(hit.name);
    }

    const folderId =
      asTrimmed(parsed.folder_id ?? parsed.folderId) ||
      asTrimmed(template.default_target_folder_id);
    const resolvedDefaultFolder = folderId
      ? null
      : await resolveDefaultTargetFolder({
          accessToken,
          hubId,
          projectId,
        });
    const resolvedFolderId = folderId || resolvedDefaultFolder?.folderId || "";
    const resolvedFolderPath = resolvedDefaultFolder?.folderPath;
    if (!resolvedFolderId) {
      return {
        success: false,
        requires_folder_id: true,
        project_id: projectId,
        template: { key: template.key, label: template.label },
        message:
          `Unable to resolve target folder path. Tried: ${DEFAULT_TARGET_FOLDER_PATH_CANDIDATES.map((p) => p.join(" > ")).join(" OR ")}. Provide folder_id or configure default_target_folder_id for this template.`,
      };
    }

    const activityId =
      asTrimmed(parsed.activity_id ?? parsed.activityId) ||
      asTrimmed(process.env.DA_ACTIVITY_ID_CREATE_MODEL) ||
      asTrimmed(process.env.DA_ACTIVITY_ID);
    if (!activityId) {
      return {
        success: false,
        message:
          "No create-model DA activity configured. Set DA_ACTIVITY_ID_CREATE_MODEL.",
      };
    }

    const region = parsed.region ?? template.region ?? "US";
    const enableWorksharing =
      parsed.enableWorksharing ?? parsed.enable_worksharing ?? true;
    const explicitModelName = asTrimmed(parsed.model_name ?? parsed.modelName);
    const inferredYearSuffix = inferRevitYearSuffix(activityId, template);
    const inferredProjectName = sanitizeModelNamePart(resolvedProjectName);
    const inferredFallbackProjectName = sanitizeModelNamePart(projectNumber);
    const defaultModelNameBase =
      inferredProjectName || inferredFallbackProjectName || "New Revit Model";
    const modelName =
      explicitModelName ||
      `${defaultModelNameBase}${inferredYearSuffix}`.trim() ||
      `New Revit Model ${Date.now()}`;
    const targetProjectGuid = stripBPrefix(projectId);
    const templateProjectGuid =
      stripBPrefix(asTrimmed(parsed.template_project_guid ?? parsed.templateProjectGuid)) ||
      stripBPrefix(asTrimmed(template.template_project_guid)) ||
      targetProjectGuid;

    const revitmodel = {
      region,
      projectGuid: templateProjectGuid,
      modelGuid: stripBPrefix(templateModelGuid),
      toolName: "create_model",
      save: true,
    };
    const toolinputs = {
      accountId: stripBPrefix(hubId),
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
      accessToken,
      candidateSources: bootstrapSourceCandidates,
    });
    if (bootstrapInput?.url) {
      inputFileArgument = {
        url: bootstrapInput.url,
        headers: bootstrapInput.headers,
      };
    }
    const payload = {
      activityId,
      arguments: {
        adsk3LeggedToken: accessToken,
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
        revitmodel: { url: toDataUrl(revitmodel) },
        toolinputs: { url: toDataUrl(toolinputs) },
      },
    };

    if (parsed.dry_run || parsed.dryRun) {
      return {
        success: true,
        dry_run: true,
        message: "Dry run only. Create-model payload prepared.",
        payload_preview: payload,
        resolved: {
          template: { key: template.key, label: template.label },
          project_id: projectId,
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

    const daToken = await getDesignAutomationAccessTokenMcp();
    const activityParameterNames = await getDaActivityParameterNamesMcp(
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
    const submit = await fetch(`${daBaseUrlMcp()}/workitems`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${daToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
      cache: "no-store",
    });
    const submitText = await submit.text();
    let submitJson: unknown = submitText;
    try {
      submitJson = submitText ? (JSON.parse(submitText) as unknown) : {};
    } catch {
      // keep raw text
    }
    if (!submit.ok) {
      return {
        success: false,
        message: `DA submit failed (${submit.status}).`,
        error: submitJson,
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
      resolved: {
        template: { key: template.key, label: template.label },
        project_id: projectId,
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
        model_name: modelName,
        enable_worksharing: enableWorksharing,
      },
      raw: submitJson,
    };
  },
};

export async function runCreateRevitCloudWorksharedModel(
  args: unknown,
  context: ToolContext = {},
) {
  const parsed = createRevitCloudModelParams.parse(args);
  return createRevitCloudWorksharedModelTool.handler(parsed, context);
}

