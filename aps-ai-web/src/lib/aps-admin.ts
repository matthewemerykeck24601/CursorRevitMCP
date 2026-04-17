import fs from "node:fs/promises";
import path from "node:path";
import {
  fetchProjectObjectJson,
  getDesignFilesBucketKey,
  uploadProjectJsonObject,
} from "@/lib/aps-oss";

const APS_BASE = "https://developer.api.autodesk.com";

export type AdminProjectMatch = {
  projectId: string;
  projectName: string;
  projectNumber: string;
  businessUnitId?: string;
  businessUnitName?: string;
};

export type AdminUserAssignmentResult = {
  projectId: string;
  projectName: string;
  projectNumber: string;
  email: string;
  ok: boolean;
  status?: number;
  response?: unknown;
  error?: string;
};

type AdminProductAssignment = {
  key: string;
  access: string;
};

const DEFAULT_ADMIN_PRODUCTS: AdminProductAssignment[] = [
  { key: "build", access: "member" },
];

export type AdminRoleCacheSeedResult = {
  success: boolean;
  hubId: string;
  region: "US" | "EMEA";
  role_cache_object_key: string;
  role_count_before: number;
  role_count_after: number;
  roles_added: number;
  projects_scanned: number;
  projects_with_errors: number;
  role_cache_updated: boolean;
  tables_updated: number;
  sample_roles: Array<{ role_name: string; role_id: string }>;
  errors: string[];
  data_connector?: {
    account_id: string;
    request_id?: string;
    job_id?: string;
    status: "seeded" | "pending" | "failed";
  };
  message?: string;
};

export type AdminRoleCacheListResult = {
  success: boolean;
  hubId: string;
  role_cache_object_key: string;
  role_count: number;
  updated_at: string;
  roles: Array<{ role_name: string; role_id: string }>;
};

export type AdminReferenceCacheSeedResult = {
  success: boolean;
  hubId: string;
  project_cache_object_key: string;
  project_count_before: number;
  project_count_after: number;
  projects_added: number;
  folders_project_count: number;
  folders_total_count: number;
  business_unit_project_count: number;
  updated_at: string;
  cache_updated: boolean;
  source: "seed";
  message?: string;
};

export type AdminReferenceCacheStatusResult = {
  success: boolean;
  hubId: string;
  project_cache_object_key: string;
  project_count: number;
  folders_project_count: number;
  folders_total_count: number;
  business_unit_project_count: number;
  business_units: Array<{ business_unit_id: string; business_unit_name?: string; project_count: number }>;
  updated_at: string;
  sample_projects: AdminProjectMatch[];
  sample_folders_by_project: Array<{
    projectId: string;
    folder_count: number;
    sample_folders: Array<{ folder_name: string; folder_id: string }>;
  }>;
  lookup?: {
    project_number?: string;
    project_id?: string;
    folder_name?: string;
    project_match?: AdminProjectMatch;
    business_unit_id?: string;
    business_unit_name?: string;
    folder_id?: string;
  };
};

type RoleCacheFile = {
  hubId: string;
  updatedAt: string;
  rolesByName: Record<string, string>;
};

type HubReferenceCacheFile = {
  hubId: string;
  updatedAt: string;
  projectsByNumber: Record<
    string,
    {
      projectId: string;
      projectName: string;
      projectNumber: string;
      businessUnitId?: string;
      businessUnitName?: string;
    }
  >;
  foldersByProjectId: Record<string, Record<string, string>>;
};

type DmProject = {
  id?: string;
  attributes?: {
    name?: string;
    extension?: {
      type?: string;
    };
  };
};

type DmProjectsResponse = {
  data?: DmProject[];
};

function firstProjectToken(name: string): string {
  const token = name.match(/^\s*([A-Za-z0-9._-]+)/)?.[1] ?? "";
  return token.trim();
}

export function normalizeProjectNumber(value: string): string {
  return value.trim().toUpperCase();
}

function normalizeBusinessUnitName(value: string): string {
  return value.trim().toLowerCase().replace(/\s+/g, " ");
}

function normalizeProjectIdForAdminJoin(value: string): string {
  const trimmed = value.trim();
  return trimmed.startsWith("b.") ? trimmed.slice(2) : trimmed;
}

function normalizeFolderName(value: string): string {
  return value.trim().toLowerCase().replace(/\s+/g, " ");
}

function normalizeEmail(value: string): string {
  return value.trim().toLowerCase();
}

function normalizeProductAssignments(
  products: Array<Record<string, unknown>> | undefined,
): AdminProductAssignment[] {
  const out: AdminProductAssignment[] = [];
  const pushUnique = (keyRaw: string, accessRaw: string) => {
    const key = keyRaw.trim().toLowerCase();
    const access = accessRaw.trim().toLowerCase();
    if (!key || !access) return;
    if (out.some((p) => p.key === key && p.access === access)) return;
    out.push({ key, access });
  };

  for (const product of products ?? []) {
    if (!product || typeof product !== "object") continue;
    const keyCandidate =
      typeof product.key === "string"
        ? product.key
        : typeof product.product === "string"
          ? product.product
          : typeof product.service === "string"
            ? product.service
            : "";
    const accessCandidate =
      typeof product.access === "string"
        ? product.access
        : typeof product.role === "string"
          ? product.role
          : typeof product.level === "string"
            ? product.level
            : "";
    if (keyCandidate && accessCandidate) {
      pushUnique(keyCandidate, accessCandidate);
      continue;
    }
    if (keyCandidate) {
      // Common shorthand from prompts: "enable BUILD service"
      pushUnique(keyCandidate, "member");
    }
  }

  if (out.length === 0) {
    for (const row of DEFAULT_ADMIN_PRODUCTS) {
      pushUnique(row.key, row.access);
    }
  }
  return out;
}

function normalizeRoleName(value: string): string {
  return value.trim().toLowerCase().replace(/\s+/g, " ");
}

function isLikelyRoleName(value: string): boolean {
  const name = normalizeRoleName(value);
  if (!name) return false;
  if (name.includes("@")) return false;
  if (name.length <= 2) return name === "it";
  const roleKeywords = [
    "role",
    "admin",
    "administrator",
    "manager",
    "designer",
    "engineer",
    "coordinator",
    "superintendent",
    "viewer",
    "connector",
    "ssa",
    "lead",
    "director",
  ];
  if (roleKeywords.some((k) => name.includes(k))) return true;
  if (name.includes("/") || name.includes("-") || name.includes("_")) return true;
  return false;
}

function sanitizeSegment(value: string): string {
  return value
    .trim()
    .replace(/[^a-zA-Z0-9._-]+/g, "_")
    .replace(/^_+|_+$/g, "")
    .slice(0, 120);
}

function roleCacheObjectKey(hubId: string): string {
  const safeHub = sanitizeSegment(hubId) || "UnknownHub";
  return `${safeHub}/AdminRoleCache/role-name-map.json`;
}

function referenceCacheObjectKey(hubId: string): string {
  const safeHub = sanitizeSegment(hubId) || "UnknownHub";
  return `${safeHub}/AdminReferenceCache/reference-index.json`;
}

function toDataConnectorAccountId(hubId: string): string {
  const trimmed = hubId.trim();
  return trimmed.startsWith("b.") ? trimmed.slice(2) : trimmed;
}

function roleCacheLocalPath(hubId: string): string {
  const safeHub = sanitizeSegment(hubId) || "UnknownHub";
  return path.join(process.cwd(), ".design-files", safeHub, "AdminRoleCache", "role-name-map.json");
}

function referenceCacheLocalPath(hubId: string): string {
  const safeHub = sanitizeSegment(hubId) || "UnknownHub";
  return path.join(
    process.cwd(),
    ".design-files",
    safeHub,
    "AdminReferenceCache",
    "reference-index.json",
  );
}

async function loadRoleCache(hubId: string): Promise<RoleCacheFile> {
  const empty: RoleCacheFile = {
    hubId,
    updatedAt: new Date(0).toISOString(),
    rolesByName: {},
  };
  const bucket = getDesignFilesBucketKey();
  const objectKey = roleCacheObjectKey(hubId);
  try {
    const data = await fetchProjectObjectJson<RoleCacheFile>(bucket, objectKey);
    if (data && typeof data === "object" && data.rolesByName) {
      return {
        hubId,
        updatedAt:
          typeof data.updatedAt === "string" ? data.updatedAt : new Date().toISOString(),
        rolesByName:
          typeof data.rolesByName === "object" && data.rolesByName
            ? data.rolesByName
            : {},
      };
    }
  } catch {
    // fall through to local
  }
  try {
    const full = roleCacheLocalPath(hubId);
    const raw = await fs.readFile(full, "utf8");
    const data = JSON.parse(raw) as RoleCacheFile;
    if (data && typeof data === "object" && data.rolesByName) {
      return {
        hubId,
        updatedAt:
          typeof data.updatedAt === "string" ? data.updatedAt : new Date().toISOString(),
        rolesByName:
          typeof data.rolesByName === "object" && data.rolesByName
            ? data.rolesByName
            : {},
      };
    }
  } catch {
    // use empty
  }
  return empty;
}

async function saveRoleCache(hubId: string, rolesByName: Record<string, string>) {
  const payload: RoleCacheFile = {
    hubId,
    updatedAt: new Date().toISOString(),
    rolesByName,
  };
  const bucket = getDesignFilesBucketKey();
  const objectKey = roleCacheObjectKey(hubId);
  try {
    await uploadProjectJsonObject(bucket, objectKey, payload);
    return;
  } catch {
    // fallback local write
  }
  const full = roleCacheLocalPath(hubId);
  await fs.mkdir(path.dirname(full), { recursive: true });
  await fs.writeFile(full, JSON.stringify(payload, null, 2), "utf8");
}

function emptyReferenceCache(hubId: string): HubReferenceCacheFile {
  return {
    hubId,
    updatedAt: new Date(0).toISOString(),
    projectsByNumber: {},
    foldersByProjectId: {},
  };
}

function normalizeProjectRefs(
  raw: Record<string, unknown> | undefined,
): HubReferenceCacheFile["projectsByNumber"] {
  const out: HubReferenceCacheFile["projectsByNumber"] = {};
  if (!raw || typeof raw !== "object") return out;
  for (const [key, value] of Object.entries(raw)) {
    if (!value || typeof value !== "object") continue;
    const row = value as {
      projectId?: unknown;
      projectName?: unknown;
      projectNumber?: unknown;
      businessUnitId?: unknown;
      businessUnitName?: unknown;
      business_unit_id?: unknown;
      business_unit_name?: unknown;
    };
    const projectId = typeof row.projectId === "string" ? row.projectId.trim() : "";
    const projectName = typeof row.projectName === "string" ? row.projectName.trim() : "";
    const projectNumberRaw =
      typeof row.projectNumber === "string" ? row.projectNumber.trim() : key.trim();
    const businessUnitId =
      typeof row.businessUnitId === "string" && row.businessUnitId.trim()
        ? row.businessUnitId.trim()
        : typeof row.business_unit_id === "string" && row.business_unit_id.trim()
          ? row.business_unit_id.trim()
          : "";
    const businessUnitName =
      typeof row.businessUnitName === "string" && row.businessUnitName.trim()
        ? row.businessUnitName.trim()
        : typeof row.business_unit_name === "string" && row.business_unit_name.trim()
          ? row.business_unit_name.trim()
          : "";
    const projectNumber = normalizeProjectNumber(projectNumberRaw);
    if (!projectId || !projectName || !projectNumber) continue;
    out[projectNumber] = {
      projectId,
      projectName,
      projectNumber,
      ...(businessUnitId ? { businessUnitId } : {}),
      ...(businessUnitName ? { businessUnitName } : {}),
    };
  }
  return out;
}

function normalizeFolderRefs(
  raw: Record<string, unknown> | undefined,
): HubReferenceCacheFile["foldersByProjectId"] {
  const out: HubReferenceCacheFile["foldersByProjectId"] = {};
  if (!raw || typeof raw !== "object") return out;
  for (const [projectIdRaw, folderMapRaw] of Object.entries(raw)) {
    const projectId = projectIdRaw.trim();
    if (!projectId) continue;
    if (!folderMapRaw || typeof folderMapRaw !== "object") continue;
    const folderMap = folderMapRaw as Record<string, unknown>;
    const normalizedMap: Record<string, string> = {};
    for (const [folderNameRaw, folderIdRaw] of Object.entries(folderMap)) {
      const folderName = normalizeFolderName(folderNameRaw);
      const folderId = typeof folderIdRaw === "string" ? folderIdRaw.trim() : "";
      if (!folderName || !folderId) continue;
      normalizedMap[folderName] = folderId;
    }
    if (Object.keys(normalizedMap).length > 0) {
      out[projectId] = normalizedMap;
    }
  }
  return out;
}

async function loadHubReferenceCache(hubId: string): Promise<HubReferenceCacheFile> {
  const empty = emptyReferenceCache(hubId);
  const bucket = getDesignFilesBucketKey();
  const objectKey = referenceCacheObjectKey(hubId);
  try {
    const data = await fetchProjectObjectJson<HubReferenceCacheFile>(bucket, objectKey);
    if (data && typeof data === "object") {
      const source = data as {
        updatedAt?: unknown;
        projectsByNumber?: Record<string, unknown>;
        foldersByProjectId?: Record<string, unknown>;
      };
      return {
        hubId,
        updatedAt:
          typeof source.updatedAt === "string" && source.updatedAt.trim()
            ? source.updatedAt
            : new Date().toISOString(),
        projectsByNumber: normalizeProjectRefs(source.projectsByNumber),
        foldersByProjectId: normalizeFolderRefs(source.foldersByProjectId),
      };
    }
  } catch {
    // fall through to local
  }
  try {
    const full = referenceCacheLocalPath(hubId);
    const raw = await fs.readFile(full, "utf8");
    const data = JSON.parse(raw) as {
      updatedAt?: unknown;
      projectsByNumber?: Record<string, unknown>;
      foldersByProjectId?: Record<string, unknown>;
    };
    return {
      hubId,
      updatedAt:
        typeof data.updatedAt === "string" && data.updatedAt.trim()
          ? data.updatedAt
          : new Date().toISOString(),
      projectsByNumber: normalizeProjectRefs(data.projectsByNumber),
      foldersByProjectId: normalizeFolderRefs(data.foldersByProjectId),
    };
  } catch {
    return empty;
  }
}

async function saveHubReferenceCache(
  hubId: string,
  projectsByNumber: HubReferenceCacheFile["projectsByNumber"],
  foldersByProjectId: HubReferenceCacheFile["foldersByProjectId"],
) {
  const payload: HubReferenceCacheFile = {
    hubId,
    updatedAt: new Date().toISOString(),
    projectsByNumber,
    foldersByProjectId,
  };
  const bucket = getDesignFilesBucketKey();
  const objectKey = referenceCacheObjectKey(hubId);
  try {
    await uploadProjectJsonObject(bucket, objectKey, payload);
    return;
  } catch {
    // fallback local write
  }
  const full = referenceCacheLocalPath(hubId);
  await fs.mkdir(path.dirname(full), { recursive: true });
  await fs.writeFile(full, JSON.stringify(payload, null, 2), "utf8");
}

function projectsToNumberMap(
  projects: AdminProjectMatch[],
): HubReferenceCacheFile["projectsByNumber"] {
  const out: HubReferenceCacheFile["projectsByNumber"] = {};
  for (const project of projects) {
    const key = normalizeProjectNumber(project.projectNumber);
    if (!key || !project.projectId || !project.projectName) continue;
    out[key] = {
      projectId: project.projectId,
      projectName: project.projectName,
      projectNumber: key,
      ...(project.businessUnitId ? { businessUnitId: project.businessUnitId } : {}),
      ...(project.businessUnitName ? { businessUnitName: project.businessUnitName } : {}),
    };
  }
  return out;
}

function projectsFromNumberMap(
  map: HubReferenceCacheFile["projectsByNumber"],
): AdminProjectMatch[] {
  return Object.values(map)
    .map((row) => ({
      projectId: row.projectId,
      projectName: row.projectName,
      projectNumber: row.projectNumber,
      ...(row.businessUnitId ? { businessUnitId: row.businessUnitId } : {}),
      ...(row.businessUnitName ? { businessUnitName: row.businessUnitName } : {}),
    }))
    .filter((row) => row.projectId && row.projectName && row.projectNumber);
}

export async function upsertHubReferenceProjects(params: {
  hubId: string;
  projects: AdminProjectMatch[];
}): Promise<{
  success: boolean;
  updated: boolean;
  project_cache_object_key: string;
  project_count_before: number;
  project_count_after: number;
}> {
  const hubId = params.hubId.trim();
  if (!hubId) throw new Error("hubId is required.");
  const cache = await loadHubReferenceCache(hubId);
  const projectsByNumber: HubReferenceCacheFile["projectsByNumber"] = {
    ...cache.projectsByNumber,
  };
  const before = Object.keys(projectsByNumber).length;
  let changed = false;
  for (const project of params.projects) {
    const projectNumber = normalizeProjectNumber(project.projectNumber);
    const projectId = String(project.projectId ?? "").trim();
    const projectName = String(project.projectName ?? "").trim();
    if (!projectNumber || !projectId || !projectName) continue;
    const existing = projectsByNumber[projectNumber];
    const nextRow = {
      projectId,
      projectName,
      projectNumber,
      ...(project.businessUnitId
        ? { businessUnitId: project.businessUnitId }
        : existing?.businessUnitId
          ? { businessUnitId: existing.businessUnitId }
          : {}),
      ...(project.businessUnitName
        ? { businessUnitName: project.businessUnitName }
        : existing?.businessUnitName
          ? { businessUnitName: existing.businessUnitName }
          : {}),
    };
    if (
      !existing ||
      existing.projectId !== nextRow.projectId ||
      existing.projectName !== nextRow.projectName ||
      (existing.businessUnitId ?? "") !== (nextRow.businessUnitId ?? "") ||
      (existing.businessUnitName ?? "") !== (nextRow.businessUnitName ?? "")
    ) {
      projectsByNumber[projectNumber] = nextRow;
      changed = true;
    }
  }
  if (changed || before === 0) {
    await saveHubReferenceCache(hubId, projectsByNumber, cache.foldersByProjectId);
  }
  return {
    success: true,
    updated: changed || before === 0,
    project_cache_object_key: referenceCacheObjectKey(hubId),
    project_count_before: before,
    project_count_after: Object.keys(projectsByNumber).length,
  };
}

export async function getHubProjectsWithCache(params: {
  accessToken: string;
  hubId: string;
  requestedProjectNumbers: string[];
  cacheOnly?: boolean;
}): Promise<{
  projectsByNumber: Map<string, AdminProjectMatch>;
  cache_updated: boolean;
  project_cache_object_key: string;
  cache_source: "cache" | "cache+live-refresh" | "cache-only-miss";
}> {
  const requested = Array.from(
    new Set(params.requestedProjectNumbers.map((p) => normalizeProjectNumber(p)).filter(Boolean)),
  );
  const cache = await loadHubReferenceCache(params.hubId);
  const buBackfill = await backfillBusinessUnitsFromLocalCsv(cache.projectsByNumber);
  const cacheMap: HubReferenceCacheFile["projectsByNumber"] = {
    ...buBackfill.projectsByNumber,
  };
  if (buBackfill.updated) {
    await saveHubReferenceCache(params.hubId, cacheMap, cache.foldersByProjectId);
  }

  const misses = requested.filter((n) => !cacheMap[n]);
  if (params.cacheOnly && misses.length > 0) {
    const map = new Map(
      projectsFromNumberMap(cacheMap).map((p) => [normalizeProjectNumber(p.projectNumber), p]),
    );
    return {
      projectsByNumber: map,
      cache_updated: false,
      project_cache_object_key: referenceCacheObjectKey(params.hubId),
      cache_source: "cache-only-miss",
    };
  }
  if (misses.length > 0 || Object.keys(cacheMap).length === 0) {
    const live = await listHubProjects(params.accessToken, params.hubId);
    const liveMap = projectsToNumberMap(live);
    let changed = false;
    for (const [projectNumber, row] of Object.entries(liveMap)) {
      const existing = cacheMap[projectNumber];
      const nextRow = {
        ...row,
        ...(row.businessUnitId ? {} : existing?.businessUnitId ? { businessUnitId: existing.businessUnitId } : {}),
        ...(row.businessUnitName ? {} : existing?.businessUnitName ? { businessUnitName: existing.businessUnitName } : {}),
      };
      if (
        !existing ||
        existing.projectId !== nextRow.projectId ||
        existing.projectName !== nextRow.projectName ||
        (existing.businessUnitId ?? "") !== (nextRow.businessUnitId ?? "") ||
        (existing.businessUnitName ?? "") !== (nextRow.businessUnitName ?? "")
      ) {
        cacheMap[projectNumber] = nextRow;
        changed = true;
      }
    }
    if (changed || Object.keys(cache.projectsByNumber).length === 0) {
      await saveHubReferenceCache(params.hubId, cacheMap, cache.foldersByProjectId);
    }
    const map = new Map(
      projectsFromNumberMap(cacheMap).map((p) => [normalizeProjectNumber(p.projectNumber), p]),
    );
    return {
      projectsByNumber: map,
      cache_updated: changed || Object.keys(cache.projectsByNumber).length === 0,
      project_cache_object_key: referenceCacheObjectKey(params.hubId),
      cache_source: "cache+live-refresh",
    };
  }

  const map = new Map(
    projectsFromNumberMap(cacheMap).map((p) => [normalizeProjectNumber(p.projectNumber), p]),
  );
  return {
    projectsByNumber: map,
    cache_updated: false,
    project_cache_object_key: referenceCacheObjectKey(params.hubId),
    cache_source: "cache",
  };
}

export async function upsertHubReferenceFolders(params: {
  hubId: string;
  projectId: string;
  folders: Array<{ folderId: string; folderName: string }>;
}): Promise<{
  success: boolean;
  updated: boolean;
  project_cache_object_key: string;
  folder_count: number;
}> {
  const hubId = params.hubId.trim();
  const projectId = params.projectId.trim();
  if (!hubId || !projectId) {
    throw new Error("hubId and projectId are required.");
  }
  const cache = await loadHubReferenceCache(hubId);
  const foldersByProjectId: HubReferenceCacheFile["foldersByProjectId"] = {
    ...cache.foldersByProjectId,
  };
  const projectFolderMap: Record<string, string> = {
    ...(foldersByProjectId[projectId] ?? {}),
  };

  let changed = false;
  for (const folder of params.folders) {
    const folderId = String(folder.folderId ?? "").trim();
    const folderName = normalizeFolderName(String(folder.folderName ?? ""));
    if (!folderId || !folderName) continue;
    if (projectFolderMap[folderName] !== folderId) {
      projectFolderMap[folderName] = folderId;
      changed = true;
    }
  }
  if (Object.keys(projectFolderMap).length > 0) {
    if (!foldersByProjectId[projectId]) changed = true;
    foldersByProjectId[projectId] = projectFolderMap;
  }

  if (changed) {
    await saveHubReferenceCache(hubId, cache.projectsByNumber, foldersByProjectId);
  }
  return {
    success: true,
    updated: changed,
    project_cache_object_key: referenceCacheObjectKey(hubId),
    folder_count: Object.keys(projectFolderMap).length,
  };
}

function countFoldersInCache(
  foldersByProjectId: HubReferenceCacheFile["foldersByProjectId"],
): { folders_project_count: number; folders_total_count: number } {
  const projectIds = Object.keys(foldersByProjectId);
  const folders_total_count = projectIds.reduce(
    (sum, projectId) => sum + Object.keys(foldersByProjectId[projectId] ?? {}).length,
    0,
  );
  return {
    folders_project_count: projectIds.length,
    folders_total_count,
  };
}

export async function seedHubReferenceCache(params: {
  accessToken: string;
  hubId: string;
}): Promise<AdminReferenceCacheSeedResult> {
  const cache = await loadHubReferenceCache(params.hubId);
  const beforeCount = Object.keys(cache.projectsByNumber).length;
  const liveProjects = await listHubProjects(params.accessToken, params.hubId);
  const buIndex = await loadLocalProjectBusinessUnitIndex();
  const enrichedProjects = liveProjects.map((project) => {
    const byProjectId = buIndex.byProjectId.get(
      normalizeProjectIdForAdminJoin(project.projectId),
    );
    const byProjectNumber = buIndex.byProjectNumber.get(
      normalizeProjectNumber(project.projectNumber),
    );
    const bu = byProjectId ?? byProjectNumber;
    return {
      ...project,
      ...(bu?.businessUnitId ? { businessUnitId: bu.businessUnitId } : {}),
      ...(bu?.businessUnitName ? { businessUnitName: bu.businessUnitName } : {}),
    };
  });
  const liveMap = projectsToNumberMap(enrichedProjects);
  const nextProjectsByNumber: HubReferenceCacheFile["projectsByNumber"] = {
    ...cache.projectsByNumber,
  };
  let changed = false;
  for (const [projectNumber, row] of Object.entries(liveMap)) {
    const existing = nextProjectsByNumber[projectNumber];
    if (
      !existing ||
      existing.projectId !== row.projectId ||
      existing.projectName !== row.projectName
    ) {
      nextProjectsByNumber[projectNumber] = row;
      changed = true;
    }
  }
  const afterCount = Object.keys(nextProjectsByNumber).length;
  if (changed || beforeCount === 0) {
    await saveHubReferenceCache(params.hubId, nextProjectsByNumber, cache.foldersByProjectId);
  }
  const folderCounts = countFoldersInCache(cache.foldersByProjectId);
  const businessUnitProjectCount = Object.values(nextProjectsByNumber).filter(
    (p) => Boolean(p.businessUnitId),
  ).length;
  return {
    success: true,
    hubId: params.hubId,
    project_cache_object_key: referenceCacheObjectKey(params.hubId),
    project_count_before: beforeCount,
    project_count_after: afterCount,
    projects_added: Math.max(0, afterCount - beforeCount),
    folders_project_count: folderCounts.folders_project_count,
    folders_total_count: folderCounts.folders_total_count,
    business_unit_project_count: businessUnitProjectCount,
    updated_at: new Date().toISOString(),
    cache_updated: changed || beforeCount === 0,
    source: "seed",
    ...(changed || beforeCount === 0
      ? {}
      : { message: "Reference cache already up-to-date for known projects." }),
  };
}

export async function getHubReferenceCacheStatus(params: {
  hubId: string;
  projectNumber?: string;
  projectId?: string;
  businessUnitId?: string;
  businessUnitName?: string;
  folderName?: string;
  limit?: number;
}): Promise<AdminReferenceCacheStatusResult> {
  const cache = await loadHubReferenceCache(params.hubId);
  const buBackfill = await backfillBusinessUnitsFromLocalCsv(cache.projectsByNumber);
  const projectsByNumber = buBackfill.projectsByNumber;
  if (buBackfill.updated) {
    await saveHubReferenceCache(
      params.hubId,
      projectsByNumber,
      cache.foldersByProjectId,
    );
  }
  const allProjects = projectsFromNumberMap(projectsByNumber);
  const projectByNumber = new Map(
    allProjects.map((p) => [normalizeProjectNumber(p.projectNumber), p]),
  );
  const projectById = new Map(allProjects.map((p) => [p.projectId, p]));
  const limitRaw = Number(params.limit);
  const limit =
    Number.isFinite(limitRaw) && limitRaw > 0
      ? Math.min(500, Math.trunc(limitRaw))
      : 100;
  const folderCounts = countFoldersInCache(cache.foldersByProjectId);
  const businessUnitIdFilter = params.businessUnitId?.trim() ?? "";
  const businessUnitNameFilter = params.businessUnitName
    ? normalizeBusinessUnitName(params.businessUnitName)
    : "";
  const projectsFilteredByBu =
    businessUnitIdFilter || businessUnitNameFilter
      ? allProjects.filter((p) => {
          const byId = businessUnitIdFilter
            ? (p.businessUnitId ?? "") === businessUnitIdFilter
            : true;
          const byName = businessUnitNameFilter
            ? normalizeBusinessUnitName(p.businessUnitName ?? "") === businessUnitNameFilter
            : true;
          return byId && byName;
        })
      : allProjects;
  const buCounter = new Map<string, { name?: string; count: number }>();
  for (const project of allProjects) {
    const id = project.businessUnitId ?? "";
    if (!id) continue;
    const prev = buCounter.get(id);
    buCounter.set(id, {
      name: project.businessUnitName ?? prev?.name,
      count: (prev?.count ?? 0) + 1,
    });
  }
  const businessUnits = Array.from(buCounter.entries())
    .map(([business_unit_id, value]) => ({
      business_unit_id,
      ...(value.name ? { business_unit_name: value.name } : {}),
      project_count: value.count,
    }))
    .sort((a, b) => b.project_count - a.project_count || a.business_unit_id.localeCompare(b.business_unit_id));
  const businessUnitProjectCount = projectsFilteredByBu.filter((p) => Boolean(p.businessUnitId)).length;

  const sampleProjects = projectsFilteredByBu.slice(0, limit);
  const sampleFoldersByProject = Object.entries(cache.foldersByProjectId)
    .slice(0, Math.min(limit, 50))
    .map(([projectId, folderMap]) => {
      const entries = Object.entries(folderMap ?? {});
      return {
        projectId,
        folder_count: entries.length,
        sample_folders: entries.slice(0, 20).map(([folder_name, folder_id]) => ({
          folder_name,
          folder_id,
        })),
      };
    });

  const normalizedProjectNumber = params.projectNumber
    ? normalizeProjectNumber(params.projectNumber)
    : "";
  const normalizedFolderName = params.folderName
    ? normalizeFolderName(params.folderName)
    : "";
  const lookupProject =
    (normalizedProjectNumber ? projectByNumber.get(normalizedProjectNumber) : undefined) ??
    (params.projectId ? projectById.get(params.projectId.trim()) : undefined);
  const lookupProjectId = lookupProject?.projectId ?? params.projectId?.trim() ?? "";
  const folder_id =
    normalizedFolderName && lookupProjectId
      ? cache.foldersByProjectId[lookupProjectId]?.[normalizedFolderName]
      : undefined;

  return {
    success: true,
    hubId: params.hubId,
    project_cache_object_key: referenceCacheObjectKey(params.hubId),
    project_count: allProjects.length,
    folders_project_count: folderCounts.folders_project_count,
    folders_total_count: folderCounts.folders_total_count,
    business_unit_project_count: businessUnitProjectCount,
    business_units: businessUnits.slice(0, limit),
    updated_at: cache.updatedAt,
    sample_projects: sampleProjects,
    sample_folders_by_project: sampleFoldersByProject,
    ...((params.projectNumber || params.projectId || params.folderName || params.businessUnitId || params.businessUnitName)
      ? {
          lookup: {
            ...(normalizedProjectNumber
              ? { project_number: normalizedProjectNumber }
              : {}),
            ...(lookupProjectId ? { project_id: lookupProjectId } : {}),
            ...(normalizedFolderName ? { folder_name: normalizedFolderName } : {}),
            ...(lookupProject ? { project_match: lookupProject } : {}),
            ...(lookupProject?.businessUnitId
              ? { business_unit_id: lookupProject.businessUnitId }
              : {}),
            ...(lookupProject?.businessUnitName
              ? { business_unit_name: lookupProject.businessUnitName }
              : {}),
            ...(folder_id ? { folder_id } : {}),
          },
        }
      : {}),
  };
}

function mapFromRoleCache(cache: RoleCacheFile): Map<string, string> {
  const out = new Map<string, string>();
  for (const [name, id] of Object.entries(cache.rolesByName ?? {})) {
    const normalized = normalizeRoleName(name);
    if (!normalized) continue;
    if (!isLikelyRoleName(normalized)) continue;
    if (typeof id !== "string" || !id.trim()) continue;
    out.set(normalized, id.trim());
  }
  return out;
}

type DataConnectorRequest = {
  id?: string;
  serviceGroups?: string[];
  scheduleInterval?: string;
  isActive?: boolean;
  createdAt?: string;
  updatedAt?: string;
};

type DataConnectorRequestsResponse = {
  results?: DataConnectorRequest[];
};

type DataConnectorJob = {
  id?: string;
  status?: string;
  completionStatus?: string;
  createdAt?: string;
  updatedAt?: string;
};

type DataConnectorJobsResponse = {
  results?: DataConnectorJob[];
};

type DataConnectorSignedData = {
  signedUrl?: string;
  signedHeaders?: Record<string, string>;
};

function compareIsoDesc(a?: string, b?: string): number {
  const ta = a ? Date.parse(a) : 0;
  const tb = b ? Date.parse(b) : 0;
  return tb - ta;
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

function extractRolesFromCsv(csv: string): Map<string, string> {
  const lines = csv
    .replace(/^\uFEFF/, "")
    .split(/\r?\n/)
    .map((l) => l.trim())
    .filter((l) => l.length > 0);
  const out = new Map<string, string>();
  if (lines.length < 2) return out;

  const headers = parseCsvRow(lines[0]).map((h) => h.toLowerCase().trim());
  const findHeaderIndex = (aliases: string[]) =>
    headers.findIndex((h) => aliases.some((a) => h === a || h.endsWith(`.${a}`)));
  const roleIdIdx = findHeaderIndex([
    "role_id",
    "roleid",
    "project_role_id",
    "projectroleid",
  ]);
  const roleNameIdx = findHeaderIndex([
    "role_name",
    "rolename",
    "name",
    "project_role_name",
    "projectrolename",
  ]);
  if (roleIdIdx < 0 || roleNameIdx < 0) return out;

  for (let i = 1; i < lines.length; i += 1) {
    const cols = parseCsvRow(lines[i]);
    if (cols.length <= Math.max(roleIdIdx, roleNameIdx)) continue;
    const roleId = cols[roleIdIdx]?.trim() ?? "";
    const roleName = normalizeRoleName(cols[roleNameIdx] ?? "");
    if (!roleId || !roleName) continue;
    if (!isLikelyRoleName(roleName)) continue;
    out.set(roleName, roleId);
  }
  return out;
}

async function listDataConnectorRequests(params: {
  accessToken: string;
  accountId: string;
  region: "US" | "EMEA";
}): Promise<DataConnectorRequest[]> {
  const response = await fetch(
    `${APS_BASE}/data-connector/v1/accounts/${encodeURIComponent(params.accountId)}/requests`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
        Region: params.region,
      },
      cache: "no-store",
    },
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Data Connector requests failed (${response.status}): ${text}`);
  }
  const json = (await response.json()) as DataConnectorRequestsResponse;
  return Array.isArray(json.results) ? json.results : [];
}

async function createAdminDataConnectorRequest(params: {
  accessToken: string;
  accountId: string;
  region: "US" | "EMEA";
}): Promise<string> {
  const response = await fetch(
    `${APS_BASE}/data-connector/v1/accounts/${encodeURIComponent(params.accountId)}/requests`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
        Region: params.region,
      },
      body: JSON.stringify({
        description: `Role cache seed ${new Date().toISOString()}`,
        isActive: true,
        scheduleInterval: "ONE_TIME",
        serviceGroups: ["admin"],
        effectiveFrom: new Date().toISOString(),
      }),
      cache: "no-store",
    },
  );
  const text = await response.text();
  let parsed: unknown = text;
  try {
    parsed = text ? (JSON.parse(text) as unknown) : {};
  } catch {
    // keep text
  }
  if (!response.ok) {
    throw new Error(
      `Data Connector create request failed (${response.status}): ${
        typeof parsed === "string" ? parsed : JSON.stringify(parsed)
      }`,
    );
  }
  const maybe = parsed as { id?: unknown };
  const requestId = typeof maybe.id === "string" ? maybe.id.trim() : "";
  if (!requestId) throw new Error("Data Connector create request did not return id.");
  return requestId;
}

async function listDataConnectorRequestJobs(params: {
  accessToken: string;
  accountId: string;
  requestId: string;
  region: "US" | "EMEA";
}): Promise<DataConnectorJob[]> {
  const response = await fetch(
    `${APS_BASE}/data-connector/v1/accounts/${encodeURIComponent(params.accountId)}/requests/${encodeURIComponent(params.requestId)}/jobs`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
        Region: params.region,
      },
      cache: "no-store",
    },
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Data Connector request jobs failed (${response.status}): ${text}`);
  }
  const json = (await response.json()) as DataConnectorJobsResponse;
  return Array.isArray(json.results) ? json.results : [];
}

async function getDataConnectorJobDataListing(params: {
  accessToken: string;
  accountId: string;
  jobId: string;
  region: "US" | "EMEA";
}): Promise<Array<Record<string, unknown>>> {
  const response = await fetch(
    `${APS_BASE}/data-connector/v1/accounts/${encodeURIComponent(params.accountId)}/jobs/${encodeURIComponent(params.jobId)}/data-listing`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${params.accessToken}`,
        "Content-Type": "application/json",
        Region: params.region,
      },
      cache: "no-store",
    },
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Data Connector data-listing failed (${response.status}): ${text}`);
  }
  const json = (await response.json()) as unknown;
  return Array.isArray(json) ? (json as Array<Record<string, unknown>>) : [];
}

async function getDataConnectorSignedData(params: {
  accessToken: string;
  accountId: string;
  jobId: string;
  dataName: string;
  region: "US" | "EMEA";
}): Promise<DataConnectorSignedData> {
  const rawDataName = params.dataName.trim();
  const normalized = normalizeDataRef(rawDataName);
  const encodedWhole = encodeURIComponent(normalized);
  const encodedBySegment = normalized
    .split("/")
    .filter(Boolean)
    .map((segment) => encodeURIComponent(segment))
    .join("/");
  const dataPathCandidates = Array.from(
    new Set(
      [normalized, encodedBySegment, encodedWhole].filter(
        (v) => typeof v === "string" && v.trim().length > 0,
      ),
    ),
  );

  let lastStatus = 0;
  let lastText = "No response body";
  for (const dataPath of dataPathCandidates) {
    const response = await fetch(
      `${APS_BASE}/data-connector/v1/accounts/${encodeURIComponent(params.accountId)}/jobs/${encodeURIComponent(params.jobId)}/data/${dataPath}`,
      {
        method: "GET",
        headers: {
          Authorization: `Bearer ${params.accessToken}`,
          "Content-Type": "application/json",
          Region: params.region,
        },
        cache: "no-store",
      },
    );
    if (response.ok) {
      return (await response.json()) as DataConnectorSignedData;
    }
    lastStatus = response.status;
    lastText = await response.text();
  }
  throw new Error(
    `Data Connector data signed-url failed (${lastStatus}): ${lastText} (dataName=${normalized}; tried=${dataPathCandidates.join(",")})`,
  );
}

function normalizeDataRef(value: string): string {
  return value.replace(/^\/*/, "").trim();
}

function dataRefCandidates(value: string): string[] {
  const raw = normalizeDataRef(value);
  if (!raw) return [];
  const out = new Set<string>();
  out.add(raw);
  const decoded = (() => {
    try {
      return decodeURIComponent(raw);
    } catch {
      return raw;
    }
  })();
  out.add(decoded);
  const baseRaw = raw.split("/").filter(Boolean).at(-1) ?? "";
  const baseDecoded = decoded.split("/").filter(Boolean).at(-1) ?? "";
  if (baseRaw) out.add(baseRaw);
  if (baseDecoded) out.add(baseDecoded);
  return Array.from(out).filter(Boolean);
}

function collectListingStringValues(
  value: unknown,
  depth = 0,
  out: Set<string> = new Set(),
): Set<string> {
  if (depth > 4 || value == null) return out;
  if (typeof value === "string") {
    const trimmed = value.trim();
    if (trimmed) out.add(trimmed);
    return out;
  }
  if (typeof value === "number" || typeof value === "boolean") {
    out.add(String(value));
    return out;
  }
  if (Array.isArray(value)) {
    for (const item of value) collectListingStringValues(item, depth + 1, out);
    return out;
  }
  if (typeof value === "object") {
    for (const item of Object.values(value as Record<string, unknown>)) {
      collectListingStringValues(item, depth + 1, out);
    }
  }
  return out;
}

function roleCsvRefsFromListing(
  listing: Array<Record<string, unknown>>,
): Array<{ displayName: string; refs: string[] }> {
  const out: Array<{ displayName: string; refs: string[] }> = [];
  const seen = new Set<string>();
  for (const entry of listing) {
    const candidates = Array.from(collectListingStringValues(entry)).filter(Boolean);
    if (candidates.length === 0) continue;
    const joined = candidates.join(" | ").toLowerCase();
    if (
      !joined.includes("admin_project_roles") ||
      (!joined.includes(".csv") && !joined.includes(".csv.gz"))
    ) {
      continue;
    }
    const refs = Array.from(
      new Set(candidates.flatMap((candidate) => dataRefCandidates(candidate))),
    );
    const marker = refs.join("||");
    if (seen.has(marker)) continue;
    seen.add(marker);
    out.push({
      displayName: refs[0],
      refs,
    });
  }
  return out;
}

async function fetchSignedCsv(signed: DataConnectorSignedData): Promise<string> {
  if (!signed.signedUrl) throw new Error("Missing signedUrl for Data Connector data.");
  const response = await fetch(signed.signedUrl, {
    method: "GET",
    headers: signed.signedHeaders ?? {},
    cache: "no-store",
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Signed CSV download failed (${response.status}): ${text}`);
  }
  return await response.text();
}

async function loadLocalCsvByName(fileName: string): Promise<string | null> {
  const candidates = [
    path.join(process.cwd(), fileName),
    path.join(process.cwd(), "..", fileName),
  ];
  for (const file of candidates) {
    try {
      const text = await fs.readFile(file, "utf8");
      if (text.trim()) return text;
    } catch {
      // continue
    }
  }
  return null;
}

async function loadLocalProjectBusinessUnitIndex(): Promise<{
  byProjectId: Map<
    string,
    { businessUnitId: string; businessUnitName?: string; projectNumber?: string }
  >;
  byProjectNumber: Map<
    string,
    { businessUnitId: string; businessUnitName?: string; projectNumber?: string }
  >;
}> {
  const projectsCsv = await loadLocalCsvByName("admin_projects.csv");
  const businessUnitsCsv = await loadLocalCsvByName("admin_business_units.csv");
  const byProjectId = new Map<
    string,
    { businessUnitId: string; businessUnitName?: string; projectNumber?: string }
  >();
  const byProjectNumber = new Map<
    string,
    { businessUnitId: string; businessUnitName?: string; projectNumber?: string }
  >();
  if (!projectsCsv) return { byProjectId, byProjectNumber };

  const buNameById = new Map<string, string>();
  if (businessUnitsCsv) {
    const buLines = businessUnitsCsv
      .replace(/^\uFEFF/, "")
      .split(/\r?\n/)
      .map((l) => l.trim())
      .filter((l) => l.length > 0);
    if (buLines.length >= 2) {
      const headers = parseCsvRow(buLines[0]).map((h) => h.toLowerCase().trim());
      const idIdx = headers.findIndex((h) => h === "id");
      const nameIdx = headers.findIndex((h) => h === "name");
      for (let i = 1; i < buLines.length; i += 1) {
        const cols = parseCsvRow(buLines[i]);
        if (idIdx < 0 || nameIdx < 0) break;
        if (cols.length <= Math.max(idIdx, nameIdx)) continue;
        const id = cols[idIdx]?.trim() ?? "";
        const name = cols[nameIdx]?.trim() ?? "";
        if (!id || !name) continue;
        buNameById.set(id, name);
      }
    }
  }

  const lines = projectsCsv
    .replace(/^\uFEFF/, "")
    .split(/\r?\n/)
    .map((l) => l.trim())
    .filter((l) => l.length > 0);
  if (lines.length < 2) return { byProjectId, byProjectNumber };
  const headers = parseCsvRow(lines[0]).map((h) => h.toLowerCase().trim());
  const idIdx = headers.findIndex((h) => h === "id");
  const nameIdx = headers.findIndex((h) => h === "name");
  const jobNumberIdx = headers.findIndex((h) => h === "job_number");
  const buIdIdx = headers.findIndex((h) => h === "business_unit_id");
  if (idIdx < 0 || buIdIdx < 0) return { byProjectId, byProjectNumber };

  for (let i = 1; i < lines.length; i += 1) {
    const cols = parseCsvRow(lines[i]);
    if (cols.length <= Math.max(idIdx, buIdIdx)) continue;
    const projectIdRaw = cols[idIdx]?.trim() ?? "";
    const businessUnitId = cols[buIdIdx]?.trim() ?? "";
    if (!projectIdRaw || !businessUnitId) continue;
    const projectId = normalizeProjectIdForAdminJoin(projectIdRaw);
    const projectName = nameIdx >= 0 && cols.length > nameIdx ? cols[nameIdx]?.trim() ?? "" : "";
    const fromJobNumber =
      jobNumberIdx >= 0 && cols.length > jobNumberIdx ? cols[jobNumberIdx]?.trim() ?? "" : "";
    const projectNumber = normalizeProjectNumber(
      fromJobNumber || firstProjectToken(projectName),
    );
    const row = {
      businessUnitId,
      ...(projectNumber ? { projectNumber } : {}),
      ...(buNameById.get(businessUnitId)
        ? { businessUnitName: buNameById.get(businessUnitId) }
        : {}),
    };
    if (projectId) byProjectId.set(projectId, row);
    if (projectNumber) byProjectNumber.set(projectNumber, row);
  }
  return { byProjectId, byProjectNumber };
}

async function backfillBusinessUnitsFromLocalCsv(
  projectsByNumber: HubReferenceCacheFile["projectsByNumber"],
): Promise<{
  projectsByNumber: HubReferenceCacheFile["projectsByNumber"];
  updated: boolean;
}> {
  const index = await loadLocalProjectBusinessUnitIndex();
  if (index.byProjectId.size === 0 && index.byProjectNumber.size === 0) {
    return { projectsByNumber, updated: false };
  }
  const next: HubReferenceCacheFile["projectsByNumber"] = {
    ...projectsByNumber,
  };
  let updated = false;
  for (const [projectNumber, project] of Object.entries(projectsByNumber)) {
    const byId = index.byProjectId.get(
      normalizeProjectIdForAdminJoin(project.projectId ?? ""),
    );
    const byNumber = index.byProjectNumber.get(
      normalizeProjectNumber(projectNumber),
    );
    const bu = byId ?? byNumber;
    if (!bu?.businessUnitId) continue;
    const currentBuId = project.businessUnitId ?? "";
    const currentBuName = project.businessUnitName ?? "";
    const nextBuId = bu.businessUnitId;
    const nextBuName = bu.businessUnitName ?? currentBuName;
    if (currentBuId !== nextBuId || currentBuName !== nextBuName) {
      next[projectNumber] = {
        ...project,
        businessUnitId: nextBuId,
        ...(nextBuName ? { businessUnitName: nextBuName } : {}),
      };
      updated = true;
    }
  }
  return { projectsByNumber: next, updated };
}

async function loadLocalRoleCsvFallback(): Promise<string | null> {
  return loadLocalCsvByName("admin_project_roles.csv");
}

export async function listHubProjects(
  accessToken: string,
  hubId: string,
): Promise<AdminProjectMatch[]> {
  const rows: AdminProjectMatch[] = [];
  const localIndex = await loadLocalProjectBusinessUnitIndex();
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
      throw new Error(
        `Failed loading hub projects (${response.status}): ${text || "empty response"}`,
      );
    }

    const json = (await response.json()) as DmProjectsResponse & {
      links?: { next?: { href?: string } };
    };
    for (const p of json.data ?? []) {
      const projectId = typeof p.id === "string" ? p.id.trim() : "";
      const projectName =
        typeof p.attributes?.name === "string" ? p.attributes.name.trim() : "";
      if (!projectId || !projectName) continue;
      const local = localIndex.byProjectId.get(
        normalizeProjectIdForAdminJoin(projectId),
      );
      const projectNumber =
        local?.projectNumber || firstProjectToken(projectName);
      if (!projectNumber) continue;
      rows.push({
        projectId,
        projectName,
        projectNumber,
        ...(local?.businessUnitId ? { businessUnitId: local.businessUnitId } : {}),
        ...(local?.businessUnitName ? { businessUnitName: local.businessUnitName } : {}),
      });
    }

    const href = json.links?.next?.href?.trim() ?? "";
    nextUrl = href || null;
  }
  return rows;
}

async function postProjectUser(
  accessToken: string,
  projectId: string,
  body: Record<string, unknown>,
  region: "US" | "EMEA",
): Promise<{ ok: boolean; status: number; response?: unknown; error?: string }> {
  const candidates = projectId.startsWith("b.")
    ? [projectId, projectId.slice(2)]
    : [projectId];
  let lastError = "Unknown error";
  let lastStatus = 500;

  for (const candidate of candidates) {
    const response = await fetch(
      `${APS_BASE}/construction/admin/v1/projects/${encodeURIComponent(candidate)}/users`,
      {
        method: "POST",
        headers: {
          Authorization: `Bearer ${accessToken}`,
          "Content-Type": "application/json",
          Region: region,
        },
        body: JSON.stringify(body),
        cache: "no-store",
      },
    );
    const text = await response.text();
    let parsed: unknown = text;
    try {
      parsed = text ? (JSON.parse(text) as unknown) : {};
    } catch {
      // keep raw text
    }
    if (response.ok) {
      return { ok: true, status: response.status, response: parsed };
    }
    lastStatus = response.status;
    lastError =
      typeof parsed === "string"
        ? parsed
        : `API error (${response.status}): ${JSON.stringify(parsed)}`;
    if (response.status !== 404) break;
  }
  return { ok: false, status: lastStatus, error: lastError };
}

async function listProjectUsers(
  accessToken: string,
  projectId: string,
  region: "US" | "EMEA",
): Promise<Array<{ id: string; email: string }>> {
  const candidates = projectId.startsWith("b.")
    ? [projectId, projectId.slice(2)]
    : [projectId];
  const out: Array<{ id: string; email: string }> = [];
  for (const candidate of candidates) {
    let offset = 0;
    let keepGoing = true;
    while (keepGoing) {
      const response = await fetch(
        `${APS_BASE}/construction/admin/v1/projects/${encodeURIComponent(candidate)}/users?offset=${offset}&limit=200`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${accessToken}`,
            "Content-Type": "application/json",
            Region: region,
          },
          cache: "no-store",
        },
      );
      if (!response.ok) {
        break;
      }
      const json = (await response.json()) as {
        results?: Array<{ id?: string; email?: string }>;
      };
      const rows = Array.isArray(json.results) ? json.results : [];
      for (const row of rows) {
        const id = typeof row.id === "string" ? row.id.trim() : "";
        const email = typeof row.email === "string" ? row.email.trim().toLowerCase() : "";
        if (!id || !email) continue;
        out.push({ id, email });
      }
      if (rows.length < 200) {
        keepGoing = false;
      } else {
        offset += rows.length;
      }
    }
    if (out.length > 0) break;
  }
  return out;
}

async function patchProjectUser(
  accessToken: string,
  projectId: string,
  userId: string,
  region: "US" | "EMEA",
  body: Record<string, unknown>,
): Promise<{ ok: boolean; status: number; response?: unknown; error?: string }> {
  const candidates = projectId.startsWith("b.")
    ? [projectId, projectId.slice(2)]
    : [projectId];
  let lastStatus = 500;
  let lastError = "Unknown error";
  for (const candidate of candidates) {
    const response = await fetch(
      `${APS_BASE}/construction/admin/v1/projects/${encodeURIComponent(candidate)}/users/${encodeURIComponent(userId)}`,
      {
        method: "PATCH",
        headers: {
          Authorization: `Bearer ${accessToken}`,
          "Content-Type": "application/json",
          Region: region,
        },
        body: JSON.stringify(body),
        cache: "no-store",
      },
    );
    const text = await response.text();
    let parsed: unknown = text;
    try {
      parsed = text ? (JSON.parse(text) as unknown) : {};
    } catch {
      // keep raw text
    }
    if (response.ok) {
      return { ok: true, status: response.status, response: parsed };
    }
    lastStatus = response.status;
    lastError =
      typeof parsed === "string"
        ? parsed
        : `API error (${response.status}): ${JSON.stringify(parsed)}`;
    if (response.status !== 404) break;
  }
  return { ok: false, status: lastStatus, error: lastError };
}


export async function addUsersToProjectsByNumber(params: {
  accessToken: string;
  hubId: string;
  projectNumbers: string[];
  businessUnitId?: string;
  businessUnitName?: string;
  emails: string[];
  roleIds?: string[];
  roleNames?: string[];
  products?: Array<Record<string, unknown>>;
  region?: "US" | "EMEA";
  dryRun?: boolean;
  cacheOnly?: boolean;
  additionalUserPayload?: Record<string, unknown>;
}): Promise<{
  success: boolean;
  requested_project_numbers: string[];
  requested_business_unit?: { business_unit_id?: string; business_unit_name?: string };
  requested_emails: string[];
  matched_projects: AdminProjectMatch[];
  missing_project_numbers: string[];
  attempted_assignments: number;
  success_count: number;
  failure_count: number;
  dry_run: boolean;
  resolved_role_ids: string[];
  unresolved_role_names: string[];
  role_cache_updated: boolean;
  role_cache_object_key: string;
  project_cache_updated: boolean;
  project_cache_object_key: string;
  project_cache_source: "cache" | "cache+live-refresh" | "cache-only-miss";
  results: AdminUserAssignmentResult[];
  message?: string;
}> {
  const requestedProjectNumbers = Array.from(
    new Set(
      params.projectNumbers
        .map((v) => normalizeProjectNumber(v))
        .filter((v) => v.length > 0),
    ),
  );
  const requestedEmails = Array.from(
    new Set(params.emails.map((v) => normalizeEmail(v)).filter((v) => v.length > 0)),
  );
  const requestedBusinessUnitId = params.businessUnitId?.trim() ?? "";
  const requestedBusinessUnitName = params.businessUnitName
    ? normalizeBusinessUnitName(params.businessUnitName)
    : "";
  if (requestedProjectNumbers.length === 0 && !requestedBusinessUnitId && !requestedBusinessUnitName) {
    throw new Error("projectNumbers or businessUnit filter is required.");
  }
  if (requestedEmails.length === 0) {
    throw new Error("emails is required.");
  }

  const region = params.region === "EMEA" ? "EMEA" : "US";
  const projectCache = await getHubProjectsWithCache({
    accessToken: params.accessToken,
    hubId: params.hubId,
    requestedProjectNumbers,
    cacheOnly: Boolean(params.cacheOnly),
  });
  const byNumber = projectCache.projectsByNumber;

  const allProjects = Array.from(byNumber.values());
  const matchesBusinessUnit = (project: AdminProjectMatch): boolean => {
    const matchById = requestedBusinessUnitId
      ? (project.businessUnitId ?? "") === requestedBusinessUnitId
      : true;
    const matchByName = requestedBusinessUnitName
      ? normalizeBusinessUnitName(project.businessUnitName ?? "") ===
        requestedBusinessUnitName
      : true;
    return matchById && matchByName;
  };
  let matchedProjects: AdminProjectMatch[] = [];
  const missingProjectNumbers: string[] = [];
  for (const projectNumber of requestedProjectNumbers) {
    const hit = byNumber.get(projectNumber);
    if (hit) matchedProjects.push(hit);
    else missingProjectNumbers.push(projectNumber);
  }
  if (requestedBusinessUnitId || requestedBusinessUnitName) {
    if (requestedProjectNumbers.length > 0) {
      const filtered = matchedProjects.filter(matchesBusinessUnit);
      const filteredKeys = new Set(
        filtered.map((p) => normalizeProjectNumber(p.projectNumber)),
      );
      for (const projectNumber of requestedProjectNumbers) {
        if (!filteredKeys.has(projectNumber) && !missingProjectNumbers.includes(projectNumber)) {
          missingProjectNumbers.push(projectNumber);
        }
      }
      matchedProjects = filtered;
    } else {
      matchedProjects = allProjects.filter(matchesBusinessUnit);
    }
  }

  const roleIds =
    Array.isArray(params.roleIds) && params.roleIds.length > 0
      ? params.roleIds.map((r) => String(r).trim()).filter(Boolean)
      : [];
  const requestedRoleNames = Array.from(
    new Set(
      (Array.isArray(params.roleNames) ? params.roleNames : [])
        .map((v) => normalizeRoleName(String(v)))
        .filter(Boolean),
    ),
  );
  const products =
    Array.isArray(params.products) && params.products.length > 0
      ? params.products
      : [];
  const normalizedProducts = normalizeProductAssignments(products);
  const additionalUserPayload =
    params.additionalUserPayload && typeof params.additionalUserPayload === "object"
      ? params.additionalUserPayload
      : {};

  const dryRun = Boolean(params.dryRun);
  const results: AdminUserAssignmentResult[] = [];
  const roleCache = await loadRoleCache(params.hubId);
  const cachedRoleMap = Object.fromEntries(mapFromRoleCache(roleCache).entries());
  let roleCacheUpdated = false;
  const discoveredRoleMap = new Map<string, string>();
  for (const [name, id] of Object.entries(cachedRoleMap)) {
    const n = normalizeRoleName(name);
    if (n && typeof id === "string" && id.trim()) {
      discoveredRoleMap.set(n, id.trim());
    }
  }

  for (const [name, id] of discoveredRoleMap.entries()) {
    if (cachedRoleMap[name] !== id) {
      cachedRoleMap[name] = id;
      roleCacheUpdated = true;
    }
  }
  if (roleCacheUpdated) {
    await saveRoleCache(params.hubId, cachedRoleMap);
  }

  const resolvedFromNames = requestedRoleNames
    .map((name) => discoveredRoleMap.get(name) ?? "")
    .filter(Boolean);
  const resolvedRoleIds = Array.from(new Set([...roleIds, ...resolvedFromNames]));
  const unresolvedAfterLookup = requestedRoleNames.filter(
    (name) => !discoveredRoleMap.has(name),
  );

  if (requestedRoleNames.length > 0 && unresolvedAfterLookup.length > 0) {
    return {
      success: false,
      requested_project_numbers: requestedProjectNumbers,
      ...(requestedBusinessUnitId || requestedBusinessUnitName
        ? {
            requested_business_unit: {
              ...(requestedBusinessUnitId ? { business_unit_id: requestedBusinessUnitId } : {}),
              ...(requestedBusinessUnitName
                ? { business_unit_name: requestedBusinessUnitName }
                : {}),
            },
          }
        : {}),
      requested_emails: requestedEmails,
      matched_projects: matchedProjects,
      missing_project_numbers: missingProjectNumbers,
      attempted_assignments: 0,
      success_count: 0,
      failure_count: 0,
      dry_run: dryRun,
      resolved_role_ids: resolvedRoleIds,
      unresolved_role_names: unresolvedAfterLookup,
      role_cache_updated: roleCacheUpdated,
      role_cache_object_key: roleCacheObjectKey(params.hubId),
      project_cache_updated: projectCache.cache_updated,
      project_cache_object_key: projectCache.project_cache_object_key,
      project_cache_source: projectCache.cache_source,
      results: [],
      message:
        "Some role names could not be resolved to role IDs. Run a dry run with explicit role_ids once, and the cache will retain mappings for later prompts.",
    };
  }

  for (const project of matchedProjects) {
    for (const email of requestedEmails) {
      if (dryRun) {
        results.push({
          projectId: project.projectId,
          projectName: project.projectName,
          projectNumber: project.projectNumber,
          email,
          ok: true,
          status: 200,
          response: {
            dry_run: true,
            payload_preview: {
              email,
              ...(resolvedRoleIds.length > 0 ? { roleIds: resolvedRoleIds } : {}),
              ...(normalizedProducts.length > 0 ? { products: normalizedProducts } : {}),
              ...additionalUserPayload,
            },
          },
        });
        continue;
      }

      const body: Record<string, unknown> = {
        email,
        ...(resolvedRoleIds.length > 0 ? { roleIds: resolvedRoleIds } : {}),
        ...(normalizedProducts.length > 0 ? { products: normalizedProducts } : {}),
        ...additionalUserPayload,
      };
      const assigned = await postProjectUser(
        params.accessToken,
        project.projectId,
        body,
        region,
      );
      let finalAssigned = assigned;
      let fallbackApplied = false;
      if (!assigned.ok && assigned.status === 409 && resolvedRoleIds.length > 0) {
        try {
          const existingUsers = await listProjectUsers(
            params.accessToken,
            project.projectId,
            region,
          );
          const existing = existingUsers.find((u) => u.email === email);
          if (existing) {
            const patch = await patchProjectUser(
              params.accessToken,
              project.projectId,
              existing.id,
              region,
              {
                roleIds: resolvedRoleIds,
                ...(normalizedProducts.length > 0 ? { products: normalizedProducts } : {}),
              },
            );
            if (patch.ok) {
              fallbackApplied = true;
              finalAssigned = {
                ok: true,
                status: patch.status,
                response: {
                  mode: "patched_existing_user_after_409",
                  existing_user_id: existing.id,
                  patch_response: patch.response,
                },
              };
            }
          }
        } catch {
          // keep original 409 response if fallback fails
        }
      }
      results.push({
        projectId: project.projectId,
        projectName: project.projectName,
        projectNumber: project.projectNumber,
        email,
        ok: finalAssigned.ok,
        status: finalAssigned.status,
        response: finalAssigned.response,
        error: finalAssigned.error,
        ...(fallbackApplied ? { fallback: "patch_existing_user" } : {}),
      });
    }
  }

  const successCount = results.filter((r) => r.ok).length;
  const failureCount = results.length - successCount;
  const totalPlanned = matchedProjects.length * requestedEmails.length;
  const success =
    missingProjectNumbers.length === 0 &&
    failureCount === 0 &&
    (totalPlanned > 0 || dryRun);

  return {
    success,
    requested_project_numbers: requestedProjectNumbers,
    ...(requestedBusinessUnitId || requestedBusinessUnitName
      ? {
          requested_business_unit: {
            ...(requestedBusinessUnitId ? { business_unit_id: requestedBusinessUnitId } : {}),
            ...(requestedBusinessUnitName ? { business_unit_name: requestedBusinessUnitName } : {}),
          },
        }
      : {}),
    requested_emails: requestedEmails,
    matched_projects: matchedProjects,
    missing_project_numbers: missingProjectNumbers,
    attempted_assignments: results.length,
    success_count: successCount,
    failure_count: failureCount,
    dry_run: dryRun,
    resolved_role_ids: resolvedRoleIds,
    unresolved_role_names: unresolvedAfterLookup,
    role_cache_updated: roleCacheUpdated,
    role_cache_object_key: roleCacheObjectKey(params.hubId),
    project_cache_updated: projectCache.cache_updated,
    project_cache_object_key: projectCache.project_cache_object_key,
    project_cache_source: projectCache.cache_source,
    results,
  };
}

export async function seedHubRoleCache(params: {
  accessToken: string;
  hubId: string;
  region?: "US" | "EMEA";
  maxProjects?: number;
  createRequestIfMissing?: boolean;
}): Promise<AdminRoleCacheSeedResult> {
  const region = params.region === "EMEA" ? "EMEA" : "US";
  const createRequestIfMissing = Boolean(params.createRequestIfMissing);
  const roleCache = await loadRoleCache(params.hubId);
  const cachedRoleMap = mapFromRoleCache(roleCache);
  const before = cachedRoleMap.size;
  const discoveredRoleMap = new Map<string, string>();
  let tablesUpdated = 0;
  const errors: string[] = [];
  const accountId = toDataConnectorAccountId(params.hubId);
  let requestId = "";
  let jobId = "";

  const requests = await listDataConnectorRequests({
    accessToken: params.accessToken,
    accountId,
    region,
  });
  const adminRequests = requests
    .filter((r) =>
      Array.isArray(r.serviceGroups) &&
      r.serviceGroups.some((g) => String(g).toLowerCase() === "admin"),
    )
    .sort((a, b) => compareIsoDesc(a.updatedAt ?? a.createdAt, b.updatedAt ?? b.createdAt));

  if (adminRequests.length > 0 && typeof adminRequests[0].id === "string") {
    requestId = adminRequests[0].id;
  } else {
    if (createRequestIfMissing) {
      requestId = await createAdminDataConnectorRequest({
        accessToken: params.accessToken,
        accountId,
        region,
      });
    } else {
      return {
        success: false,
        hubId: params.hubId,
        region,
        role_cache_object_key: roleCacheObjectKey(params.hubId),
        role_count_before: before,
        role_count_after: before,
        roles_added: 0,
        projects_scanned: 0,
        projects_with_errors: 0,
        role_cache_updated: false,
        tables_updated: 0,
        sample_roles: Array.from(cachedRoleMap.entries())
          .slice(0, 25)
          .map(([role_name, role_id]) => ({ role_name, role_id })),
        errors,
        data_connector: {
          account_id: accountId,
          status: "pending",
        },
        message:
          "No existing Data Connector admin request/job found for this hub account. Extraction was not triggered (createRequestIfMissing=false).",
      };
    }
  }

  const jobs = await listDataConnectorRequestJobs({
    accessToken: params.accessToken,
    accountId,
    requestId,
    region,
  });
  const completeSuccess = jobs
    .filter(
      (j) =>
        String(j.status ?? "").toLowerCase() === "complete" &&
        String(j.completionStatus ?? "").toLowerCase() === "success",
    )
    .sort((a, b) => compareIsoDesc(a.updatedAt ?? a.createdAt, b.updatedAt ?? b.createdAt));

  if (completeSuccess.length === 0 || typeof completeSuccess[0].id !== "string") {
    return {
      success: false,
      hubId: params.hubId,
      region,
      role_cache_object_key: roleCacheObjectKey(params.hubId),
      role_count_before: before,
      role_count_after: before,
      roles_added: 0,
      projects_scanned: 0,
      projects_with_errors: 0,
      role_cache_updated: false,
      tables_updated: 0,
      sample_roles: Array.from(cachedRoleMap.entries())
        .slice(0, 25)
        .map(([role_name, role_id]) => ({ role_name, role_id })),
      errors,
      data_connector: {
        account_id: accountId,
        request_id: requestId,
        status: "pending",
      },
      message:
        "Data Connector admin extraction is not complete yet. Re-run seed in a minute to ingest role CSVs.",
    };
  }

  jobId = completeSuccess[0].id;
  const listing = await getDataConnectorJobDataListing({
    accessToken: params.accessToken,
    accountId,
    jobId,
    region,
  });
  const roleCsvFiles = roleCsvRefsFromListing(listing);

  if (roleCsvFiles.length === 0) {
    return {
      success: false,
      hubId: params.hubId,
      region,
      role_cache_object_key: roleCacheObjectKey(params.hubId),
      role_count_before: before,
      role_count_after: before,
      roles_added: 0,
      projects_scanned: 0,
      projects_with_errors: 0,
      role_cache_updated: false,
      tables_updated: 0,
      sample_roles: Array.from(cachedRoleMap.entries())
        .slice(0, 25)
        .map(([role_name, role_id]) => ({ role_name, role_id })),
      errors,
      data_connector: {
        account_id: accountId,
        request_id: requestId,
        job_id: jobId,
        status: "failed",
      },
      message:
        "No admin_project_roles CSV files were found in the latest completed job. Extraction was not re-triggered automatically.",
    };
  }

  for (const file of roleCsvFiles) {
    let ingested = false;
    let lastErr = "";
    for (const dataRef of file.refs) {
      try {
        const signed = await getDataConnectorSignedData({
          accessToken: params.accessToken,
          accountId,
          jobId,
          dataName: dataRef,
          region,
        });
        const csv = await fetchSignedCsv(signed);
        const rows = extractRolesFromCsv(csv);
        for (const [name, id] of rows.entries()) discoveredRoleMap.set(name, id);
        if (rows.size > 0) tablesUpdated += 1;
        ingested = true;
        break;
      } catch (error) {
        const msg = error instanceof Error ? error.message : "unknown error";
        lastErr = `${msg} (dataRef=${dataRef})`;
      }
    }
    if (!ingested) {
      errors.push(`Failed to ingest role CSV ${file.displayName}: ${lastErr}`);
    }
  }

  const rolesByName = Object.fromEntries(
    Array.from(discoveredRoleMap.entries()).sort((a, b) =>
      a[0].localeCompare(b[0], undefined, { sensitivity: "base" }),
    ),
  );
  const after = discoveredRoleMap.size;
  const added = Math.max(
    0,
    Array.from(discoveredRoleMap.keys()).filter((k) => !cachedRoleMap.has(k)).length,
  );
  const removed = Math.max(
    0,
    Array.from(cachedRoleMap.keys()).filter((k) => !discoveredRoleMap.has(k)).length,
  );
  const updated = added > 0 || removed > 0;
  if (updated) {
    await saveRoleCache(params.hubId, rolesByName);
  }

  const allIngestFailed =
    after === 0 &&
    roleCsvFiles.length > 0 &&
    errors.length >= roleCsvFiles.length;
  if (allIngestFailed) {
    const localCsv = await loadLocalRoleCsvFallback();
    if (localCsv) {
      const localRows = extractRolesFromCsv(localCsv);
      if (localRows.size > 0) {
        const merged = new Map(discoveredRoleMap);
        for (const [name, id] of localRows.entries()) {
          merged.set(name, id);
        }
        const localRolesByName = Object.fromEntries(
          Array.from(merged.entries()).sort((a, b) =>
            a[0].localeCompare(b[0], undefined, { sensitivity: "base" }),
          ),
        );
        await saveRoleCache(params.hubId, localRolesByName);
        return {
          success: true,
          hubId: params.hubId,
          region,
          role_cache_object_key: roleCacheObjectKey(params.hubId),
          role_count_before: before,
          role_count_after: merged.size,
          roles_added: Math.max(
            0,
            Array.from(merged.keys()).filter((k) => !cachedRoleMap.has(k)).length,
          ),
          projects_scanned: 0,
          projects_with_errors: 0,
          role_cache_updated: true,
          tables_updated: 1,
          sample_roles: Array.from(merged.entries())
            .slice(0, 25)
            .map(([role_name, role_id]) => ({ role_name, role_id })),
          errors: [
            ...errors,
            "Used local fallback CSV (admin_project_roles.csv) because Data Connector signed-url fetch failed.",
          ],
          data_connector: {
            account_id: accountId,
            request_id: requestId,
            job_id: jobId,
            status: "seeded",
          },
          message:
            "Seeded role cache from local fallback CSV after Data Connector artifact fetch failed.",
        };
      }
    }
    return {
      success: false,
      hubId: params.hubId,
      region,
      role_cache_object_key: roleCacheObjectKey(params.hubId),
      role_count_before: before,
      role_count_after: 0,
      roles_added: 0,
      projects_scanned: 0,
      projects_with_errors: 0,
      role_cache_updated: false,
      tables_updated: 0,
      sample_roles: [],
      errors,
      data_connector: {
        account_id: accountId,
        request_id: requestId,
        job_id: jobId,
        status: "failed",
      },
      message:
        "Existing Data Connector job artifacts were not accessible (CSV fetch failed). Extraction was not re-triggered automatically.",
    };
  }

  const seedStatus: "seeded" | "failed" = after > 0 ? "seeded" : "failed";
  return {
    success: errors.length === 0 || after > 0,
    hubId: params.hubId,
    region,
    role_cache_object_key: roleCacheObjectKey(params.hubId),
    role_count_before: before,
    role_count_after: after,
    roles_added: added,
    projects_scanned: 0,
    projects_with_errors: 0,
    role_cache_updated: updated,
    tables_updated: tablesUpdated,
    sample_roles: Array.from(discoveredRoleMap.entries())
      .slice(0, 25)
      .map(([role_name, role_id]) => ({ role_name, role_id })),
    errors,
    data_connector: {
      account_id: accountId,
      request_id: requestId,
      job_id: jobId,
      status: seedStatus,
    },
    ...(after === 0
      ? {
          message:
            "Data Connector extraction completed but no role rows were ingested. Check data-listing references and admin_project_roles CSV availability for this account.",
        }
      : {}),
  };
}

export async function listHubRoleCache(params: {
  hubId: string;
}): Promise<AdminRoleCacheListResult> {
  const cache = await loadRoleCache(params.hubId);
  const roles = Array.from(mapFromRoleCache(cache).entries())
    .map(([role_name, role_id]) => ({
      role_name,
      role_id: String(role_id ?? ""),
    }))
    .filter((row) => row.role_name.trim() && row.role_id.trim())
    .sort((a, b) =>
      a.role_name.localeCompare(b.role_name, undefined, {
        sensitivity: "base",
        numeric: true,
      }),
    );

  return {
    success: true,
    hubId: params.hubId,
    role_cache_object_key: roleCacheObjectKey(params.hubId),
    role_count: roles.length,
    updated_at: cache.updatedAt,
    roles,
  };
}

