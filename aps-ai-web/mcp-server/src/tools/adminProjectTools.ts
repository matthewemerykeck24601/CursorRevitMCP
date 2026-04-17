import { z } from "zod";

const APS_BASE = "https://developer.api.autodesk.com";

type ToolContext = {
  accessToken?: string;
  access_token?: string;
  hubId?: string;
  hub_id?: string;
};

const addUsersToProjectsParams = z.object({
  access_token: z.string().optional(),
  accessToken: z.string().optional(),
  hub_id: z.string().optional(),
  hubId: z.string().optional(),
  project_numbers: z.array(z.string()).default([]),
  emails: z.array(z.string()).default([]),
  role_ids: z.array(z.string()).optional().default([]),
  products: z.array(z.record(z.string(), z.unknown())).optional().default([]),
  region: z.enum(["US", "EMEA"]).optional().default("US"),
  dry_run: z.boolean().optional().default(false),
  additional_user_payload: z.record(z.string(), z.unknown()).optional(),
});

type AddUsersToProjectsParams = z.infer<typeof addUsersToProjectsParams>;

type DmProject = {
  id?: string;
  attributes?: {
    name?: string;
  };
};

type DmProjectsResponse = {
  data?: DmProject[];
};

function firstProjectToken(name: string): string {
  const token = name.match(/^\s*([A-Za-z0-9._-]+)/)?.[1] ?? "";
  return token.trim();
}

function getToken(params: AddUsersToProjectsParams, context: ToolContext): string {
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

function getHubId(params: AddUsersToProjectsParams, context: ToolContext): string {
  const hubId = params.hub_id ?? params.hubId ?? context.hubId ?? context.hub_id;
  if (!hubId || !hubId.trim()) {
    throw new Error("hub_id is required.");
  }
  return hubId.trim();
}

async function listHubProjects(accessToken: string, hubId: string) {
  const response = await fetch(
    `${APS_BASE}/project/v1/hubs/${encodeURIComponent(hubId)}/projects`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    },
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Project list failed (${response.status}): ${text}`);
  }
  const json = (await response.json()) as DmProjectsResponse;
  return (json.data ?? [])
    .map((p) => {
      const id = typeof p.id === "string" ? p.id.trim() : "";
      const name = typeof p.attributes?.name === "string" ? p.attributes.name.trim() : "";
      const projectNumber = firstProjectToken(name).toUpperCase();
      return { id, name, projectNumber };
    })
    .filter((p) => p.id && p.name && p.projectNumber);
}

async function postProjectUser(
  accessToken: string,
  projectId: string,
  region: "US" | "EMEA",
  body: Record<string, unknown>,
) {
  const candidates = projectId.startsWith("b.")
    ? [projectId, projectId.slice(2)]
    : [projectId];
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
    if (response.status !== 404) {
      return {
        ok: false,
        status: response.status,
        error:
          typeof parsed === "string"
            ? parsed
            : `API error (${response.status}): ${JSON.stringify(parsed)}`,
      };
    }
  }
  return { ok: false, status: 404, error: "Project not found in Admin API." };
}

export const adminAddUsersToProjects = {
  name: "admin_add_users_to_projects" as const,
  description:
    "Adds one or more emails to multiple ACC projects by project number using the selected hub project list.",
  parameters: addUsersToProjectsParams,
  async handler(params: AddUsersToProjectsParams, context: ToolContext) {
    const accessToken = getToken(params, context);
    const hubId = getHubId(params, context);
    const projectNumbers = Array.from(
      new Set(params.project_numbers.map((v) => v.trim().toUpperCase()).filter(Boolean)),
    );
    const emails = Array.from(
      new Set(params.emails.map((v) => v.trim().toLowerCase()).filter(Boolean)),
    );
    if (projectNumbers.length === 0) {
      throw new Error("project_numbers is required.");
    }
    if (emails.length === 0) {
      throw new Error("emails is required.");
    }

    const projects = await listHubProjects(accessToken, hubId);
    const byNumber = new Map(projects.map((p) => [p.projectNumber, p]));
    const matches = projectNumbers
      .map((n) => byNumber.get(n))
      .filter((v): v is NonNullable<typeof v> => Boolean(v));
    const missing = projectNumbers.filter((n) => !byNumber.has(n));

    const results: Array<Record<string, unknown>> = [];
    for (const project of matches) {
      for (const email of emails) {
        const body: Record<string, unknown> = {
          email,
          ...(params.role_ids.length > 0 ? { roleIds: params.role_ids } : {}),
          ...(params.products.length > 0 ? { products: params.products } : {}),
          ...(params.additional_user_payload ?? {}),
        };
        if (params.dry_run) {
          results.push({
            projectId: project.id,
            projectName: project.name,
            projectNumber: project.projectNumber,
            email,
            ok: true,
            dry_run: true,
            payload_preview: body,
          });
          continue;
        }
        const response = await postProjectUser(
          accessToken,
          project.id,
          params.region,
          body,
        );
        results.push({
          projectId: project.id,
          projectName: project.name,
          projectNumber: project.projectNumber,
          email,
          ...response,
        });
      }
    }

    const successCount = results.filter((r) => r.ok === true).length;
    const failureCount = results.length - successCount;

    return {
      success: missing.length === 0 && failureCount === 0,
      requested_project_numbers: projectNumbers,
      requested_emails: emails,
      matched_projects: matches.map((m) => ({
        projectId: m.id,
        projectName: m.name,
        projectNumber: m.projectNumber,
      })),
      missing_project_numbers: missing,
      attempted_assignments: results.length,
      success_count: successCount,
      failure_count: failureCount,
      dry_run: params.dry_run,
      results,
    };
  },
};

export async function runAdminAddUsersToProjects(
  args: unknown,
  context: ToolContext = {},
) {
  const parsed = adminAddUsersToProjects.parameters.parse(args);
  return adminAddUsersToProjects.handler(parsed, context);
}

