/**
 * Canonical Firestore lookup schema for Alice admin workflows.
 * Phase B foundation: define stable document contracts and path helpers.
 */

export type LookupEntityMeta = {
  sourceJobId?: string;
  sourceFile?: string;
  sourceRegion?: "US" | "EMEA";
  extractedAt?: string;
  ingestedAt: string;
  schemaVersion: number;
};

export type LookupProjectDoc = {
  projectId: string;
  projectNumber: string;
  projectName: string;
  businessUnitId?: string;
  businessUnitName?: string;
  businessUnitNameNormalized?: string;
  active: boolean;
  meta: LookupEntityMeta;
};

export type LookupRoleDoc = {
  roleId: string;
  roleName: string;
  nameNormalized: string;
  active: boolean;
  meta: LookupEntityMeta;
};

export type LookupUserDoc = {
  userId: string;
  email: string;
  emailNormalized: string;
  displayName?: string;
  status?: string;
  active: boolean;
  meta: LookupEntityMeta;
};

export type LookupProjectUserDoc = {
  key: string; // `${projectId}_${userId}`
  projectId: string;
  userId: string;
  emailNormalized: string;
  roleIds: string[];
  productKeys: string[];
  status?: string;
  meta: LookupEntityMeta;
};

export type LookupFolderDoc = {
  folderId: string;
  projectId: string;
  parentFolderId?: string;
  folderName: string;
  folderNameNormalized: string;
  isProjectFilesRoot?: boolean;
  meta: LookupEntityMeta;
};

export type LookupCacheStateDoc = {
  hubId: string;
  latestSuccessfulRequestId?: string;
  latestSuccessfulJobId?: string;
  latestSuccessfulJobCompletedAt?: string;
  lastIngestedAt?: string;
  lastIngestStatus: "idle" | "running" | "success" | "failed";
  rowCounts: Record<string, number>;
  schemaVersion: number;
  readOnlyLatestExtraction: boolean;
};

export const LOOKUP_SCHEMA_VERSION = 1;

export function normalizeLookupName(value: string): string {
  return String(value ?? "")
    .trim()
    .toLowerCase()
    .replace(/\s+/g, " ");
}

export function normalizeLookupEmail(value: string): string {
  return String(value ?? "").trim().toLowerCase();
}

export function lookupDocPaths(tenantId: string, hubId: string) {
  const base = `tenants/${tenantId}/hubs/${hubId}`;
  return {
    base,
    cacheState: `${base}/meta/cacheState`,
    projects: `${base}/projects`,
    roles: `${base}/roles`,
    users: `${base}/users`,
    projectUsers: `${base}/projectUsers`,
    folders: `${base}/folders`,
    operations: `${base}/operations`,
    indexProjectsByBusinessUnit: `${base}/indexes/projectsByBusinessUnit`,
    indexProjectsByUser: `${base}/indexes/projectsByUser`,
    indexRolesByName: `${base}/indexes/rolesByName`,
  };
}
