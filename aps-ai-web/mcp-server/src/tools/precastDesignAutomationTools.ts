import { z } from "zod";
import { v4 as uuidv4 } from "uuid";
import { analyzeAecElementsForMarks } from "../lib/aec-elements-for-marks.js";
import { submitMarkUpdateWorkitemMcp } from "../lib/daWorkitemsMcp.js";

/** Host-provided context (MCP stdio may pass URNs via tool args instead). */
export type PrecastDaContext = {
  modelUrn?: string;
  urn?: string;
};

export type CachedAnalysisResult = {
  cache_id: string;
  timestamp: string;
  product_prefix: string;
  model_urn: string;
  proposed_marks: Array<{
    groupId?: number;
    control_mark?: string;
    count?: number;
    elements?: unknown[];
  }>;
  sameness_groups: unknown[];
  warnings: unknown[];
  dry_run: boolean;
  workitem_arguments?: Record<string, unknown>;
  aec_summary?: {
    elements_fetched: number;
    elements_after_filter: number;
    aec_project_id: string;
  };
};

const analysisCache = new Map<string, CachedAnalysisResult>();

const AnalyzePublishedModelParamsSchema = z.object({
  product_prefix: z.enum(["WPA", "WPB", "CLA", "ALL"]).default("ALL"),
  dry_run: z.boolean().default(true),
  model_urn: z.string().optional(),
  urn: z.string().optional(),
  access_token: z.string().optional(),
  accessToken: z.string().optional(),
  hub_id: z.string().optional(),
  hubId: z.string().optional(),
  project_id: z.string().optional(),
  projectId: z.string().optional(),
  item_id: z.string().optional(),
  itemId: z.string().optional(),
});

type AnalyzePublishedModelParams = z.infer<
  typeof AnalyzePublishedModelParamsSchema
>;

// ======================
// Tool 1: Analyze Published Model + Cache
// ======================
export const analyzePublishedModelAndCache = {
  name: "analyze_published_model_and_cache" as const,
  description:
    "Queries the current APS published model (Viewer / AEC Data Model), runs mark verification & sameness logic, proposes CONTROL_MARKs starting at 100, and caches results for later Design Automation. Pass access_token, hub_id, project_id for AEC GraphQL; pass model_urn for viewer correlation.",
  parameters: AnalyzePublishedModelParamsSchema,

  async handler(parsed: AnalyzePublishedModelParams, context: PrecastDaContext) {
    const modelUrn =
      parsed.model_urn ??
      parsed.urn ??
      context.modelUrn ??
      context.urn ??
      "";

    const token = parsed.access_token ?? parsed.accessToken ?? "";
    const hubId = parsed.hub_id ?? parsed.hubId ?? "";
    const projectId = parsed.project_id ?? parsed.projectId ?? "";
    const itemId = parsed.item_id ?? parsed.itemId;

    const { product_prefix, dry_run } = parsed;

    if (!(token && hubId && projectId) && !modelUrn) {
      throw new Error(
        "Provide access_token + hub_id + project_id for AEC analysis, or pass model_urn (minimal cache).",
      );
    }

    let proposed_marks: CachedAnalysisResult["proposed_marks"] = [];
    let sameness_groups: unknown[] = [];
    const warnings: unknown[] = [];
    let workitem_arguments: Record<string, unknown> | undefined;
    let aec_summary: CachedAnalysisResult["aec_summary"];

    if (token && hubId && projectId) {
      try {
        console.log(
          `[Analyze] AEC GraphQL for project ${projectId}, prefix ${product_prefix}`,
        );
        const analysis = await analyzeAecElementsForMarks({
          accessToken: token,
          hubId,
          projectId,
          product_prefix,
        });
        proposed_marks = analysis.proposed_marks.map((g) => ({
          groupId: g.groupId,
          control_mark: g.control_mark,
          count: g.count,
          elements: g.elements,
        }));
        sameness_groups = analysis.sameness_groups;
        warnings.push(...analysis.warnings);
        workitem_arguments = {
          ...analysis.workitem_arguments,
          viewerModelUrn: modelUrn || undefined,
          itemId,
        };
        aec_summary = {
          elements_fetched: analysis.elements_fetched,
          elements_after_filter: analysis.elements_after_filter,
          aec_project_id: analysis.aec_project_id,
        };
      } catch (e) {
        warnings.push(
          `AEC analysis failed: ${e instanceof Error ? e.message : String(e)}`,
        );
      }
    }

    const analysisResult: CachedAnalysisResult = {
      cache_id: uuidv4(),
      timestamp: new Date().toISOString(),
      product_prefix,
      model_urn: modelUrn || "urn:viewer:not-supplied",
      proposed_marks,
      sameness_groups,
      warnings,
      dry_run,
      workitem_arguments,
      aec_summary,
    };

    if (workitem_arguments) {
      workitem_arguments.cache_id = analysisResult.cache_id;
    }

    analysisCache.set(analysisResult.cache_id, analysisResult);

    return {
      success: true,
      message: `Analysis complete for ${product_prefix}. ${analysisResult.proposed_marks.length} group(s) proposed.`,
      cache_id: analysisResult.cache_id,
      preview: analysisResult.proposed_marks.slice(0, 10),
      aec_summary,
      warnings,
      next_step: dry_run
        ? "Review the preview above. If correct, call trigger_design_automation_mark_update with this cache_id."
        : "Dry-run disabled — proceeding to Design Automation (safety gate still applies).",
    };
  },
};

// ======================
// Tool 2: Trigger Design Automation Write
// ======================
export const triggerDesignAutomationMarkUpdate = {
  name: "trigger_design_automation_mark_update" as const,
  description:
    "Takes cached analysis results and runs APS Design Automation on the central Revit model to apply CONTROL_MARKs and other parameter updates.",
  parameters: z.object({
    cache_id: z.string(),
    confirm: z.boolean().default(false),
  }),

  async handler(
    params: { cache_id: string; confirm: boolean },
    _context: PrecastDaContext,
  ) {
    const { cache_id, confirm } = params;

    if (!confirm) {
      return {
        success: false,
        message:
          "Confirmation required. Set confirm: true to proceed with Design Automation.",
        cache_id,
      };
    }

    const cached = analysisCache.get(cache_id);
    if (!cached) {
      throw new Error(
        `Cache ID ${cache_id} not found or expired. Run analyze_published_model_and_cache first.`,
      );
    }

    console.log(`[Design Automation] Triggering update for cache ${cache_id}`);

    const args =
      cached.workitem_arguments && typeof cached.workitem_arguments === "object"
        ? { ...cached.workitem_arguments, cache_id: cached.cache_id }
        : {
            cache_id: cached.cache_id,
            product_prefix: cached.product_prefix,
            proposed_marks: cached.proposed_marks,
          };

    try {
      const da = await submitMarkUpdateWorkitemMcp({ workitemArguments: args });
      if (da) {
        return {
          success: true,
          workitem_id: da.id,
          status: da.status ?? "submitted",
          applied_marks: cached.proposed_marks,
          note: "Workitem submitted. Sync central model in Revit; next publish updates Viewer.",
          da_raw: da.raw,
        };
      }
    } catch (e) {
      return {
        success: false,
        message: `DA submit failed: ${e instanceof Error ? e.message : String(e)}`,
        cache_id,
      };
    }

    return {
      success: true,
      workitem_id: "da-stub-" + Date.now(),
      status: "stub",
      message:
        "DA_ENABLED is not true — no workitem posted. Set DA_ENABLED=true and DA_ACTIVITY_ID in MCP server environment.",
      applied_marks: cached.proposed_marks,
      note: "Users must Sync their local model to see changes. Next publish will update the Viewer.",
    };
  },
};

// ======================
// Tool 3: Get Cached Results (for preview / status)
// ======================
export const getCachedMarkAnalysis = {
  name: "get_cached_mark_analysis" as const,
  description:
    "Returns the latest cached mark verification results for user preview before triggering Design Automation.",
  parameters: z.object({}),

  async handler(
    _params: Record<string, never>,
    _context: PrecastDaContext,
  ) {
    const latest = Array.from(analysisCache.values()).sort(
      (a, b) =>
        new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
    )[0];

    if (!latest) {
      return {
        success: false,
        message:
          "No cached analysis found. Run analyze_published_model_and_cache first.",
      };
    }

    return {
      success: true,
      ...latest,
    };
  },
};

export async function runAnalyzePublishedModelAndCache(
  args: unknown,
  context: PrecastDaContext = {},
) {
  const parsed = analyzePublishedModelAndCache.parameters.parse(args);
  return analyzePublishedModelAndCache.handler(parsed, context);
}

export async function runTriggerDesignAutomationMarkUpdate(
  args: unknown,
  context: PrecastDaContext = {},
) {
  const parsed = triggerDesignAutomationMarkUpdate.parameters.parse(args);
  return triggerDesignAutomationMarkUpdate.handler(parsed, context);
}

export async function runGetCachedMarkAnalysis(
  args: unknown,
  context: PrecastDaContext = {},
) {
  const parsed = getCachedMarkAnalysis.parameters.parse(args);
  return getCachedMarkAnalysis.handler(parsed, context);
}
