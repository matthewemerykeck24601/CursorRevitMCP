import { z } from "zod";

const AecQueryInput = z.object({
  accessToken: z.string(),
  hubId: z.string(),
  projectId: z.string(),
  itemId: z.string().optional(),
});

type DataRow = {
  key: string;
  value: string;
  source: string;
};

function flatten(input: unknown, source: string, limit = 300): DataRow[] {
  const out: DataRow[] = [];
  const walk = (v: unknown, path: string) => {
    if (out.length >= limit) return;
    if (v === null || v === undefined) {
      out.push({ key: path, value: String(v), source });
      return;
    }
    if (typeof v !== "object") {
      out.push({ key: path, value: String(v), source });
      return;
    }
    if (Array.isArray(v)) {
      if (v.length === 0) {
        out.push({ key: path, value: "[]", source });
        return;
      }
      v.forEach((child, i) => walk(child, `${path}[${i}]`));
      return;
    }
    for (const [k, child] of Object.entries(v as Record<string, unknown>)) {
      walk(child, path ? `${path}.${k}` : k);
      if (out.length >= limit) break;
    }
  };
  walk(input, "");
  return out;
}

async function apsGet(path: string, accessToken: string): Promise<unknown> {
  const response = await fetch(`https://developer.api.autodesk.com${path}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    cache: "no-store",
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`APS GET failed (${response.status}): ${text}`);
  }
  return (await response.json()) as unknown;
}

async function apsPost(
  path: string,
  accessToken: string,
  body: unknown,
): Promise<unknown> {
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
  return (await response.json()) as unknown;
}

export async function aecQuery(rawInput: unknown) {
  const input = AecQueryInput.parse(rawInput);

  const graphQuery = `
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

  try {
    const payload = await apsPost("/aec/graphql", input.accessToken, {
      query: graphQuery,
      variables: { projectId: input.projectId },
    });
    return {
      sourcePath: "/aec/graphql#elementsByProject",
      rows: flatten(payload, "aecdatamodel:/aec/graphql", 300),
    };
  } catch {
    // fall through to legacy path candidates
  }

  const paths: string[] = [];
  if (input.itemId) {
    paths.push(
      `/aecdatamodel/v1/hubs/${encodeURIComponent(input.hubId)}/projects/${encodeURIComponent(input.projectId)}/items/${encodeURIComponent(input.itemId)}`,
    );
  }
  paths.push(
    `/aecdatamodel/v1/hubs/${encodeURIComponent(input.hubId)}/projects/${encodeURIComponent(input.projectId)}`,
  );

  for (const path of paths) {
    try {
      const payload = await apsGet(path, input.accessToken);
      return {
        sourcePath: path,
        rows: flatten(payload, `aecdatamodel:${path}`, 300),
      };
    } catch {
      // try next
    }
  }

  return {
    sourcePath: null,
    rows: [
      {
        key: "aecdatamodel",
        value: "No payload from attempted AEC Data Model endpoints.",
        source: "aecdatamodel",
      },
    ],
  };
}
