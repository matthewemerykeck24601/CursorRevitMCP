import { randomUUID } from "node:crypto";
import {
  analyzeAecElementsForMarks,
  type ProductPrefix,
} from "@/lib/aec-elements-for-marks";
import { submitMarkUpdateWorkitem } from "@/lib/da-workitems";

/** Mirrors MCP `CachedAnalysisResult` (separate in-memory cache in Next.js). */
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

function parsePrefix(raw: unknown): ProductPrefix {
  const o = raw && typeof raw === "object" ? (raw as Record<string, unknown>) : {};
  const p = String(o.product_prefix ?? "ALL");
  return ["WPA", "WPB", "CLA", "ALL"].includes(p)
    ? (p as ProductPrefix)
    : "ALL";
}

export type AnalyzePublishedOptions = {
  modelUrn?: string;
  accessToken?: string;
  hubId?: string;
  projectId?: string;
  itemId?: string;
};

export async function analyzePublishedModelAndCacheContract(
  raw: unknown,
  options: AnalyzePublishedOptions = {},
) {
  const o = raw && typeof raw === "object" ? (raw as Record<string, unknown>) : {};
  const product_prefix = parsePrefix(raw);
  const dry_run = o.dry_run !== false;
  const model_urn =
    (typeof o.model_urn === "string" && o.model_urn) ||
    (typeof o.urn === "string" && o.urn) ||
    options.modelUrn ||
    "";

  const token = options.accessToken;
  const hubId = options.hubId;
  const projectId = options.projectId;

  let proposed_marks: CachedAnalysisResult["proposed_marks"] = [];
  let sameness_groups: unknown[] = [];
  const warnings: unknown[] = [];
  let workitem_arguments: Record<string, unknown> | undefined;
  let aec_summary: CachedAnalysisResult["aec_summary"];

  if (token && hubId && projectId) {
    try {
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
        viewerModelUrn: model_urn || undefined,
        itemId: options.itemId,
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
  } else if (!model_urn) {
    throw new Error(
      "Provide hub/project with session for AEC analysis, or pass model_urn for a minimal cache entry.",
    );
  }

  const analysisResult: CachedAnalysisResult = {
    cache_id: randomUUID(),
    timestamp: new Date().toISOString(),
    product_prefix,
    model_urn: model_urn || "urn:viewer:not-supplied",
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
    success: true as const,
    message: `Analysis complete for ${product_prefix}. ${analysisResult.proposed_marks.length} mark group(s) proposed.`,
    cache_id: analysisResult.cache_id,
    preview: analysisResult.proposed_marks.slice(0, 10),
    aec_summary,
    warnings,
    next_step: dry_run
      ? "Review the preview above. If correct, call trigger_design_automation_mark_update with this cache_id."
      : "Dry-run disabled — proceeding to Design Automation (safety gate still applies).",
  };
}

export function getCachedMarkAnalysisContract() {
  const latest = Array.from(analysisCache.values()).sort(
    (a, b) =>
      new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
  )[0];

  if (!latest) {
    return {
      success: false as const,
      message:
        "No cached analysis found. Run analyze_published_model_and_cache first.",
    };
  }

  return {
    success: true as const,
    ...latest,
  };
}

export async function triggerDesignAutomationMarkUpdateContract(raw: unknown) {
  const o = raw && typeof raw === "object" ? (raw as Record<string, unknown>) : {};
  const cache_id = String(o.cache_id ?? "");
  const confirm = o.confirm === true;

  if (!confirm) {
    return {
      success: false as const,
      message:
        "Confirmation required. Set confirm: true to proceed with Design Automation.",
      cache_id: cache_id || undefined,
    };
  }

  if (!cache_id) {
    return {
      success: false as const,
      message: "cache_id is required.",
    };
  }

  const cached = analysisCache.get(cache_id);
  if (!cached) {
    throw new Error(
      `Cache ID ${cache_id} not found or expired. Run analyze_published_model_and_cache first.`,
    );
  }

  const args =
    cached.workitem_arguments && typeof cached.workitem_arguments === "object"
      ? { ...cached.workitem_arguments, cache_id: cached.cache_id }
      : {
          cache_id: cached.cache_id,
          product_prefix: cached.product_prefix,
          proposed_marks: cached.proposed_marks,
        };

  let daResult: Awaited<ReturnType<typeof submitMarkUpdateWorkitem>> = null;
  try {
    daResult = await submitMarkUpdateWorkitem({ workitemArguments: args });
  } catch (e) {
    return {
      success: false as const,
      message: `Design Automation submit failed: ${e instanceof Error ? e.message : String(e)}`,
      cache_id,
    };
  }

  if (daResult) {
    return {
      success: true as const,
      workitem_id: daResult.id,
      status: daResult.status ?? "submitted",
      applied_marks: cached.proposed_marks,
      note: "Workitem submitted. Users must Sync their local central model to see changes; next publish updates the Viewer.",
      da_raw: daResult.raw,
    };
  }

  return {
    success: true as const,
    workitem_id: "da-stub-" + Date.now(),
    status: "stub" as const,
    message:
      "DA_ENABLED is not true — no cloud workitem posted. Set DA_ENABLED=true, DA_ACTIVITY_ID, and APS credentials with code:all for live Design Automation.",
    applied_marks: cached.proposed_marks,
    note: "Users must Sync their local model to see changes. Next publish will update the Viewer.",
  };
}
