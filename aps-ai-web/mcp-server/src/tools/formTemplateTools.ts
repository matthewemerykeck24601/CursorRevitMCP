import { z } from "zod";

const DEFAULT_WEB_BASE_URL = "http://localhost:3000";

type ToolContext = {
  accessToken?: string;
  access_token?: string;
  hubId?: string;
  hub_id?: string;
};

function webBaseUrl(): string {
  const configured = process.env.APS_AI_WEB_BASE_URL?.trim();
  return (configured || DEFAULT_WEB_BASE_URL).replace(/\/+$/, "");
}

function resolveAccessToken(
  params: { access_token?: string; accessToken?: string },
  context: ToolContext,
): string {
  const token =
    params.access_token ??
    params.accessToken ??
    context.accessToken ??
    context.access_token;
  if (!token?.trim()) {
    throw new Error("access_token is required.");
  }
  return token.trim();
}

function resolveHubId(
  params: { hub_id?: string; hubId?: string },
  context: ToolContext,
): string | undefined {
  const hubId = params.hub_id ?? params.hubId ?? context.hubId ?? context.hub_id;
  return hubId?.trim() || undefined;
}

const analyzePdfParams = z.object({
  access_token: z.string().optional(),
  accessToken: z.string().optional(),
  account_id: z.string().optional(),
  accountId: z.string().optional(),
  hub_id: z.string().optional(),
  hubId: z.string().optional(),
  template_name: z.string().optional(),
  templateName: z.string().optional(),
  template_type: z.string().optional(),
  templateType: z.string().optional(),
  upload_token: z.string().optional(),
  uploadToken: z.string().optional(),
  project_id: z.string().optional(),
  projectId: z.string().optional(),
  version_id: z.string().optional(),
  versionId: z.string().optional(),
  pdf_base64: z.string().optional(),
  pdfBase64: z.string().optional(),
  pdf_url: z.string().optional(),
  pdfUrl: z.string().optional(),
});

const createTemplateParams = z.object({
  access_token: z.string().optional(),
  accessToken: z.string().optional(),
  account_id: z.string().optional(),
  accountId: z.string().optional(),
  hub_id: z.string().optional(),
  hubId: z.string().optional(),
  analysis_id: z.string().optional(),
  analysisId: z.string().optional(),
  template_name: z.string().optional(),
  templateName: z.string().optional(),
  template_type: z.string().optional(),
  templateType: z.string().optional(),
  dry_run: z.boolean().optional(),
  dryRun: z.boolean().optional(),
  field_overrides: z.array(z.record(z.string(), z.unknown())).optional(),
  fieldOverrides: z.array(z.record(z.string(), z.unknown())).optional(),
  ordered_field_ids: z.array(z.string()).optional(),
  orderedFieldIds: z.array(z.string()).optional(),
});

type AnalyzePdfParams = z.infer<typeof analyzePdfParams>;
type CreateTemplateParams = z.infer<typeof createTemplateParams>;

type AnalyzeSource =
  | { kind: "upload_token"; uploadToken: string }
  | { kind: "acc_version"; projectId: string; versionId: string }
  | { kind: "pdf_base64"; base64: string }
  | { kind: "pdf_url"; url: string };

function buildAnalyzeSource(params: AnalyzePdfParams): AnalyzeSource {
  const uploadToken = (params.upload_token ?? params.uploadToken ?? "").trim();
  if (uploadToken) {
    return { kind: "upload_token", uploadToken };
  }
  const projectId = (params.project_id ?? params.projectId ?? "").trim();
  const versionId = (params.version_id ?? params.versionId ?? "").trim();
  if (projectId && versionId) {
    return { kind: "acc_version", projectId, versionId };
  }
  const base64 = (params.pdf_base64 ?? params.pdfBase64 ?? "").trim();
  if (base64) {
    return { kind: "pdf_base64", base64 };
  }
  const url = (params.pdf_url ?? params.pdfUrl ?? "").trim();
  if (url) {
    return { kind: "pdf_url", url };
  }
  throw new Error(
    "Provide one source: upload_token, project_id+version_id, pdf_base64, or pdf_url.",
  );
}

async function callWebApi(
  path: string,
  accessToken: string,
  payload: Record<string, unknown>,
): Promise<unknown> {
  const response = await fetch(`${webBaseUrl()}${path}`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
    cache: "no-store",
  });
  const text = await response.text();
  let parsed: unknown = text;
  try {
    parsed = text ? (JSON.parse(text) as unknown) : {};
  } catch {
    // keep text
  }
  if (!response.ok) {
    throw new Error(
      `Web API call failed (${response.status}) at ${path}: ${
        typeof parsed === "string" ? parsed : JSON.stringify(parsed)
      }`,
    );
  }
  return parsed;
}

export const analyzePdfForFormTemplateTool = {
  name: "analyze_pdf_for_form_template" as const,
  description:
    "Analyze an uploaded/ACC Docs PDF into normalized native form template fields with confidence scoring and review gating.",
  parameters: analyzePdfParams,
  async handler(params: AnalyzePdfParams, context: ToolContext) {
    const accessToken = resolveAccessToken(params, context);
    const source = buildAnalyzeSource(params);
    const accountId = (params.account_id ?? params.accountId ?? "").trim();
    const hubId = resolveHubId(params, context);
    const templateName = (params.template_name ?? params.templateName ?? "").trim();
    const templateType = (params.template_type ?? params.templateType ?? "").trim();
    const payload = await callWebApi("/api/admin/forms/analyze", accessToken, {
      source,
      ...(accountId ? { accountId } : {}),
      ...(hubId ? { hubId } : {}),
      ...(templateName ? { templateName } : {}),
      ...(templateType ? { templateType } : {}),
    });
    return payload;
  },
};

export const createFormTemplateFromPdfTool = {
  name: "create_form_template_from_pdf" as const,
  description:
    "Create a native account-level form template from a reviewed PDF analysis artifact. Supports field_overrides (label/type/required/options/reviewState/calculated/formula/groupKey) and ordered_field_ids (reorder + remove). Translates grouped fields into Forma sections (multiple-entries) and calculated tables. Blocks if unresolved review fields remain.",
  parameters: createTemplateParams,
  async handler(params: CreateTemplateParams, context: ToolContext) {
    const accessToken = resolveAccessToken(params, context);
    const analysisId = (params.analysis_id ?? params.analysisId ?? "").trim();
    if (!analysisId) {
      throw new Error("analysis_id is required.");
    }
    const accountId = (params.account_id ?? params.accountId ?? "").trim();
    const hubId = resolveHubId(params, context);
    const templateName = (params.template_name ?? params.templateName ?? "").trim();
    const templateType = (params.template_type ?? params.templateType ?? "").trim();
    const fieldOverrides =
      (params.field_overrides ?? params.fieldOverrides ?? []) as Array<Record<string, unknown>>;
    const orderedFieldIds =
      params.ordered_field_ids ?? params.orderedFieldIds ?? [];
    const payload = await callWebApi(
      "/api/admin/forms/create-template",
      accessToken,
      {
        analysisId,
        ...(accountId ? { accountId } : {}),
        ...(hubId ? { hubId } : {}),
        ...(templateName ? { templateName } : {}),
        ...(templateType ? { templateType } : {}),
        ...(fieldOverrides.length > 0 ? { fieldOverrides } : {}),
        ...(orderedFieldIds.length > 0 ? { orderedFieldIds } : {}),
        dryRun: Boolean(params.dry_run ?? params.dryRun ?? false),
      },
    );
    return payload;
  },
};

export async function runAnalyzePdfForFormTemplate(
  args: unknown,
  context: ToolContext = {},
) {
  const parsed = analyzePdfForFormTemplateTool.parameters.parse(args);
  return analyzePdfForFormTemplateTool.handler(parsed, context);
}

export async function runCreateFormTemplateFromPdf(
  args: unknown,
  context: ToolContext = {},
) {
  const parsed = createFormTemplateFromPdfTool.parameters.parse(args);
  return createFormTemplateFromPdfTool.handler(parsed, context);
}

