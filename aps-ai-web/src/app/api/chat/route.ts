import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { getModelMetadata, queryAecDataModel } from "@/lib/aps";
import {
  planLocalAgentTools,
  resolveViewerIntent,
  type ViewerIntentAction,
} from "@/lib/ai-chat";
import {
  fetchAecdmElementsByCategory,
  type AecdmProductPrefix,
} from "@/lib/aecdm-get-elements";
import { env, hasAnyAiProviderKey } from "@/lib/env";
import { log } from "@/lib/logger";
import { getRequestId } from "@/lib/request";
import { mcpGetElementParameters } from "@/lib/mcp-local-tools";
import {
  createProjectIssue,
  getIssuesContainerId,
  listProjectIssues,
} from "@/lib/aps-issues";
import {
  analyzePublishedModelAndCacheContract,
  getCachedMarkAnalysisContract,
  triggerDesignAutomationMarkUpdateContract,
} from "@/lib/precast-design-automation-contract";
import {
  analyzeProductsAndMarkContract,
  assignControlMarksContract,
  getProductSamenessReportContract,
} from "@/lib/precast-mark-contract";

type ChatRequest = {
  message?: string;
  selectedModelName?: string;
  selectedModelUrn?: string;
  selectedHubId?: string;
  selectedProjectId?: string;
  selectedItemId?: string;
  selectedDbIds?: number[];
  selectedCount?: number;
  chatHistory?: string[];
  selectedElements?: Array<{
    dbId?: number;
    name?: string;
    externalId?: string;
    properties?: Array<{
      displayName?: string;
      displayValue?: string;
      units?: string;
    }>;
  }>;
  aiProvider?: string;
  aiModel?: string;
  assistantMode?: string;
  workspaceMode?: "model" | "product-analysis";
  productAnalysis?: {
    rulesText?: string;
    selectedDesignFile?: Record<string, unknown> | null;
    selectedProduct?: Record<string, unknown> | null;
    analysisInputs?: Record<string, string>;
    selectionContext?: {
      hubId?: string;
      hubName?: string;
      projectId?: string;
      projectNumber?: string;
      projectName?: string;
      modelItemId?: string;
      modelVersionId?: string;
      modelName?: string;
      modelUrn?: string;
    };
  };
};

type ChatToolAction = ViewerIntentAction;

function parseToolDbIds(raw: unknown): number[] {
  if (!Array.isArray(raw)) return [];
  const out: number[] = [];
  for (const x of raw) {
    const n = typeof x === "number" ? x : Number(x);
    if (Number.isFinite(n)) out.push(Math.trunc(n));
  }
  return out.slice(0, 500);
}

function parseProductPrefix(
  raw: unknown,
): AecdmProductPrefix | undefined {
  if (
    raw === "WPA" ||
    raw === "WPB" ||
    raw === "CLA" ||
    raw === "COLUMN" ||
    raw === "ALL"
  ) {
    return raw;
  }
  return undefined;
}

function fallbackIntent(message: string): {
  actions: ChatToolAction[];
  modelQueryRequested: boolean;
  message: string;
} {
  const lower = message.toLowerCase();
  const actions: ChatToolAction[] = [];

  if (lower.includes("fit") || lower.includes("zoom extents")) {
    actions.push({ type: "viewer.fitToView" });
  }
  if (lower.includes("clear selection") || lower.includes("unselect")) {
    actions.push({ type: "viewer.clearSelection" });
  }
  if (lower.includes("show all") || lower.includes("reset visibility")) {
    actions.push({ type: "viewer.showAll" });
  }
  if (lower.includes("save markup")) {
    actions.push({ type: "viewer.markupsSave" });
  }
  if (lower.includes("load markup")) {
    actions.push({ type: "viewer.markupsLoad" });
  }
  if (lower.includes("clear markup")) {
    actions.push({ type: "viewer.markupsClear" });
  }

  const isolateMatch = message.match(/(?:isolate|only show)\s+(.+)/i);
  if (isolateMatch?.[1]) {
    actions.push({ type: "viewer.isolateByQuery", query: isolateMatch[1].trim() });
  } else if (lower.includes("isolate")) {
    actions.push({ type: "viewer.isolateSelection" });
  }

  const searchMatch = message.match(/(?:find|search|highlight)\s+(.+)/i);
  if (searchMatch?.[1]) {
    actions.push({ type: "viewer.search", query: searchMatch[1].trim() });
  }

  const modelQueryRequested =
    lower.includes("list views") ||
    lower.includes("model views") ||
    lower.includes("metadata");

  return {
    actions,
    modelQueryRequested,
    message:
      actions.length > 0
        ? `Understood. I prepared ${actions.length} viewer action(s).`
        : "I can help query and control the viewer. Try: find walls, fit view, or clear selection.",
  };
}

export const runtime = "nodejs";

function shouldLogFull(): boolean {
  return env.chatLogLevel === "full";
}

type ElementProperty = {
  displayName: string;
  displayValue: string;
  units?: string;
};

type ElementSnapshot = {
  dbId: number;
  name?: string;
  externalId?: string;
  properties: ElementProperty[];
};

function normalizeElements(input: ChatRequest["selectedElements"]): ElementSnapshot[] {
  if (!Array.isArray(input)) return [];
  const out: ElementSnapshot[] = [];
  for (const el of input) {
    const dbId = Number(el.dbId);
    if (!Number.isFinite(dbId)) continue;
    const properties = Array.isArray(el.properties)
      ? el.properties
          .filter((p) => typeof p.displayName === "string")
          .map((p) => ({
            displayName: String(p.displayName),
            displayValue: String(p.displayValue ?? ""),
            units: p.units,
          }))
      : [];
    out.push({
      dbId,
      name: el.name,
      externalId: el.externalId,
      properties,
    });
  }
  return out;
}

export async function POST(request: NextRequest) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const body = (await request.json()) as ChatRequest;
  const message = (body.message ?? "").trim();

  if (!message) {
    return NextResponse.json(
      { error: "Message cannot be empty", requestId },
      { status: 400 },
    );
  }

  const selectedModelName = body.selectedModelName ?? "no model selected";
  const selectedCount =
    typeof body.selectedCount === "number"
      ? body.selectedCount
      : Array.isArray(body.selectedDbIds)
        ? body.selectedDbIds.length
        : 0;
  const selectedDbIds = Array.isArray(body.selectedDbIds)
    ? body.selectedDbIds.filter((v) => Number.isFinite(v)).slice(0, 20)
    : [];
  const selectedElements = normalizeElements(body.selectedElements).slice(0, 5);
  const selectedHubId = body.selectedHubId;
  const selectedProjectId = body.selectedProjectId;
  const selectedItemId = body.selectedItemId;
  const aiProvider = body.aiProvider ?? env.aiProvider;
  const aiModel = body.aiModel ?? "";
  const assistantMode = "agent";
  const workspaceMode = body.workspaceMode ?? "model";
  const productAnalysis = body.productAnalysis;
  let actions: ChatToolAction[] = [];
  const toolViewerActions: ChatToolAction[] = [];
  let modelQueryRequested = false;
  let responseText = "";
  const queryResult: Record<string, unknown> = {};
  let externalContext = "";

  if (shouldLogFull()) {
    log("info", "chat-request", {
      requestId,
      provider: aiProvider,
      model: aiModel,
      assistantMode,
      selectedModelName,
      selectedCount,
      selectedDbIds,
      selectedElementsCount: selectedElements.length,
      selectedElementsPropertyCounts: selectedElements.map((e) => ({
        dbId: e.dbId,
        propertyCount: e.properties.length,
      })),
      workspaceMode,
      prompt: message,
    });
  }

  if (selectedHubId && selectedProjectId) {
    externalContext = `${externalContext}\nHUB_PROJECT_AVAILABLE`.trim();
  }

  let modelViewsFromTool: Array<{ guid: string; name: string; role: string }> = [];
  let issuesSummaryText = "";
  if (hasAnyAiProviderKey()) {
    try {
      const plannedTools = await planLocalAgentTools(message, selectedModelName, {
        provider: aiProvider,
        model: aiModel,
        assistantMode,
        selectedCount,
        selectedDbIds,
        selectedElements,
        chatHistory: body.chatHistory,
        externalContext,
      });

      if (shouldLogFull()) {
        log("info", "local-agent-tool-plan", {
          requestId,
          plannedTools,
        });
      }

      for (const call of plannedTools) {
        if (
          call.tool === "aec_query" &&
          selectedHubId &&
          selectedProjectId
        ) {
          try {
            const aec = await queryAecDataModel(
              auth.session.accessToken,
              selectedHubId,
              selectedProjectId,
              selectedItemId,
            );
            if (aec) {
              const aecContext = JSON.stringify(aec.payload).slice(0, 6000);
              externalContext = `${externalContext}\nAEC_QUERY_CONTEXT: ${aecContext}`.trim();
            }
          } catch (error) {
            if (shouldLogFull()) {
              log("warn", "local-aec-context-unavailable", {
                requestId,
                details: error instanceof Error ? error.message : "Unknown error",
              });
            }
          }
        }

        if (call.tool === "selected_element_parameters") {
          const localMcpRead = mcpGetElementParameters({
            selectedElements,
            dbIds: selectedDbIds,
            parameterQueries: [
              "CONTROL_MARK",
              "CONTROL_NUMBER",
              "DIM_*",
              "IDENTITY_DESCRIPTION",
              "CONSTRUCTION_PRODUCT",
              "DESIGN_NUMBER",
              "WIDTH",
              "HEIGHT",
              "LENGTH",
            ],
          });
          externalContext = `${externalContext}\nMCP_READ_CONTEXT: ${JSON.stringify(
            localMcpRead.slice(0, 3),
          )}`.trim();
        }

        if (call.tool === "get_cached_selection") {
          const snapshot = selectedElements.slice(0, 200).map((el) => ({
            dbId: el.dbId,
            externalId: el.externalId ?? "",
            name: el.name ?? "",
          }));
          const payload = {
            success: true,
            count: selectedElements.length,
            dbIds: selectedDbIds.slice(0, 200),
            elements: snapshot,
            note: "Use externalId in parameter_patches.externalIds or parameter_updates for skip_analysis Design Automation payloads.",
          };
          externalContext =
            `${externalContext}\nGET_CACHED_SELECTION: ${JSON.stringify(payload)}`.trim();
        }

        if (call.tool === "model_views" && body.selectedModelUrn) {
          try {
            const metadata = await getModelMetadata(
              body.selectedModelUrn,
              auth.session.accessToken,
            );
            modelViewsFromTool = metadata.data.metadata.map((item) => ({
              guid: item.guid,
              name: item.name,
              role: item.role,
            }));
            externalContext = `${externalContext}\nMODEL_VIEWS_CONTEXT: ${JSON.stringify(
              modelViewsFromTool.slice(0, 25),
            )}`.trim();
          } catch (error) {
            if (shouldLogFull()) {
              log("warn", "local-model-views-context-unavailable", {
                requestId,
                details: error instanceof Error ? error.message : "Unknown error",
              });
            }
          }
        }
        if (call.tool === "issues_list") {
          const containerId = getIssuesContainerId();
          if (!containerId) {
            issuesSummaryText =
              "Issues container is not configured yet. Set APS_ISSUES_CONTAINER_ID to enable issue commands.";
          } else {
            try {
              const issues = await listProjectIssues(
                auth.session.accessToken,
                containerId,
              );
              issuesSummaryText = `Found ${issues.length} project issue(s).`;
              queryResult.issues = issues.slice(0, 20);
            } catch (error) {
              issuesSummaryText = `Unable to list issues: ${
                error instanceof Error ? error.message : "unknown error"
              }`;
            }
          }
        }
        if (call.tool === "issues_create") {
          const containerId = getIssuesContainerId();
          if (!containerId) {
            issuesSummaryText =
              "Issues container is not configured yet. Set APS_ISSUES_CONTAINER_ID to enable issue creation.";
          } else {
            const title = message.slice(0, 120);
            try {
              const created = await createProjectIssue(
                auth.session.accessToken,
                containerId,
                {
                  title: title || "Issue created by Alice",
                  description: `Created from chat prompt: ${message}`.slice(0, 500),
                },
              );
              issuesSummaryText = `Created issue: ${created.title}`;
              queryResult.createdIssue = created;
            } catch (error) {
              issuesSummaryText = `Unable to create issue: ${
                error instanceof Error ? error.message : "unknown error"
              }`;
            }
          }
        }
        if (call.tool === "analyze_published_model_and_cache") {
          const baseArgs =
            call.args &&
            typeof call.args === "object" &&
            !Array.isArray(call.args)
              ? { ...call.args }
              : {};
          try {
            const payload = await analyzePublishedModelAndCacheContract(
              baseArgs,
              {
                modelUrn: body.selectedModelUrn,
                accessToken: auth.session.accessToken,
                hubId: body.selectedHubId,
                projectId: body.selectedProjectId,
                itemId: body.selectedItemId,
              },
            );
            externalContext =
              `${externalContext}\nPRECAST_ANALYZE_PUBLISHED_CACHE: ${JSON.stringify(payload)}`.trim();
          } catch (error) {
            externalContext =
              `${externalContext}\nPRECAST_ANALYZE_PUBLISHED_CACHE_ERROR: ${JSON.stringify({
                message:
                  error instanceof Error ? error.message : "Unknown error",
              })}`.trim();
          }
        }
        if (call.tool === "get_cached_mark_analysis") {
          const payload = getCachedMarkAnalysisContract();
          externalContext =
            `${externalContext}\nPRECAST_CACHED_MARK_ANALYSIS: ${JSON.stringify(payload)}`.trim();
        }
        if (call.tool === "trigger_design_automation_mark_update") {
          try {
            const payload = await triggerDesignAutomationMarkUpdateContract(
              call.args ?? { cache_id: "", confirm: false },
            );
            externalContext =
              `${externalContext}\nPRECAST_DA_MARK_UPDATE: ${JSON.stringify(payload)}`.trim();
            const p = payload as {
              workitem_submitted?: boolean;
              status?: string;
            };
            const noCloudWrite =
              p.workitem_submitted === false ||
              (p.workitem_submitted === undefined && p.status === "stub");
            if (noCloudWrite) {
              externalContext =
                `${externalContext}\nCLOUD_WRITE_TRUTH: The central Revit cloud model was NOT modified. Design Automation did not submit a workitem (disabled/stub or pre-confirm). Do NOT tell the user CONTROL_MARK was cleared in Revit or that syncing will show parameter changes. Quote PRECAST_DA_MARK_UPDATE message/note. Selection-based "clear marks" is not a live cloud write unless DA is enabled and the activity applies that change.`.trim();
            }
          } catch (error) {
            externalContext =
              `${externalContext}\nPRECAST_DA_MARK_UPDATE_ERROR: ${JSON.stringify({
                message:
                  error instanceof Error ? error.message : "Unknown error",
              })}`.trim();
            externalContext =
              `${externalContext}\nCLOUD_WRITE_TRUTH: Design Automation failed; the central Revit model was not updated.`.trim();
          }
        }
        if (call.tool === "analyze_products_and_mark") {
          const payload = analyzeProductsAndMarkContract(
            call.args ?? {
              product_prefix: "ALL",
              dry_run: true,
            },
          );
          externalContext =
            `${externalContext}\nPRECAST_ANALYZE_PRODUCTS_AND_MARK: ${JSON.stringify(payload)}`.trim();
        }
        if (call.tool === "get_product_sameness_report") {
          const payload = getProductSamenessReportContract(
            call.args ?? { element_ids: selectedDbIds.map(String) },
          );
          externalContext =
            `${externalContext}\nPRECAST_SAMENESS_REPORT: ${JSON.stringify(payload)}`.trim();
        }
        if (call.tool === "assign_control_marks") {
          const payload = assignControlMarksContract(
            call.args ?? { mark_groups: [], start_number: 100 },
          );
          externalContext =
            `${externalContext}\nPRECAST_ASSIGN_CONTROL_MARKS: ${JSON.stringify(payload)}`.trim();
        }

        if (
          call.tool === "get_elements_by_category" &&
          selectedHubId &&
          selectedProjectId &&
          body.selectedModelUrn
        ) {
          const baseArgs =
            call.args &&
            typeof call.args === "object" &&
            !Array.isArray(call.args)
              ? (call.args as Record<string, unknown>)
              : {};
          try {
            const limitRaw = baseArgs.limit;
            const limit =
              typeof limitRaw === "number" && Number.isFinite(limitRaw)
                ? Math.min(Math.max(Math.trunc(limitRaw), 1), 2000)
                : 500;
            const result = await fetchAecdmElementsByCategory({
              accessToken: auth.session.accessToken,
              hubId: selectedHubId,
              dmProjectId: selectedProjectId,
              modelUrn: body.selectedModelUrn,
              category:
                typeof baseArgs.category === "string"
                  ? baseArgs.category
                  : undefined,
              family:
                typeof baseArgs.family === "string"
                  ? baseArgs.family
                  : undefined,
              type:
                typeof baseArgs.type === "string" ? baseArgs.type : undefined,
              limit,
              product_prefix: parseProductPrefix(baseArgs.product_prefix),
            });
            const dbIds = result.elements
              .map((e) => e.dbId)
              .filter((id): id is number => id != null);
            externalContext =
              `${externalContext}\nGET_ELEMENTS_BY_CATEGORY: ${JSON.stringify({
                count: result.count,
                dbIds: dbIds.slice(0, 300),
                preview: result.elements.slice(0, 20).map((e) => ({
                  dbId: e.dbId,
                  category: e.category,
                  family: e.family,
                  type: e.type,
                  controlMark: e.controlMark,
                  externalId: e.externalId,
                })),
              })}`.trim();
          } catch (error) {
            externalContext =
              `${externalContext}\nGET_ELEMENTS_BY_CATEGORY_ERROR: ${JSON.stringify({
                message:
                  error instanceof Error ? error.message : "Unknown error",
              })}`.trim();
          }
        }

        if (call.tool === "select_elements") {
          const baseArgs =
            call.args &&
            typeof call.args === "object" &&
            !Array.isArray(call.args)
              ? (call.args as Record<string, unknown>)
              : {};
          const dbIds = parseToolDbIds(baseArgs.dbIds);
          if (dbIds.length > 0) {
            toolViewerActions.push({
              type: "viewer.selectDbIds",
              dbIds,
              clearFirst: baseArgs.clearFirst !== false,
              fitToView: baseArgs.zoomToSelection === true,
            });
          }
        }

      }
    } catch (error) {
      log("warn", "local-agent-tool-plan-failed", {
        requestId,
        details: error instanceof Error ? error.message : "Unknown error",
      });
    }
  }

  if (!externalContext.includes("MCP_READ_CONTEXT")) {
    const localMcpRead = mcpGetElementParameters({
      selectedElements,
      dbIds: selectedDbIds,
      parameterQueries: [
        "CONTROL_MARK",
        "CONTROL_NUMBER",
        "DIM_*",
        "IDENTITY_DESCRIPTION",
        "CONSTRUCTION_PRODUCT",
        "DESIGN_NUMBER",
        "WIDTH",
        "HEIGHT",
        "LENGTH",
      ],
    });
    externalContext = `${externalContext}\nMCP_READ_CONTEXT: ${JSON.stringify(
      localMcpRead.slice(0, 3),
    )}`.trim();
  }
  const agentContext = {
    source: "local-agent",
    note: "Local agent context from viewer selection and APS APIs.",
    workspaceMode,
    selectedCount,
    selectedDbIds,
    selectedElements: selectedElements.map((el) => ({
      dbId: el.dbId,
      name: el.name,
      externalId: el.externalId,
    })),
  };
  externalContext = `${externalContext}\nAGENT_CONTEXT: ${JSON.stringify(agentContext)}`.trim();
  if (productAnalysis) {
    const analysisContext = {
      rulesText: productAnalysis.rulesText ?? "",
      selectedDesignFile: productAnalysis.selectedDesignFile ?? null,
      selectedProduct: productAnalysis.selectedProduct ?? null,
      analysisInputs: productAnalysis.analysisInputs ?? {},
      selectionContext: productAnalysis.selectionContext ?? {},
    };
    externalContext = `${externalContext}\nPRODUCT_ANALYSIS_CONTEXT: ${JSON.stringify(
      analysisContext,
    )}`.trim();
  }

  try {
    if (hasAnyAiProviderKey()) {
      const aiIntent = await resolveViewerIntent(message, selectedModelName, {
        provider: aiProvider,
        model: aiModel,
        assistantMode,
        selectedCount,
        selectedDbIds,
        selectedElements,
        chatHistory: body.chatHistory,
        externalContext: externalContext.trim(),
      });
      actions = [...toolViewerActions, ...aiIntent.actions];
      modelQueryRequested =
        aiIntent.requestModelViews || modelViewsFromTool.length > 0;
      responseText = aiIntent.message;
    } else {
      const fallback = fallbackIntent(message);
      actions = [...toolViewerActions, ...fallback.actions];
      modelQueryRequested = fallback.modelQueryRequested;
      responseText = fallback.message;
    }
  } catch (error) {
    log("warn", "chat-ai-intent-fallback", {
      requestId,
      details: error instanceof Error ? error.message : "Unknown error",
    });
    const fallback = fallbackIntent(message);
    actions = [...toolViewerActions, ...fallback.actions];
    modelQueryRequested = fallback.modelQueryRequested;
    responseText = fallback.message;
  }

  if (modelViewsFromTool.length > 0) {
    queryResult.views = modelViewsFromTool;
  } else if (modelQueryRequested && body.selectedModelUrn) {
    try {
      const metadata = await getModelMetadata(
        body.selectedModelUrn,
        auth.session.accessToken,
      );
      queryResult.views = metadata.data.metadata.map((item) => ({
        guid: item.guid,
        name: item.name,
        role: item.role,
      }));
    } catch (error) {
      log("warn", "chat-model-query-failed", {
        requestId,
        details: error instanceof Error ? error.message : "Unknown error",
      });
      queryResult.queryError =
        error instanceof Error ? error.message : "Metadata query failed";
    }
  }

  if (!responseText) {
    responseText =
      actions.length > 0
        ? `Understood. I prepared ${actions.length} viewer action(s).`
        : "I can help query and control the viewer.";
  }

  if ("views" in queryResult && Array.isArray(queryResult.views)) {
    responseText += ` I also retrieved ${queryResult.views.length} model view(s).`;
  }
  if (issuesSummaryText) {
    responseText += ` ${issuesSummaryText}`;
  }

  const response = NextResponse.json({
    message: responseText,
    actions,
    queryResult,
    requestId,
  });
  for (const cookie of auth.response.cookies.getAll()) {
    response.cookies.set(cookie);
  }
  log("info", "chat-response", {
    requestId,
    provider: aiProvider,
    model: aiModel,
    assistantMode,
    actions: actions.length,
    modelQueryRequested,
    responseText: shouldLogFull() ? responseText : undefined,
    queryResult: shouldLogFull() ? queryResult : undefined,
  });
  return response;
}

