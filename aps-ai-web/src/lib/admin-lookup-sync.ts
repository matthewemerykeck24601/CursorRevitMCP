import {
  getHubReferenceCacheSnapshot,
  listHubRoleCache,
  seedHubReferenceCache,
  seedHubRoleCache,
} from "@/lib/aps-admin";
import { firestoreAdmin } from "@/lib/firebase-admin";
import {
  LOOKUP_SCHEMA_VERSION,
  lookupDocPaths,
  normalizeLookupName,
  type LookupCacheStateDoc,
} from "@/lib/admin-lookup-schema";
import { getLookupIngestPolicy } from "@/lib/admin-lookup-ingest";

type SyncParams = {
  tenantId: string;
  hubId: string;
  accessToken: string;
  region?: "US" | "EMEA";
  refreshReferenceCache?: boolean;
};

export type LookupSyncResult = {
  success: boolean;
  tenantId: string;
  hubId: string;
  role_count: number;
  project_count: number;
  folder_count: number;
  role_seed: {
    success: boolean;
    message?: string;
    data_connector?: {
      account_id: string;
      request_id?: string;
      job_id?: string;
      status?: string;
    };
  };
  reference_cache_refreshed: boolean;
  cache_state_path: string;
};

function normalizeRegion(v: string | undefined): "US" | "EMEA" {
  return String(v ?? "US").toUpperCase() === "EMEA" ? "EMEA" : "US";
}

export async function syncLookupTablesFromHubCaches(
  params: SyncParams,
): Promise<LookupSyncResult> {
  const nowIso = new Date().toISOString();
  const region = normalizeRegion(params.region);
  const policy = getLookupIngestPolicy();
  const paths = lookupDocPaths(params.tenantId, params.hubId);

  const roleSeed = await seedHubRoleCache({
    accessToken: params.accessToken,
    hubId: params.hubId,
    region,
  });
  if (!roleSeed.success) {
    return {
      success: false,
      tenantId: params.tenantId,
      hubId: params.hubId,
      role_count: 0,
      project_count: 0,
      folder_count: 0,
      role_seed: {
        success: false,
        message: roleSeed.message,
        data_connector: roleSeed.data_connector,
      },
      reference_cache_refreshed: false,
      cache_state_path: paths.cacheState,
    };
  }

  let referenceRefreshed = false;
  if (params.refreshReferenceCache) {
    await seedHubReferenceCache({
      accessToken: params.accessToken,
      hubId: params.hubId,
    });
    referenceRefreshed = true;
  }

  const [roles, snapshot] = await Promise.all([
    listHubRoleCache({ hubId: params.hubId }),
    getHubReferenceCacheSnapshot({ hubId: params.hubId }),
  ]);

  const projects = snapshot.projects;
  const foldersByProject = snapshot.folders_by_project;
  const folderRows = Object.entries(foldersByProject).flatMap(([projectId, folderMap]) =>
    Object.entries(folderMap ?? {}).map(([folderName, folderId]) => ({
      projectId,
      folderName,
      folderId,
    })),
  );

  const db = firestoreAdmin;
  const writes: Array<Promise<unknown>> = [];

  for (const role of roles.roles) {
    const roleRef = db.doc(`${paths.roles}/${role.role_id}`);
    const roleNameNormalized = normalizeLookupName(role.role_name);
    writes.push(
      roleRef.set(
        {
          roleId: role.role_id,
          roleName: role.role_name,
          nameNormalized: roleNameNormalized,
          active: true,
          meta: {
            sourceJobId: roleSeed.data_connector?.job_id,
            sourceRegion: region,
            ingestedAt: nowIso,
            schemaVersion: LOOKUP_SCHEMA_VERSION,
          },
        },
        { merge: true },
      ),
    );
    if (roleNameNormalized) {
      writes.push(
        db.doc(`${paths.indexRolesByName}/${roleNameNormalized}`).set(
          {
            roleId: role.role_id,
            roleName: role.role_name,
            updatedAt: nowIso,
          },
          { merge: true },
        ),
      );
    }
  }

  const projectsByBusinessUnit = new Map<string, string[]>();
  for (const project of projects) {
    const projectRef = db.doc(`${paths.projects}/${project.projectId}`);
    const buNameNormalized = normalizeLookupName(project.businessUnitName ?? "");
    writes.push(
      projectRef.set(
        {
          projectId: project.projectId,
          projectNumber: project.projectNumber,
          projectName: project.projectName,
          businessUnitId: project.businessUnitId ?? null,
          businessUnitName: project.businessUnitName ?? null,
          businessUnitNameNormalized: buNameNormalized || null,
          active: true,
          meta: {
            extractedAt: snapshot.updated_at,
            ingestedAt: nowIso,
            schemaVersion: LOOKUP_SCHEMA_VERSION,
          },
        },
        { merge: true },
      ),
    );
    const buId = project.businessUnitId?.trim();
    if (buId) {
      projectsByBusinessUnit.set(buId, [
        ...(projectsByBusinessUnit.get(buId) ?? []),
        project.projectId,
      ]);
    }
  }

  for (const [businessUnitId, projectIds] of projectsByBusinessUnit.entries()) {
    writes.push(
      db.doc(`${paths.indexProjectsByBusinessUnit}/${businessUnitId}`).set(
        {
          businessUnitId,
          projectIds: Array.from(new Set(projectIds)).sort(),
          projectCount: Array.from(new Set(projectIds)).length,
          updatedAt: nowIso,
        },
        { merge: true },
      ),
    );
  }

  for (const row of folderRows) {
    writes.push(
      db.doc(`${paths.folders}/${row.folderId}`).set(
        {
          folderId: row.folderId,
          projectId: row.projectId,
          folderName: row.folderName,
          folderNameNormalized: normalizeLookupName(row.folderName),
          meta: {
            extractedAt: snapshot.updated_at,
            ingestedAt: nowIso,
            schemaVersion: LOOKUP_SCHEMA_VERSION,
          },
        },
        { merge: true },
      ),
    );
  }

  const cacheState: LookupCacheStateDoc = {
    hubId: params.hubId,
    latestSuccessfulRequestId: roleSeed.data_connector?.request_id,
    latestSuccessfulJobId: roleSeed.data_connector?.job_id,
    latestSuccessfulJobCompletedAt: undefined,
    lastIngestedAt: nowIso,
    lastIngestStatus: "success",
    rowCounts: {
      roles: roles.role_count,
      projects: projects.length,
      folders: folderRows.length,
    },
    schemaVersion: LOOKUP_SCHEMA_VERSION,
    readOnlyLatestExtraction: policy.readOnlyLatestExtraction,
  };
  writes.push(db.doc(paths.cacheState).set(cacheState, { merge: true }));

  // Firestore Admin SDK automatically chunks RPCs; still keep bounded parallel writes.
  const chunkSize = 300;
  for (let i = 0; i < writes.length; i += chunkSize) {
    await Promise.all(writes.slice(i, i + chunkSize));
  }

  return {
    success: true,
    tenantId: params.tenantId,
    hubId: params.hubId,
    role_count: roles.role_count,
    project_count: projects.length,
    folder_count: folderRows.length,
    role_seed: {
      success: true,
      message: roleSeed.message,
      data_connector: roleSeed.data_connector,
    },
    reference_cache_refreshed: referenceRefreshed,
    cache_state_path: paths.cacheState,
  };
}
