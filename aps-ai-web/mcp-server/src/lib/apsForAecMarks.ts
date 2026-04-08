/** Minimal APS client for AEC mark analysis (mirrors aps-ai-web/src/lib/aps.ts). */

export async function apsPost<T>(
  path: string,
  accessToken: string,
  body: unknown,
): Promise<T> {
  const response = await fetch(`https://developer.api.autodesk.com${path}`, {
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
    throw new Error(`APS POST failed (${response.status}): ${text}`);
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
      /* fall through */
    }
  }

  for (const candidate of buildProjectIdCandidates(projectId)) {
    if (await projectWorks(candidate)) return candidate;
  }

  return buildProjectIdCandidates(projectId)[0];
}
