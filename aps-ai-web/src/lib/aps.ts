import { assertApsCredentials, env } from "@/lib/env";

const APS_BASE = "https://developer.api.autodesk.com";

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
    scope: env.apsScope,
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
  url.searchParams.set("scope", env.apsScope);
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

