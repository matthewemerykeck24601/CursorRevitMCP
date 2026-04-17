import { assertApsCredentials, env } from "@/lib/env";

const APS_BASE = "https://developer.api.autodesk.com";
const BLOCKED_3LEGGED_SCOPES = new Set(["code:all"]);

function getUserOAuthScope(): string {
  // APS rejects code:all in 3-legged authorize flows.
  const scopes = env.apsScope
    .split(/\s+/)
    .map((scope) => scope.trim())
    .filter(Boolean)
    .filter((scope) => !BLOCKED_3LEGGED_SCOPES.has(scope));
  return scopes.join(" ");
}

export type ApsTokenResponse = {
  access_token: string;
  token_type: "Bearer";
  expires_in: number;
  refresh_token?: string;
  scope?: string;
};

export async function exchangeCodeForToken(
  code: string,
): Promise<ApsTokenResponse> {
  assertApsCredentials();
  const body = new URLSearchParams({
    client_id: env.apsClientId,
    client_secret: env.apsClientSecret,
    grant_type: "authorization_code",
    code,
    redirect_uri: env.apsCallbackUrl,
  });

  const response = await fetch(`${APS_BASE}/authentication/v2/token`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
    cache: "no-store",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed APS code exchange: ${response.status} ${text}`);
  }

  return (await response.json()) as ApsTokenResponse;
}

export async function refreshApsToken(
  refreshToken: string,
): Promise<ApsTokenResponse> {
  assertApsCredentials();
  const body = new URLSearchParams({
    client_id: env.apsClientId,
    client_secret: env.apsClientSecret,
    grant_type: "refresh_token",
    refresh_token: refreshToken,
    scope: getUserOAuthScope(),
  });

  const response = await fetch(`${APS_BASE}/authentication/v2/token`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
    cache: "no-store",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed APS token refresh: ${response.status} ${text}`);
  }

  return (await response.json()) as ApsTokenResponse;
}

export function buildAuthorizeUrl(state: string): string {
  assertApsCredentials();
  const url = new URL(`${APS_BASE}/authentication/v2/authorize`);
  url.searchParams.set("response_type", "code");
  url.searchParams.set("client_id", env.apsClientId);
  url.searchParams.set("redirect_uri", env.apsCallbackUrl);
  url.searchParams.set("scope", getUserOAuthScope());
  url.searchParams.set("state", state);
  return url.toString();
}

export function toViewerUrn(versionId: string): string {
  return Buffer.from(versionId)
    .toString("base64")
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/g, "");
}

export type DerivativeMetadataResponse = {
  data: {
    metadata: Array<{
      guid: string;
      name: string;
      role: string;
    }>;
  };
};

export async function apsGet<T>(
  path: string,
  accessToken: string,
): Promise<T> {
  const response = await fetch(`${APS_BASE}${path}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    cache: "no-store",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`APS GET ${path} failed (${response.status}): ${text}`);
  }

  return (await response.json()) as T;
}

export async function apsPost<T>(
  path: string,
  accessToken: string,
  body: unknown,
): Promise<T> {
  const response = await fetch(`${APS_BASE}${path}`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
    cache: "no-store",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`APS POST ${path} failed (${response.status}): ${text}`);
  }

  return (await response.json()) as T;
}

function normalizeDmProjectId(projectId: string): string {
  const trimmed = projectId.trim();
  return trimmed.startsWith("b.") ? trimmed : `b.${trimmed}`;
}

function buildProjectIdCandidates(projectId: string): string[] {
  if (projectId.startsWith("urn:adsk")) return [projectId];
  const dm = normalizeDmProjectId(projectId);
  return [
    `urn:adsk.wipprod:dm.project:${dm}`,
    `urn:adsk.wipprod:project:${dm}`,
  ];
}

type AecProjectsQueryResponse = {
  data?: {
    projects?: {
      results?: Array<{
        id?: string;
        alternativeIdentifiers?: {
          dataManagementAPIProjectId?: string;
          externalProjectId?: string;
        };
      }>;
    };
  };
};

export async function resolveAecProjectId(
  accessToken: string,
  hubId: string,
  projectId: string,
): Promise<string> {
  const testProjectQuery = `
    query TestProject($projectId: ID!) {
      elementsByProject(projectId: $projectId, pagination: { limit: 1 }) {
        results { id }
      }
    }
  `;
  const projectWorks = async (candidate: string): Promise<boolean> => {
    try {
      const probe = await apsPost<{
        data?: unknown;
        errors?: Array<{ message?: string }>;
      }>("/aec/graphql", accessToken, {
        query: testProjectQuery,
        variables: { projectId: candidate },
      });
      return !probe.errors || probe.errors.length === 0;
    } catch {
      return false;
    }
  };

  if (projectId.startsWith("urn:adsk")) {
    if (await projectWorks(projectId)) return projectId;
  }
  const dmProjectId = normalizeDmProjectId(projectId);

  const projectsQuery = `
    query ResolveProjectId($hubId: ID!) {
      projects(hubId: $hubId, pagination: { limit: 200 }) {
        results {
          id
          alternativeIdentifiers {
            dataManagementAPIProjectId
            externalProjectId
          }
        }
      }
    }
  `;

  if (hubId.startsWith("urn:adsk")) {
    try {
      const result = await apsPost<AecProjectsQueryResponse>(
        "/aec/graphql",
        accessToken,
        {
          query: projectsQuery,
          variables: { hubId },
        },
      );
      const match = (result.data?.projects?.results ?? []).find((p) => {
        const dm = p.alternativeIdentifiers?.dataManagementAPIProjectId ?? "";
        const ext = p.alternativeIdentifiers?.externalProjectId ?? "";
        return dm === dmProjectId || ext === projectId || ext === dmProjectId;
      });
      if (match?.id?.startsWith("urn:adsk")) return match.id;
    } catch {
      // fallback to probing deterministic candidates below
    }
  }

  for (const candidate of buildProjectIdCandidates(projectId)) {
    if (await projectWorks(candidate)) return candidate;
  }

  return buildProjectIdCandidates(projectId)[0];
}

export async function getModelMetadata(
  viewerUrn: string,
  accessToken: string,
): Promise<DerivativeMetadataResponse> {
  const safeUrn = encodeURIComponent(viewerUrn);
  return apsGet<DerivativeMetadataResponse>(
    `/modelderivative/v2/designdata/${safeUrn}/metadata`,
    accessToken,
  );
}

export async function queryAecDataModel(
  accessToken: string,
  hubId: string,
  projectId: string,
  itemId?: string,
): Promise<{ sourcePath: string; payload: unknown } | null> {
  const aecProjectId = await resolveAecProjectId(accessToken, hubId, projectId);
  const diagnostics: string[] = [];
  // AEC Data Model primary access is GraphQL.
  const projectQuery = `
    query AecProjectSnapshot($projectId: ID!) {
      elementsByProject(projectId: $projectId, pagination: { limit: 25 }) {
        pagination { cursor }
        results {
          id
          name
          properties(
            filter: { names: ["External ID", "Category", "Family Name", "Type Name", "CONTROL_MARK", "CONTROL_NUMBER", "Length", "Width", "Height"] }
          ) {
            results {
              name
              value
              definition {
                units { name }
              }
            }
          }
        }
      }
    }
  `;

  type GraphqlEnvelope = {
    data?: unknown;
    errors?: Array<{ message?: string }>;
  };

  try {
    const gql = await apsPost<GraphqlEnvelope>("/aec/graphql", accessToken, {
      query: projectQuery,
      variables: { projectId: aecProjectId },
    });
    if (gql.data) {
      return {
        sourcePath: "/aec/graphql#elementsByProject",
        payload: gql.data,
      };
    }
    if (Array.isArray(gql.errors) && gql.errors.length > 0) {
      throw new Error(
        gql.errors.map((e) => e.message ?? "Unknown GraphQL error").join("; "),
      );
    }
  } catch (error) {
    diagnostics.push(
      `graphql: ${error instanceof Error ? error.message : "unknown graphql error"}`,
    );
    // Continue to legacy candidate paths as fallback for older integrations.
  }

  const candidates: string[] = [];
  if (itemId) {
    candidates.push(
      `/aecdatamodel/v1/hubs/${encodeURIComponent(hubId)}/projects/${encodeURIComponent(projectId)}/items/${encodeURIComponent(itemId)}`,
    );
  }
  candidates.push(
    `/aecdatamodel/v1/hubs/${encodeURIComponent(hubId)}/projects/${encodeURIComponent(projectId)}`,
  );

  for (const path of candidates) {
    try {
      const payload = await apsGet<unknown>(path, accessToken);
      return { sourcePath: path, payload };
    } catch (error) {
      diagnostics.push(
        `${path}: ${error instanceof Error ? error.message : "unknown get error"}`,
      );
      // try next candidate
    }
  }
  return {
    sourcePath: "aecdatamodel:diagnostics",
    payload: {
      hubId,
      projectId,
      aecProjectId,
      itemId: itemId ?? null,
      diagnostics,
      note: "No AEC Data Model payload succeeded. See diagnostics.",
    },
  };
}

function parseOssObjectUrn(
  storageUrn: string,
): { bucketKey: string; objectKey: string } | null {
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
  const path = `/oss/v2/buckets/${encodeURIComponent(params.bucketKey)}/objects/${encodeURIComponent(params.objectKey)}/signeds3download`;
  const res = await fetch(`${APS_BASE}${path}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${params.accessToken}`,
      "Content-Type": "application/json",
    },
    cache: "no-store",
  });
  if (!res.ok) {
    const t = await res.text();
    throw new Error(`signeds3download failed (${res.status}): ${t}`);
  }
  const json = (await res.json()) as { url?: string; signedUrl?: string };
  const url = json.url ?? json.signedUrl ?? "";
  if (!url) throw new Error("signeds3download response missing url.");
  return url;
}

export async function buildVersionOssGetArgument(params: {
  accessToken: string;
  projectId: string;
  versionId: string;
}): Promise<{ url: string; headers: Record<string, string> } | null> {
  const versionPath = `/data/v1/projects/${encodeURIComponent(params.projectId)}/versions/${encodeURIComponent(params.versionId)}`;
  const version = await apsGet<{
    data?: {
      relationships?: {
        storage?: {
          data?: {
            id?: string;
          };
        };
      };
    };
  }>(versionPath, params.accessToken);
  const storageUrn = version.data?.relationships?.storage?.data?.id ?? "";
  if (!storageUrn) return null;
  const parsed = parseOssObjectUrn(storageUrn);
  if (!parsed) return null;
  try {
    const signedUrl = await getSignedS3DownloadUrl({
      accessToken: params.accessToken,
      bucketKey: parsed.bucketKey,
      objectKey: parsed.objectKey,
    });
    return {
      url: signedUrl,
      headers: {},
    };
  } catch {
    // Legacy OSS object GET endpoint is deprecated for some buckets.
    // Return null so callers can surface a deterministic "input file unavailable"
    // message instead of submitting a doomed workitem.
    return null;
  }
}

export type RevitCloudModelInfo = {
  region: string;
  projectGuid: string;
  modelGuid: string;
  revitVersionMajor?: number;
};

function inferRevitVersionMajor(
  extensionType: string,
  extensionData: unknown,
): number | undefined {
  const candidates: string[] = [extensionType];
  const visit = (value: unknown) => {
    if (value == null) return;
    if (typeof value === "string" || typeof value === "number") {
      candidates.push(String(value));
      return;
    }
    if (Array.isArray(value)) {
      for (const item of value) visit(item);
      return;
    }
    if (typeof value === "object") {
      for (const v of Object.values(value as Record<string, unknown>)) visit(v);
    }
  };
  visit(extensionData);
  for (const raw of candidates) {
    const match = raw.match(/\b(20(?:1[8-9]|2[0-9]|3[0-5]))\b/);
    if (!match) continue;
    const year = Number.parseInt(match[1], 10);
    if (Number.isFinite(year)) return year;
  }
  return undefined;
}

export async function getRevitCloudModelInfo(params: {
  accessToken: string;
  projectId: string;
  versionId: string;
}): Promise<RevitCloudModelInfo | null> {
  const versionPath = `/data/v1/projects/${encodeURIComponent(params.projectId)}/versions/${encodeURIComponent(params.versionId)}`;
  const version = await apsGet<{
    data?: {
      attributes?: {
        extension?: {
          type?: string;
          data?: {
            projectGuid?: string;
            modelGuid?: string;
            region?: string;
          };
        };
      };
    };
  }>(versionPath, params.accessToken);

  const extensionType =
    version.data?.attributes?.extension?.type?.toLowerCase() ?? "";
  if (!extensionType.includes("c4rmodel")) return null;

  const extData = version.data?.attributes?.extension?.data;
  const projectGuid = extData?.projectGuid?.trim() ?? "";
  const modelGuid = extData?.modelGuid?.trim() ?? "";
  const regionRaw = extData?.region?.trim() ?? "";
  if (!projectGuid || !modelGuid) return null;

  const inferredRegionRaw = regionRaw || (params.versionId.includes("wipemea") ? "EMEA" : "US");
  const regionUpper = inferredRegionRaw.toUpperCase();
  const region =
    regionUpper === "US" || regionUpper === "EMEA"
      ? regionUpper
      : regionUpper.includes("EMEA")
        ? "EMEA"
        : "US";

  const revitVersionMajor = inferRevitVersionMajor(extensionType, extData);
  return {
    region,
    projectGuid,
    modelGuid,
    ...(revitVersionMajor ? { revitVersionMajor } : {}),
  };
}

