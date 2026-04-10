import { randomUUID } from "node:crypto";
import {
  analyzeAecElementsForMarks,
  type ProductPrefix,
} from "@/lib/aec-elements-for-marks";
import {
  groupAecdmElementsForMarks,
  queryAecdmElementsForMarks,
  type AecdmElementRow,
} from "@/lib/aecdmMarkGrouping";
import { resolveAecProjectId } from "@/lib/aps";
import {
  expandParameterUpdatesFromCachedSelection,
  mergeParameterPatchesIntoWorkitemArgs,
  parseDaParameterPatchesFromRequest,
  parseDaParameterUpdatesFromRequest,
} from "@/lib/da-parameter-patch";
import { env } from "@/lib/env";
import {
  fetchDaAuditReportSummary,
  pollDaWorkitemWithRetries,
  submitMarkUpdateWorkitem,
  type DaWorkitemPollResult,
} from "@/lib/da-workitems";

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
  return ["WPA", "WPB", "CLA", "COLUMN", "ALL"].includes(p)
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
  let usedAecdm = false;

  if (token && hubId && projectId && model_urn) {
    try {
      const aecProjectId = await resolveAecProjectId(token, hubId, projectId);
      const rows = await queryAecdmElementsForMarks({
        accessToken: token,
        aecProjectId,
        modelUrn: model_urn,
        category: "Structural Framing",
        family:
          product_prefix === "ALL"
            ? undefined
            : product_prefix === "COLUMN"
              ? "COLUMN"
              : product_prefix,
        limit: 500,
      });
      if (rows.length > 0) {
        const grouped = groupAecdmElementsForMarks({
          elements: rows as AecdmElementRow[],
          product_prefix,
          modelUrn: model_urn,
          hubId,
          dmProjectId: projectId,
          aecProjectId,
        });
        proposed_marks = grouped.proposed_marks;
        sameness_groups = grouped.sameness_groups;
        workitem_arguments = { ...grouped.workitem_arguments };
        warnings.push(...grouped.warnings);
        aec_summary = {
          elements_fetched: grouped.elements_fetched,
          elements_after_filter: grouped.elements_after_filter,
          aec_project_id: aecProjectId,
        };
        usedAecdm = true;
      } else {
        warnings.push(
          "AECDM REST returned 0 elements; falling back to AEC GraphQL.",
        );
      }
    } catch (e) {
      warnings.push(
        `AECDM query failed (${e instanceof Error ? e.message : String(e)}); falling back to AEC GraphQL.`,
      );
    }
  } else if (token && hubId && projectId && !model_urn) {
    warnings.push(
      "model_urn not provided — skipping AECDM REST; using AEC GraphQL only.",
    );
  }

  if (!usedAecdm && token && hubId && projectId) {
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
        `AEC GraphQL analysis failed: ${e instanceof Error ? e.message : String(e)}`,
      );
    }
  } else if (!(token && hubId && projectId) && !model_urn) {
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
    if (model_urn) workitem_arguments.viewerModelUrn = model_urn;
    if (options.itemId != null && options.itemId !== "") {
      workitem_arguments.itemId = options.itemId;
    }
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
  const skip_analysis = o.skip_analysis === true;
  const operation =
    typeof o.operation === "string" && o.operation.trim()
      ? o.operation.trim()
      : "";
  const parameterPatches = parseDaParameterPatchesFromRequest(o.parameter_patches);
  const fromCachedSelection = expandParameterUpdatesFromCachedSelection(
    o.cached_selection,
    o.updates,
  );
  const parameterUpdates = [
    ...fromCachedSelection,
    ...parseDaParameterUpdatesFromRequest(o.parameter_updates),
  ];
  const hasDirectEdits = parameterPatches.length > 0 || parameterUpdates.length > 0;
  /** No analyze cache: viewer-driven edits using externalIds only. */
  const directMode = skip_analysis && hasDirectEdits;

  if (!confirm) {
    return {
      success: false as const,
      workitem_submitted: false as const,
      revit_cloud_model_updated: false as const,
      message:
        "Confirmation required. Set confirm: true to proceed with Design Automation.",
      cache_id: cache_id || undefined,
    };
  }

  if (skip_analysis && !hasDirectEdits) {
    return {
      success: false as const,
      workitem_submitted: false as const,
      revit_cloud_model_updated: false as const,
      message:
        "skip_analysis requires parameter_patches and/or parameter_updates, or cached_selection.externalIds + updates[] (param actions). No mark-analysis workitem will be sent.",
      execution_assistant_hint:
        "Tell the user in one short sentence: the cloud job was not sent because the request was missing a valid updates list (e.g. CONTROL_MARK clear) or cached_selection.externalIds. For phrases like “clear the marks”, use updates: [{ paramName: \"CONTROL_MARK\", action: \"clear\" }] with the full discovery cached_selection.",
    };
  }

  let workitemArguments: Record<string, unknown>;
  let da_mode: "direct_parameter_modify" | "cached_analysis" = "cached_analysis";
  let applied_marks_preview: CachedAnalysisResult["proposed_marks"] = [];

  if (directMode) {
    da_mode = "direct_parameter_modify";
    workitemArguments = {
      version: 2,
      intent: "direct_parameter_modify",
      operation: operation || "modify_parameters",
      skip_analysis: true,
      cache_id: cache_id || "direct",
      product_prefix: parsePrefix(raw),
    };
    if (parameterUpdates.length > 0) {
      workitemArguments.parameter_updates = parameterUpdates;
    }
    workitemArguments = mergeParameterPatchesIntoWorkitemArgs(
      workitemArguments,
      parameterPatches,
    );
  } else {
    if (!cache_id) {
      return {
        success: false as const,
        workitem_submitted: false as const,
        revit_cloud_model_updated: false as const,
        message:
          "cache_id is required unless skip_analysis is true with parameter_patches, parameter_updates, or cached_selection + updates.",
      };
    }

    const cached = analysisCache.get(cache_id);
    if (!cached) {
      throw new Error(
        `Cache ID ${cache_id} not found or expired. Run analyze_published_model_and_cache first, or use skip_analysis with parameter_patches, parameter_updates, or cached_selection + updates.`,
      );
    }

    applied_marks_preview = cached.proposed_marks;

    const args =
      cached.workitem_arguments && typeof cached.workitem_arguments === "object"
        ? { ...cached.workitem_arguments, cache_id: cached.cache_id }
        : {
            cache_id: cached.cache_id,
            product_prefix: cached.product_prefix,
            proposed_marks: cached.proposed_marks,
          };

    workitemArguments = mergeParameterPatchesIntoWorkitemArgs(
      args,
      parameterPatches,
    );

    if (parameterUpdates.length > 0) {
      workitemArguments.parameter_updates = parameterUpdates;
    }

    if (skip_analysis) {
      workitemArguments = { ...workitemArguments };
      delete (workitemArguments as { marks?: unknown }).marks;
      workitemArguments = {
        ...workitemArguments,
        operation: operation || "modify_parameters",
        skip_analysis: true,
        intent: "direct_parameter_modify",
      };
    } else if (operation) {
      workitemArguments = { ...workitemArguments, operation };
    }
  }

  if (o.cached_selection && typeof o.cached_selection === "object" && !Array.isArray(o.cached_selection)) {
    workitemArguments = { ...workitemArguments, cached_selection: o.cached_selection };
  }
  if (Array.isArray(o.updates)) {
    workitemArguments = { ...workitemArguments, updates: o.updates };
  }

  const spgm = o.shared_parameter_guid_map;
  if (spgm && typeof spgm === "object" && !Array.isArray(spgm)) {
    const incoming = spgm as Record<string, string>;
    const existing = workitemArguments.sharedParameterGuidMap;
    const base =
      existing && typeof existing === "object" && !Array.isArray(existing)
        ? { ...(existing as Record<string, string>) }
        : {};
    workitemArguments = {
      ...workitemArguments,
      sharedParameterGuidMap: { ...base, ...incoming },
    };
  }

  let daResult: Awaited<ReturnType<typeof submitMarkUpdateWorkitem>> = null;
  try {
    daResult = await submitMarkUpdateWorkitem({ workitemArguments });
  } catch (e) {
    return {
      success: false as const,
      workitem_submitted: false as const,
      revit_cloud_model_updated: false as const,
      message: `Design Automation submit failed: ${e instanceof Error ? e.message : String(e)}`,
      cache_id,
    };
  }

  if (daResult) {
    let workitemPoll: DaWorkitemPollResult | undefined;
    let execution_assistant_hint = "";
    let da_audit_summary: { one_liner: string } | undefined;
    if (!env.daSkipWorkitemPoll) {
      const bundle = await pollDaWorkitemWithRetries(daResult.id, {
        maxAttempts: env.daPollMaxAttempts,
        delayBetweenMs: env.daPollDelayMs,
      });
      workitemPoll = bundle.last ?? undefined;
    }
    const polled = workitemPoll?.status;
    if (polled === "success") {
      let auditHint = "";
      if (workitemPoll?.reportUrl) {
        const sum = await fetchDaAuditReportSummary(workitemPoll.reportUrl);
        if (sum) {
          auditHint = sum.assistant_hint;
          da_audit_summary = { one_liner: sum.one_liner };
        }
      }
      execution_assistant_hint =
        auditHint ||
        "DA workitem finished successfully. One short sentence; mention ACC/Viewer refresh after publish if relevant.";
    } else if (polled && /^failed/i.test(polled)) {
      execution_assistant_hint = `DA workitem status: ${polled}. One brief factual sentence.`;
    } else {
      execution_assistant_hint =
        "Job submitted to Design Automation. Reply in ONE sentence: it is queued/running and you will report when done. Do not ask for more confirmation or say still preparing.";
    }
    return {
      success: true as const,
      workitem_submitted: true as const,
      /** Workitem accepted by DA; Revit file changes only after the activity runs successfully. */
      revit_cloud_model_updated: false as const,
      workitem_id: daResult.id,
      status: daResult.status ?? "submitted",
      da_mode,
      applied_marks: applied_marks_preview,
      note:
        da_mode === "direct_parameter_modify"
          ? "Direct parameter modify workitem (no mark grouping). Revit updates after the activity completes; sync locally; Viewer after publish."
          : "Workitem submitted to Design Automation. Revit central file updates only after the activity completes; then users sync locally. Viewer updates after next publish.",
      da_raw: daResult.raw,
      workitem_poll: workitemPoll,
      execution_assistant_hint,
      ...(da_audit_summary ? { da_audit_summary } : {}),
    };
  }

  return {
    success: true as const,
    workitem_submitted: false as const,
    da_stub: true as const,
    revit_cloud_model_updated: false as const,
    workitem_id: "da-stub-" + Date.now(),
    status: "stub" as const,
    da_mode,
    message:
      "DA_ENABLED is not true — no cloud workitem was posted. The ACC/Revit central model file was NOT modified. Set DA_ENABLED=true, DA_ACTIVITY_ID, and APS credentials (scope code:all) to post real workitems.",
    applied_marks: applied_marks_preview,
    note: "No cloud write occurred. Revit parameters are unchanged by this action.",
    execution_assistant_hint:
      "No DA job ran (stub/disabled). One sentence; do not imply Revit changed.",
  };
}
