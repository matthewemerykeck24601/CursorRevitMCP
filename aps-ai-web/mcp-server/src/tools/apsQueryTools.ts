import axios from "axios";
import { z } from "zod";

/** In-memory cache for query/analysis (replace with Redis later). */
const queryCache = new Map<string, unknown>();

export type ApsQueryContext = {
  apsToken?: string;
  accessToken?: string;
  access_token?: string;
  modelUrn?: string;
  urn?: string;
  projectId?: string;
  project_id?: string;
  hubId?: string;
  hub_id?: string;
};

async function getApsToken(context: ApsQueryContext): Promise<string> {
  const t =
    context.apsToken ??
    context.accessToken ??
    context.access_token;
  if (typeof t === "string" && t.trim().length > 0) {
    return t.trim();
  }
  throw new Error(
    "APS access token not available; pass access_token or accessToken in tool arguments.",
  );
}

function contextFromArgs(
  args: Record<string, unknown>,
): ApsQueryContext {
  return {
    apsToken: typeof args.apsToken === "string" ? args.apsToken : undefined,
    accessToken:
      typeof args.accessToken === "string" ? args.accessToken : undefined,
    access_token:
      typeof args.access_token === "string" ? args.access_token : undefined,
    modelUrn:
      typeof args.model_urn === "string"
        ? args.model_urn
        : typeof args.modelUrn === "string"
          ? args.modelUrn
          : undefined,
    urn: typeof args.urn === "string" ? args.urn : undefined,
    projectId:
      typeof args.projectId === "string"
        ? args.projectId
        : typeof args.project_id === "string"
          ? args.project_id
          : undefined,
    project_id:
      typeof args.project_id === "string" ? args.project_id : undefined,
    hubId:
      typeof args.hubId === "string"
        ? args.hubId
        : typeof args.hub_id === "string"
          ? args.hub_id
          : undefined,
    hub_id: typeof args.hub_id === "string" ? args.hub_id : undefined,
  };
}

const tokenAndProjectFields = {
  access_token: z.string().optional(),
  accessToken: z.string().optional(),
  model_urn: z.string().optional(),
  urn: z.string().optional(),
  project_id: z.string().optional(),
  projectId: z.string().optional(),
  hub_id: z.string().optional(),
  hubId: z.string().optional(),
};

const GetElementsByCategoryParamsSchema = z.object({
  category: z.string().optional(),
  family: z.string().optional(),
  type: z.string().optional(),
  limit: z.number().default(500),
  ...tokenAndProjectFields,
});
type GetElementsByCategoryParams = z.infer<
  typeof GetElementsByCategoryParamsSchema
>;

const GetElementPropertiesParamsSchema = z.object({
  dbIds: z.array(z.union([z.string(), z.number()])),
  ...tokenAndProjectFields,
});
type GetElementPropertiesParams = z.infer<
  typeof GetElementPropertiesParamsSchema
>;

const AnalyzePublishedModelAecdmParamsSchema = z.object({
  product_prefix: z
    .enum(["WPA", "WPB", "CLA", "COLUMN", "ALL"])
    .default("ALL"),
  dry_run: z.boolean().default(true),
  ...tokenAndProjectFields,
});
type AnalyzePublishedModelAecdmParams = z.infer<
  typeof AnalyzePublishedModelAecdmParamsSchema
>;

// ======================
// Tool 1: Get Elements by Category / Family (core query tool)
// ======================
export const getElementsByCategory = {
  name: "get_elements_by_category" as const,
  description:
    "Queries the published APS model using AEC Data Model REST-style API for elements in a specific category or family (e.g., Structural Columns, WALL_PANEL_INSULATED). Returns a structured list with ids and key properties. Pass access_token, project_id, and model_urn.",
  parameters: GetElementsByCategoryParamsSchema,

  async handler(params: GetElementsByCategoryParams, context: ApsQueryContext) {
    const { category, family, type, limit } = params;
    const ctx: ApsQueryContext = {
      ...context,
      ...contextFromArgs(params as unknown as Record<string, unknown>),
    };
    const modelUrn = ctx.modelUrn || ctx.urn;
    const projectId = ctx.projectId || ctx.project_id;

    if (!modelUrn) {
      throw new Error("No model URN; pass model_urn or urn in arguments or context.");
    }
    if (!projectId) {
      throw new Error("No project id; pass project_id or projectId (AEC DM project) in arguments.");
    }

    const token = await getApsToken(ctx);

    const designId = modelUrn.includes(":")
      ? modelUrn.split(":").pop() ?? modelUrn
      : modelUrn;
    const url = `https://developer.api.autodesk.com/aecdm/v1/projects/${encodeURIComponent(projectId)}/designs/${encodeURIComponent(designId)}/elements`;

    const queryParams: Record<string, string | number> = { limit };
    if (category) queryParams["filter[category]"] = category;
    if (family) queryParams["filter[family]"] = family;
    if (type) queryParams["filter[type]"] = type;

    const response = await axios.get(url, {
      headers: { Authorization: `Bearer ${token}` },
      params: queryParams,
    });

    const raw = response.data as { elements?: unknown[] } | undefined;
    const elements = Array.isArray(raw?.elements) ? raw.elements : [];

    const result = {
      success: true,
      count: elements.length,
      elements: elements.map((el: unknown) => {
        const row = el as {
          id?: unknown;
          category?: unknown;
          family?: unknown;
          type?: unknown;
          properties?: Record<string, unknown>;
        };
        const props = row.properties ?? {};
        const extRaw =
          props["External ID"] ?? props.ExternalId ?? props.externalId;
        return {
          dbId: row.id,
          category: row.category,
          family: row.family,
          type: row.type,
          controlMark: props.CONTROL_MARK ?? null,
          externalId:
            extRaw == null || extRaw === ""
              ? null
              : String(extRaw).trim() || null,
          dimLength: props.DIM_LENGTH ?? props.Length ?? props.length,
          dimHeight: props.DIM_HEIGHT ?? props.Height ?? props.height,
          dimThickness:
            props.DIM_THICKNESS ?? props.Thickness ?? props.thickness,
        };
      }),
      modelUrn,
    };

    const cacheId = `query-${Date.now()}`;
    queryCache.set(cacheId, result);

    return {
      ...result,
      cacheId,
      message: `Found ${elements.length} elements matching the query.`,
    };
  },
};

// ======================
// Tool 2: Get Detailed Properties for Specific Elements
// ======================
export const getElementProperties = {
  name: "get_element_properties" as const,
  description:
    "Gets full parameter values and metadata for a list of element dbIds (viewer or AEC ids). Placeholder until Model Derivative / AEC batch property fetch is wired.",
  parameters: GetElementPropertiesParamsSchema,

  async handler(params: GetElementPropertiesParams, context: ApsQueryContext) {
    const { dbIds } = params;
    await getApsToken({
      ...context,
      ...contextFromArgs(params as unknown as Record<string, unknown>),
    });

    return {
      success: true,
      elements: dbIds.map((id: string | number) => ({
        dbId: id,
        properties: { CONTROL_MARK: null },
        note: "TODO: real Model Derivative / AEC property fetch",
      })),
    };
  },
};

// ======================
// Tool 3: Analyze & Cache (AECDM query path — distinct from GraphQL analyze_published_model_and_cache)
// ======================
export const analyzePublishedModelAecdmCache = {
  name: "analyze_published_model_aecdm_cache" as const,
  description:
    "Experimental: uses get_elements_by_category (AECDM REST) to pull elements, then caches a stub analysis for marking workflows. Prefer analyze_published_model_and_cache for AEC GraphQL + Design Automation contract.",
  parameters: AnalyzePublishedModelAecdmParamsSchema,

  async handler(
    params: AnalyzePublishedModelAecdmParams,
    context: ApsQueryContext,
  ) {
    const { product_prefix, dry_run } = params;
    const ctx: ApsQueryContext = {
      ...context,
      ...contextFromArgs(params as unknown as Record<string, unknown>),
    };

    const queryResult = await getElementsByCategory.handler(
      {
        category: undefined,
        family: product_prefix === "ALL" ? undefined : product_prefix,
        type: undefined,
        limit: 500,
        access_token: ctx.access_token,
        accessToken: ctx.accessToken,
        model_urn: ctx.modelUrn,
        urn: ctx.urn,
        project_id: ctx.project_id,
        projectId: ctx.projectId,
        hub_id: ctx.hub_id,
        hubId: ctx.hubId,
      },
      ctx,
    );

    const analysis = {
      cache_id: `analysis-${Date.now()}`,
      product_prefix,
      dry_run,
      elements_found: queryResult.count,
      proposed_marks: [] as unknown[],
      warnings: [] as string[],
    };

    queryCache.set(analysis.cache_id, analysis);

    return {
      success: true,
      ...analysis,
      message: `Analysis cached (AECDM path). Found ${queryResult.count} matching elements.`,
      next_step:
        "Review results; use analyze_published_model_and_cache + trigger_design_automation_mark_update for the supported DA pipeline.",
    };
  },
};

export async function runGetElementsByCategory(
  args: unknown,
  context: ApsQueryContext = {},
) {
  const parsed = getElementsByCategory.parameters.parse(args);
  return getElementsByCategory.handler(parsed, context);
}

export async function runGetElementProperties(
  args: unknown,
  context: ApsQueryContext = {},
) {
  const parsed = getElementProperties.parameters.parse(args);
  return getElementProperties.handler(parsed, context);
}

export async function runAnalyzePublishedModelAecdmCache(
  args: unknown,
  context: ApsQueryContext = {},
) {
  const parsed = analyzePublishedModelAecdmCache.parameters.parse(args);
  return analyzePublishedModelAecdmCache.handler(parsed, context);
}
