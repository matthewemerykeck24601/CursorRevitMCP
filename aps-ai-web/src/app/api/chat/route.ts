import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import {
  buildVersionOssGetArgument,
  getRevitCloudModelInfo,
  getModelMetadata,
  queryAecDataModel,
} from "@/lib/aps";
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
import {
  pollDesignAutomationStatusContract,
  type PollDesignAutomationStatusPayload,
} from "@/lib/da-poll-contract";
import { log } from "@/lib/logger";
import { getRequestId } from "@/lib/request";
import { mcpGetElementParameters } from "@/lib/mcp-local-tools";
import {
  createProjectIssue,
  getIssuesContainerId,
  listProjectIssues,
} from "@/lib/aps-issues";
import {
  addUsersToProjectsByNumber,
  getHubReferenceCacheStatus,
  listHubRoleCache,
  seedHubReferenceCache,
  seedHubRoleCache,
} from "@/lib/aps-admin";
import { createRevitCloudWorksharedModel } from "@/lib/revit-cloud-model-create";
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
import {
  buildDiscoveryCachedSelection,
  filterAecdmRowsForDiscovery,
  parseDiscoveryCachedSelectionFromClient,
  subsetDiscoveryByDbIds,
  toDaCachedSelectionPayload,
  type DiscoveryCachedSelection,
  type DiscoveryProvenanceInput,
} from "@/lib/discovery-cached-selection";
import type { AecdmElementListRow } from "@/lib/aecdm-get-elements";
import {
  augmentSkipAnalysisTriggerArgs,
  shouldAutoExecuteDesignAutomation,
  shouldAutoPollDaFollowUp,
} from "@/lib/chat-da-intent";
import {
  lookupRecentDaWorkitemByCacheId,
  registerDaWorkitemForCacheId,
} from "@/lib/da-recent-workitem-cache";

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
  workspaceMode?: "model" | "product-analysis" | "admin";
  /** Supplemental BIM-selection table context (summary + sample rows). */
  bimSelectionContext?: {
    count?: number;
    withControlMark?: number;
    withControlNumber?: number;
    sample?: Array<{
      elementId?: string;
      category?: string;
      family?: string;
      type?: string;
      controlMark?: string;
      controlNumber?: string;
    }>;
  };
  /** Last discovery payload from prior chat turn — resend for DA / follow-ups. */
  discoveryCachedSelection?: DiscoveryCachedSelection | Record<string, unknown>;
  /** Last Design Automation workitem from a prior submit (client resends for status polls). */
  lastDaJob?: {
    workitem_id: string;
    submitted_at?: string;
    cache_id?: string;
    operation?: string;
  };
  /** Model-load DA route hint from /api/aps/da-config-check. */
  daExecutionContext?: {
    resolvedActivityId?: string;
    resolvedActivitySource?: string;
    revitVersionMajor?: number;
    cloudModel?: {
      region?: string;
      projectGuid?: string;
      modelGuid?: string;
      revitVersionMajor?: number;
    };
  };
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
type NormalizedDaExecutionContext = {
  resolvedActivityId?: string;
  resolvedActivitySource?: string;
  revitVersionMajor?: number;
  cloud_model?: {
    region: string;
    projectGuid: string;
    modelGuid: string;
    revitVersionMajor?: number;
  };
};
type NormalizedBimSelectionContext = {
  count: number;
  withControlMark: number;
  withControlNumber: number;
  sample: Array<{
    elementId: string;
    category: string;
    family: string;
    type: string;
    controlMark: string;
    controlNumber: string;
  }>;
};

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

function parseProductPrefixes(raw: unknown): AecdmProductPrefix[] {
  if (raw === "WPA" || raw === "WPB" || raw === "CLA" || raw === "COLUMN" || raw === "ALL") {
    return [raw];
  }
  if (typeof raw !== "string") return [];
  const normalized = raw.trim().toUpperCase();
  if (!normalized) return [];
  if (normalized === "METROWALL" || normalized === "METROWALLS") {
    return ["WPA", "WPB"];
  }
  const tokens = normalized
    .split(/[|,;/]+/)
    .map((t) => t.trim())
    .filter(Boolean);
  const out: AecdmProductPrefix[] = [];
  for (const token of tokens) {
    if (token === "ALL") return ["ALL"];
    const parsed = parseProductPrefix(token);
    if (parsed && !out.includes(parsed)) out.push(parsed);
  }
  return out;
}

function mergeAecdmRows(rows: AecdmElementListRow[]): AecdmElementListRow[] {
  const seen = new Set<string>();
  const out: AecdmElementListRow[] = [];
  for (const row of rows) {
    const dbKey =
      row.dbId != null && Number.isFinite(Number(row.dbId))
        ? `db:${Math.trunc(Number(row.dbId))}`
        : "";
    const extKey =
      typeof row.externalId === "string" && row.externalId.trim()
        ? `ext:${row.externalId.trim()}`
        : "";
    const key = dbKey || extKey;
    if (!key) continue;
    if (seen.has(key)) continue;
    seen.add(key);
    out.push(row);
  }
  return out;
}

function decodeViewerUrn(viewerUrn: string): string {
  const normalized = viewerUrn.replace(/-/g, "+").replace(/_/g, "/");
  const pad = normalized.length % 4 === 0 ? "" : "=".repeat(4 - (normalized.length % 4));
  return Buffer.from(normalized + pad, "base64").toString("utf8");
}

function resolveModelVersionId(body: ChatRequest): string {
  const fromSelectionContext =
    typeof body.productAnalysis?.selectionContext?.modelVersionId === "string"
      ? body.productAnalysis.selectionContext.modelVersionId.trim()
      : "";
  if (fromSelectionContext) return fromSelectionContext;
  const viewerUrn =
    typeof body.selectedModelUrn === "string" ? body.selectedModelUrn.trim() : "";
  if (!viewerUrn) return "";
  try {
    const decoded = decodeViewerUrn(viewerUrn);
    return decoded.trim();
  } catch {
    return "";
  }
}

function normalizeDaExecutionContext(
  raw: unknown,
): NormalizedDaExecutionContext | undefined {
  if (!raw || typeof raw !== "object" || Array.isArray(raw)) return undefined;
  const obj = raw as Record<string, unknown>;
  const resolvedActivityId =
    typeof obj.resolvedActivityId === "string" ? obj.resolvedActivityId.trim() : "";
  const resolvedActivitySource =
    typeof obj.resolvedActivitySource === "string"
      ? obj.resolvedActivitySource.trim()
      : "";
  const revitVersionRaw = Number(obj.revitVersionMajor);
  const cloudRaw =
    obj.cloudModel && typeof obj.cloudModel === "object" && !Array.isArray(obj.cloudModel)
      ? (obj.cloudModel as Record<string, unknown>)
      : null;
  const cloudModel =
    cloudRaw &&
    typeof cloudRaw.region === "string" &&
    cloudRaw.region.trim() &&
    typeof cloudRaw.projectGuid === "string" &&
    cloudRaw.projectGuid.trim() &&
    typeof cloudRaw.modelGuid === "string" &&
    cloudRaw.modelGuid.trim()
      ? {
          region: cloudRaw.region.trim(),
          projectGuid: cloudRaw.projectGuid.trim(),
          modelGuid: cloudRaw.modelGuid.trim(),
          ...(Number.isFinite(Number(cloudRaw.revitVersionMajor))
            ? { revitVersionMajor: Math.trunc(Number(cloudRaw.revitVersionMajor)) }
            : {}),
        }
      : undefined;
  if (!resolvedActivityId && !resolvedActivitySource && !cloudModel && !Number.isFinite(revitVersionRaw)) {
    return undefined;
  }
  return {
    ...(resolvedActivityId ? { resolvedActivityId } : {}),
    ...(resolvedActivitySource ? { resolvedActivitySource } : {}),
    ...(Number.isFinite(revitVersionRaw)
      ? { revitVersionMajor: Math.trunc(revitVersionRaw) }
      : {}),
    ...(cloudModel ? { cloud_model: cloudModel } : {}),
  };
}

function normalizeBimSelectionContext(
  raw: unknown,
): NormalizedBimSelectionContext | undefined {
  if (!raw || typeof raw !== "object" || Array.isArray(raw)) return undefined;
  const obj = raw as Record<string, unknown>;
  const countRaw = Number(obj.count);
  const withControlMarkRaw = Number(obj.withControlMark);
  const withControlNumberRaw = Number(obj.withControlNumber);
  const sampleRaw = Array.isArray(obj.sample) ? obj.sample : [];
  const sample = sampleRaw
    .slice(0, 24)
    .map((row) => {
      if (!row || typeof row !== "object" || Array.isArray(row)) return null;
      const r = row as Record<string, unknown>;
      return {
        elementId: typeof r.elementId === "string" ? r.elementId.trim() : "",
        category: typeof r.category === "string" ? r.category.trim() : "",
        family: typeof r.family === "string" ? r.family.trim() : "",
        type: typeof r.type === "string" ? r.type.trim() : "",
        controlMark: typeof r.controlMark === "string" ? r.controlMark.trim() : "",
        controlNumber:
          typeof r.controlNumber === "string" ? r.controlNumber.trim() : "",
      };
    })
    .filter((x): x is NonNullable<typeof x> => x != null);
  if (sample.length === 0 && !Number.isFinite(countRaw)) return undefined;
  return {
    count: Number.isFinite(countRaw) ? Math.max(0, Math.trunc(countRaw)) : sample.length,
    withControlMark: Number.isFinite(withControlMarkRaw)
      ? Math.max(0, Math.trunc(withControlMarkRaw))
      : 0,
    withControlNumber: Number.isFinite(withControlNumberRaw)
      ? Math.max(0, Math.trunc(withControlNumberRaw))
      : 0,
    sample,
  };
}

function buildDiscoveryFromViewerSelection(
  selectedElements: ChatRequest["selectedElements"] | undefined,
  provenance: DiscoveryProvenanceInput,
): DiscoveryCachedSelection | null {
  if (!Array.isArray(selectedElements) || selectedElements.length === 0) return null;
  const rows: AecdmElementListRow[] = [];
  for (const el of selectedElements) {
    if (!el || typeof el !== "object") continue;
    const dbIdRaw = typeof el.dbId === "number" ? el.dbId : Number(el.dbId);
    const dbId = Number.isFinite(dbIdRaw) ? Math.trunc(dbIdRaw) : null;
    const externalId =
      typeof el.externalId === "string" && el.externalId.trim()
        ? el.externalId.trim()
        : null;
    if (dbId == null && !externalId) continue;
    rows.push({
      dbId,
      externalId,
      category: undefined,
      family: el.name,
      type: undefined,
      controlMark: undefined,
    });
  }
  if (rows.length === 0) return null;
  return buildDiscoveryCachedSelection(
    rows,
    provenance,
    "viewer-selection-fallback",
    "future_edit",
  );
}

type LastDaJobPayload = {
  workitem_id: string;
  submitted_at?: string;
  cache_id?: string;
  operation?: string;
};

function normalizeLastDaJob(raw: unknown): LastDaJobPayload | undefined {
  if (!raw || typeof raw !== "object" || Array.isArray(raw)) return undefined;
  const o = raw as Record<string, unknown>;
  const id = typeof o.workitem_id === "string" ? o.workitem_id.trim() : "";
  if (!id) return undefined;
  const operation =
    typeof o.operation === "string" && o.operation.trim()
      ? o.operation.trim()
      : undefined;
  return {
    workitem_id: id,
    submitted_at:
      typeof o.submitted_at === "string" ? o.submitted_at : undefined,
    cache_id: typeof o.cache_id === "string" ? o.cache_id : undefined,
    ...(operation ? { operation } : {}),
  };
}

function lastDaJobFromDiscoveryPending(
  d: DiscoveryCachedSelection | null,
): LastDaJobPayload | undefined {
  const pw = d?.pending_workitem;
  if (!pw?.workitem_id?.trim()) return undefined;
  return {
    workitem_id: pw.workitem_id.trim(),
    submitted_at: pw.submitted_at,
    cache_id: pw.cache_id ?? d?.cache_id,
    ...(pw.operation ? { operation: pw.operation } : {}),
  };
}

function resolveDaWorkitemIdForPoll(
  fromToolArgs: string,
  lastJob: LastDaJobPayload | undefined,
  discovery: DiscoveryCachedSelection | null,
): string {
  const a = fromToolArgs.trim();
  if (a) return a;
  if (lastJob?.workitem_id?.trim()) return lastJob.workitem_id.trim();
  const pend = discovery?.pending_workitem?.workitem_id?.trim();
  if (pend) return pend;
  const cid = discovery?.cache_id?.trim();
  if (cid) {
    const hit = lookupRecentDaWorkitemByCacheId(cid);
    if (hit?.workitem_id?.trim()) return hit.workitem_id.trim();
  }
  return "";
}

function recordDaSubmitOnSession(
  workitemId: string,
  operation: string | undefined,
  discovery: DiscoveryCachedSelection | null,
  assignLast: (row: LastDaJobPayload) => void,
  requestId?: string,
): void {
  if (!workitemId || workitemId.startsWith("da-stub")) return;
  const submitted_at = new Date().toISOString();
  assignLast({
    workitem_id: workitemId,
    submitted_at,
    cache_id: discovery?.cache_id,
    ...(operation ? { operation } : {}),
  });
  registerDaWorkitemForCacheId(discovery?.cache_id, workitemId, submitted_at);
  log("info", "da-workitem-submitted", {
    requestId,
    workitem_id: workitemId,
    cache_id: discovery?.cache_id ?? null,
    operation: operation ?? null,
  });
  if (discovery) {
    discovery.pending_workitem = {
      workitem_id: workitemId,
      submitted_at,
      operation,
      cache_id: discovery.cache_id,
    };
  }
}

function clearDiscoveryPendingAfterTerminalPoll(
  discovery: DiscoveryCachedSelection | null,
  polledWorkitemId: string,
  status: string | undefined,
): void {
  if (!discovery?.pending_workitem?.workitem_id) return;
  if (discovery.pending_workitem.workitem_id !== polledWorkitemId) return;
  const st = (status ?? "").toLowerCase();
  if (st === "success" || st.startsWith("failed") || st === "cancelled") {
    delete discovery.pending_workitem;
  }
}

function appendDaPollToExternalContext(
  ctx: string,
  pollPayload: PollDesignAutomationStatusPayload,
): string {
  let next = `${ctx}\nPRECAST_DA_POLL_RESULT: ${JSON.stringify(pollPayload)}`.trim();
  if (pollPayload.da_audit_summary?.one_liner) {
    next = `${next}\nDA_AUDIT_SUMMARY: ${pollPayload.da_audit_summary.one_liner}`.trim();
  }
  if (pollPayload.da_audit_detail?.trim()) {
    next = `${next}\nDA_AUDIT_DETAIL: ${pollPayload.da_audit_detail.trim().slice(0, 2500)}`.trim();
  }
  if (pollPayload.forge_status_snippet?.trim()) {
    next = `${next}\nDA_FORGE_STATUS_SNIPPET: ${pollPayload.forge_status_snippet.trim().slice(0, 1200)}`.trim();
  }
  if (pollPayload.message?.trim()) {
    next = `${next}\nDA_POLL_MESSAGE: ${pollPayload.message.trim()}`.trim();
  }
  if (pollPayload.execution_assistant_hint?.trim()) {
    next = `${next}\nDA_EXECUTION_HINT: ${pollPayload.execution_assistant_hint.trim()}`.trim();
  }
  return next;
}

function buildSelectionRulesLabel(baseArgs: Record<string, unknown>): string {
  const parts: string[] = [];
  const cat = typeof baseArgs.category === "string" ? baseArgs.category : "";
  const fam = typeof baseArgs.family === "string" ? baseArgs.family : "";
  const typ = typeof baseArgs.type === "string" ? baseArgs.type : "";
  if (cat) parts.push(`category=${cat}`);
  if (fam) parts.push(`family=${fam}`);
  if (typ) parts.push(`type=${typ}`);
  if (baseArgs.product_prefix != null) {
    parts.push(`product_prefix=${String(baseArgs.product_prefix)}`);
  }
  const nc = String(
    baseArgs.name_contains ?? baseArgs.nameContains ?? "",
  ).trim();
  if (nc) parts.push(`name contains "${nc}"`);
  const cmp = String(
    baseArgs.control_mark_prefix ?? baseArgs.controlMarkPrefix ?? "",
  ).trim();
  if (cmp) parts.push(`CONTROL_MARK prefix "${cmp}"`);
  const fc = String(
    baseArgs.family_contains ?? baseArgs.familyContains ?? "",
  ).trim();
  if (fc) parts.push(`family contains "${fc}"`);
  return parts.join("; ") || "AEC Data Model element query";
}

function dedupeViewerActions(actions: ChatToolAction[]): ChatToolAction[] {
  const seen = new Set<string>();
  const out: ChatToolAction[] = [];
  for (const action of actions) {
    let key = action.type;
    if (action.type === "viewer.searchAndSelectMetromontPieces") {
      key = [
        action.type,
        action.pieceKind,
        action.query.trim().toLowerCase(),
        action.clearFirst === false ? "keep" : "clear",
        action.fitToView === true ? "fit" : "nofit",
        String(action.maxSearchMatches ?? ""),
      ].join("|");
    } else if (action.type === "viewer.selectDbIds") {
      key = [
        action.type,
        action.clearFirst === false ? "keep" : "clear",
        action.fitToView === true ? "fit" : "nofit",
        action.dbIds.slice(0, 800).join(","),
      ].join("|");
    } else if (action.type === "viewer.isolateDbIds") {
      key = [action.type, action.dbIds.slice(0, 800).join(",")].join("|");
    } else if (
      action.type === "viewer.search" ||
      action.type === "viewer.isolateByQuery" ||
      action.type === "viewer.hideByQuery"
    ) {
      key = [action.type, action.query.trim().toLowerCase()].join("|");
    } else if (action.type === "viewer.setGhosting") {
      key = [action.type, action.enabled ? "1" : "0"].join("|");
    }
    if (seen.has(key)) continue;
    seen.add(key);
    out.push(action);
  }
  return out;
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
    ? body.selectedDbIds.filter((v) => Number.isFinite(v)).slice(0, 500)
    : [];
  const selectedElements = normalizeElements(body.selectedElements).slice(0, 300);
  const selectedHubId = body.selectedHubId;
  const selectedProjectId = body.selectedProjectId;
  const selectedItemId = body.selectedItemId;
  const selectedModelVersionId = resolveModelVersionId(body);
  const clientDaExecutionContext = normalizeDaExecutionContext(
    body.daExecutionContext,
  );
  const bimSelectionContext = normalizeBimSelectionContext(
    body.bimSelectionContext,
  );
  const aiProvider = body.aiProvider ?? env.aiProvider;
  const aiModel = body.aiModel ?? "";
  const assistantMode = "agent";
  const workspaceMode = body.workspaceMode ?? "model";
  const productAnalysis = body.productAnalysis;
  const discoveryProvenance = (): DiscoveryProvenanceInput => ({
    modelUrn: body.selectedModelUrn,
    hubId: selectedHubId,
    projectId: selectedProjectId,
    itemId: selectedItemId,
    publishedVersionId:
      (typeof productAnalysis?.selectionContext?.modelVersionId === "string"
        ? productAnalysis.selectionContext.modelVersionId
        : undefined) ?? selectedItemId,
  });
  let actions: ChatToolAction[] = [];
  const toolViewerActions: ChatToolAction[] = [];
  let modelQueryRequested = false;
  let responseText = "";
  const queryResult: Record<string, unknown> = {};
  let externalContext = "";

  let activeDiscovery: DiscoveryCachedSelection | null =
    parseDiscoveryCachedSelectionFromClient(body.discoveryCachedSelection);
  if (!activeDiscovery) {
    activeDiscovery = buildDiscoveryFromViewerSelection(
      body.selectedElements,
      discoveryProvenance(),
    );
  }
  let lastAecdmRows: AecdmElementListRow[] = [];
  let discoveryBuiltFromInspectThisTurn = false;
  let selectionToolRequestedThisTurn = false;
  let selectionToolAppliedThisTurn = false;
  let selectionToolErrorThisTurn = "";
  let lastDaJobForResponse = normalizeLastDaJob(body.lastDaJob);
  if (!lastDaJobForResponse?.workitem_id) {
    const fromPending = lastDaJobFromDiscoveryPending(activeDiscovery);
    if (fromPending) lastDaJobForResponse = fromPending;
  }
  let daSubmittedThisTurn = false;
  let daInputFileResolved = false;
  let daInputFileArg: { url: string; headers: Record<string, string> } | null = null;
  let daCloudModelResolved = Boolean(clientDaExecutionContext?.cloud_model);
  let daCloudModelArg:
    | { region: string; projectGuid: string; modelGuid: string; revitVersionMajor?: number }
    | null = clientDaExecutionContext?.cloud_model ?? null;
  const resolveDaInputFileArg = async () => {
    if (daInputFileResolved) return daInputFileArg;
    daInputFileResolved = true;
    if (!selectedProjectId || !selectedModelVersionId) return null;
    try {
      const arg = await buildVersionOssGetArgument({
        accessToken: auth.session.accessToken,
        projectId: selectedProjectId,
        versionId: selectedModelVersionId,
      });
      daInputFileArg = arg;
      return daInputFileArg;
    } catch (error) {
      log("warn", "da-input-file-resolve-failed", {
        requestId,
        projectId: selectedProjectId,
        versionId: selectedModelVersionId,
        details: error instanceof Error ? error.message : "Unknown error",
      });
      return null;
    }
  };
  const resolveDaCloudModelArg = async () => {
    if (daCloudModelResolved) return daCloudModelArg;
    daCloudModelResolved = true;
    if (!selectedProjectId || !selectedModelVersionId) return null;
    try {
      const info = await getRevitCloudModelInfo({
        accessToken: auth.session.accessToken,
        projectId: selectedProjectId,
        versionId: selectedModelVersionId,
      });
      daCloudModelArg = info;
      return daCloudModelArg;
    } catch (error) {
      log("warn", "da-cloud-model-resolve-failed", {
        requestId,
        projectId: selectedProjectId,
        versionId: selectedModelVersionId,
        details: error instanceof Error ? error.message : "Unknown error",
      });
      return null;
    }
  };
  const attachDaExecutionContext = async (base: Record<string, unknown>) => {
    const shouldRequireCloudModel = Boolean(selectedProjectId && selectedModelVersionId);
    if (shouldRequireCloudModel) {
      // Guardrail: for central-model edit flows, do not silently fall back to local OSS input-file mode.
      base.require_cloud_model = true;
    }
    if (
      (base.activityId == null || String(base.activityId).trim() === "") &&
      clientDaExecutionContext?.resolvedActivityId
    ) {
      base.activityId = clientDaExecutionContext.resolvedActivityId;
    }
    const cloudArg = await resolveDaCloudModelArg();
    if (cloudArg) {
      base.cloud_model = cloudArg;
      base.adsk3legged_token = auth.session.accessToken;
    }
    if (base.input_file == null && base.inputFile == null) {
      const inputArg = await resolveDaInputFileArg();
      if (inputArg?.url) {
        base.input_file = inputArg;
      }
    }
  };

  if (activeDiscovery) {
    externalContext = `${externalContext}\nLAST_DISCOVERY_CACHED_SELECTION: ${JSON.stringify({
      cache_id: activeDiscovery.cache_id,
      count: activeDiscovery.externalIds.length,
      selection_rules: activeDiscovery.selection_rules,
      intended_operation: activeDiscovery.intended_operation,
      provenance: activeDiscovery.provenance,
      pending_workitem_id:
        activeDiscovery.pending_workitem?.workitem_id ?? null,
    })}`.trim();
  }

  if (lastDaJobForResponse?.workitem_id) {
    externalContext =
      `${externalContext}\nLAST_DA_JOB: ${JSON.stringify(lastDaJobForResponse)}`.trim();
  }
  if (clientDaExecutionContext?.resolvedActivityId || clientDaExecutionContext?.cloud_model) {
    externalContext = `${externalContext}\nDA_MODEL_ROUTING: ${JSON.stringify({
      activityId: clientDaExecutionContext?.resolvedActivityId ?? null,
      source: clientDaExecutionContext?.resolvedActivitySource ?? null,
      revitVersionMajor:
        clientDaExecutionContext?.revitVersionMajor ??
        clientDaExecutionContext?.cloud_model?.revitVersionMajor ??
        null,
      hasCloudModelContext: Boolean(clientDaExecutionContext?.cloud_model),
    })}`.trim();
  }
  if (bimSelectionContext) {
    externalContext = `${externalContext}\nBIM_SELECTION_CONTEXT: ${JSON.stringify({
      count: bimSelectionContext.count,
      withControlMark: bimSelectionContext.withControlMark,
      withControlNumber: bimSelectionContext.withControlNumber,
      sample: bimSelectionContext.sample,
    })}`.trim();
  }

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
      daRouting: clientDaExecutionContext
        ? {
            activityId: clientDaExecutionContext.resolvedActivityId,
            source: clientDaExecutionContext.resolvedActivitySource,
            revitVersionMajor:
              clientDaExecutionContext.revitVersionMajor ??
              clientDaExecutionContext.cloud_model?.revitVersionMajor,
            hasCloudModelContext: Boolean(clientDaExecutionContext.cloud_model),
          }
        : null,
      bimSelectionContext: bimSelectionContext
        ? {
            count: bimSelectionContext.count,
            withControlMark: bimSelectionContext.withControlMark,
            withControlNumber: bimSelectionContext.withControlNumber,
            sampleCount: bimSelectionContext.sample.length,
          }
        : null,
      workspaceMode,
      prompt: message,
    });
  }

  if (selectedHubId && selectedProjectId) {
    externalContext = `${externalContext}\nHUB_PROJECT_AVAILABLE`.trim();
  }
  if (workspaceMode === "admin") {
    externalContext = `${externalContext}\nWORKSPACE_MODE: admin`.trim();
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
          const daPayload = activeDiscovery
            ? toDaCachedSelectionPayload(activeDiscovery)
            : null;
          const payload = {
            success: true,
            source: activeDiscovery ? "discovery_session" : "viewer_selection_only",
            count: activeDiscovery
              ? activeDiscovery.externalIds.length
              : selectedElements.length,
            cache_id: activeDiscovery?.cache_id,
            cached_selection: daPayload,
            dbIds: activeDiscovery
              ? activeDiscovery.dbIds.slice(0, 300)
              : selectedDbIds.slice(0, 300),
            elements: snapshot,
            selection_rules: activeDiscovery?.selection_rules,
            note: activeDiscovery
              ? "Use GET_CACHED_SELECTION.cached_selection with trigger_design_automation_mark_update (skip_analysis + updates[]). External IDs are stable for Revit DA."
              : "No discovery session — only current viewer selection. Run get_elements_by_category or inspect_published_selection to build cached_selection with externalIds before cloud edits.",
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
        if (call.tool === "admin_add_users_to_projects") {
          const args =
            call.args && typeof call.args === "object" && !Array.isArray(call.args)
              ? (call.args as Record<string, unknown>)
              : {};
          if (!selectedHubId) {
            const payload = {
              success: false,
              error: "Select a hub first. Admin add-users requires selectedHubId.",
            };
            externalContext =
              `${externalContext}\nADMIN_ADD_USERS_RESULT: ${JSON.stringify(payload)}`.trim();
          } else {
            try {
              const projectNumbersRaw = Array.isArray(args.project_numbers)
                ? args.project_numbers
                : Array.isArray(args.projectNumbers)
                  ? args.projectNumbers
                  : [];
              const emailsRaw = Array.isArray(args.emails) ? args.emails : [];
              const roleIdsRaw = Array.isArray(args.role_ids)
                ? args.role_ids
                : Array.isArray(args.roleIds)
                  ? args.roleIds
                  : [];
              const roleNamesRaw = Array.isArray(args.role_names)
                ? args.role_names
                : Array.isArray(args.roleNames)
                  ? args.roleNames
                  : [];
              const productsRaw = Array.isArray(args.products) ? args.products : [];
              const businessUnitIdRaw =
                typeof args.business_unit_id === "string"
                  ? args.business_unit_id
                  : typeof args.businessUnitId === "string"
                    ? args.businessUnitId
                    : "";
              const businessUnitNameRaw =
                typeof args.business_unit_name === "string"
                  ? args.business_unit_name
                  : typeof args.businessUnitName === "string"
                    ? args.businessUnitName
                    : "";
              const cacheOnlyArg =
                args.cache_only ?? args.cacheOnly ?? undefined;
              const preferCacheOnly =
                cacheOnlyArg == null
                  ? projectNumbersRaw.length >= 5
                  : Boolean(cacheOnlyArg);
              const additionalPayload =
                args.additional_user_payload &&
                typeof args.additional_user_payload === "object" &&
                !Array.isArray(args.additional_user_payload)
                  ? (args.additional_user_payload as Record<string, unknown>)
                  : args.additionalUserPayload &&
                      typeof args.additionalUserPayload === "object" &&
                      !Array.isArray(args.additionalUserPayload)
                    ? (args.additionalUserPayload as Record<string, unknown>)
                    : undefined;
              const result = await addUsersToProjectsByNumber({
                accessToken: auth.session.accessToken,
                hubId: selectedHubId,
                projectNumbers: projectNumbersRaw.map((v) => String(v)),
                businessUnitId: businessUnitIdRaw || undefined,
                businessUnitName: businessUnitNameRaw || undefined,
                emails: emailsRaw.map((v) => String(v)),
                roleIds: roleIdsRaw.map((v) => String(v)),
                roleNames: roleNamesRaw.map((v) => String(v)),
                products: productsRaw
                  .filter((v) => v && typeof v === "object" && !Array.isArray(v))
                  .map((v) => v as Record<string, unknown>),
                region:
                  String(args.region ?? "US").toUpperCase() === "EMEA"
                    ? "EMEA"
                    : "US",
                dryRun: Boolean(args.dry_run ?? args.dryRun ?? false),
                cacheOnly: preferCacheOnly,
                additionalUserPayload: additionalPayload,
              });
              queryResult.adminAddUsers = result;
              externalContext =
                `${externalContext}\nADMIN_ADD_USERS_RESULT: ${JSON.stringify({
                  success: result.success,
                  requested_project_numbers: result.requested_project_numbers,
                  requested_business_unit: result.requested_business_unit,
                  requested_emails: result.requested_emails,
                  matched_projects: result.matched_projects.map((p) => ({
                    projectId: p.projectId,
                    projectNumber: p.projectNumber,
                    projectName: p.projectName,
                    businessUnitId: p.businessUnitId,
                    businessUnitName: p.businessUnitName,
                  })),
                  missing_project_numbers: result.missing_project_numbers,
                  attempted_assignments: result.attempted_assignments,
                  success_count: result.success_count,
                  failure_count: result.failure_count,
                  dry_run: result.dry_run,
                  project_cache_source: result.project_cache_source,
                  project_cache_updated: result.project_cache_updated,
                  project_cache_object_key: result.project_cache_object_key,
                  sample_results: result.results.slice(0, 10),
                })}`.trim();
            } catch (error) {
              const payload = {
                success: false,
                error: error instanceof Error ? error.message : "Unknown error",
              };
              queryResult.adminAddUsers = payload;
              externalContext =
                `${externalContext}\nADMIN_ADD_USERS_RESULT: ${JSON.stringify(payload)}`.trim();
            }
          }
        }
        if (call.tool === "admin_seed_role_cache") {
          const args =
            call.args && typeof call.args === "object" && !Array.isArray(call.args)
              ? (call.args as Record<string, unknown>)
              : {};
          if (!selectedHubId) {
            const payload = {
              success: false,
              error: "Select a hub first. Role cache seed requires selectedHubId.",
            };
            queryResult.adminSeedRoleCache = payload;
            externalContext =
              `${externalContext}\nADMIN_SEED_ROLE_CACHE_RESULT: ${JSON.stringify(payload)}`.trim();
          } else {
            try {
              const result = await seedHubRoleCache({
                accessToken: auth.session.accessToken,
                hubId: selectedHubId,
                region:
                  String(args.region ?? "US").toUpperCase() === "EMEA"
                    ? "EMEA"
                    : "US",
                maxProjects: Number.isFinite(Number(args.max_projects))
                  ? Number(args.max_projects)
                  : Number.isFinite(Number(args.maxProjects))
                    ? Number(args.maxProjects)
                    : undefined,
                createRequestIfMissing: Boolean(
                  args.create_request_if_missing ??
                    args.createRequestIfMissing ??
                    false,
                ),
              });
              queryResult.adminSeedRoleCache = result;
              externalContext =
                `${externalContext}\nADMIN_SEED_ROLE_CACHE_RESULT: ${JSON.stringify(result)}`.trim();
            } catch (error) {
              const payload = {
                success: false,
                error: error instanceof Error ? error.message : "Unknown error",
              };
              queryResult.adminSeedRoleCache = payload;
              externalContext =
                `${externalContext}\nADMIN_SEED_ROLE_CACHE_RESULT: ${JSON.stringify(payload)}`.trim();
            }
          }
        }
        if (call.tool === "admin_list_role_cache") {
          const args =
            call.args && typeof call.args === "object" && !Array.isArray(call.args)
              ? (call.args as Record<string, unknown>)
              : {};
          if (!selectedHubId) {
            const payload = {
              success: false,
              error: "Select a hub first. Role cache list requires selectedHubId.",
            };
            queryResult.adminListRoleCache = payload;
            externalContext =
              `${externalContext}\nADMIN_LIST_ROLE_CACHE_RESULT: ${JSON.stringify(payload)}`.trim();
          } else {
            try {
              const result = await listHubRoleCache({ hubId: selectedHubId });
              const limitRaw = Number(args.limit);
              const limit =
                Number.isFinite(limitRaw) && limitRaw > 0
                  ? Math.min(500, Math.trunc(limitRaw))
                  : 100;
              const sliced = {
                ...result,
                roles: result.roles.slice(0, limit),
                role_count_total: result.role_count,
                role_count_returned: Math.min(result.role_count, limit),
              };
              queryResult.adminListRoleCache = sliced;
              externalContext =
                `${externalContext}\nADMIN_LIST_ROLE_CACHE_RESULT: ${JSON.stringify(sliced)}`.trim();
            } catch (error) {
              const payload = {
                success: false,
                error: error instanceof Error ? error.message : "Unknown error",
              };
              queryResult.adminListRoleCache = payload;
              externalContext =
                `${externalContext}\nADMIN_LIST_ROLE_CACHE_RESULT: ${JSON.stringify(payload)}`.trim();
            }
          }
        }
        if (call.tool === "admin_seed_reference_cache") {
          const args =
            call.args && typeof call.args === "object" && !Array.isArray(call.args)
              ? (call.args as Record<string, unknown>)
              : {};
          if (!selectedHubId) {
            const payload = {
              success: false,
              error: "Select a hub first. Reference cache seed requires selectedHubId.",
            };
            queryResult.adminSeedReferenceCache = payload;
            externalContext =
              `${externalContext}\nADMIN_SEED_REFERENCE_CACHE_RESULT: ${JSON.stringify(payload)}`.trim();
          } else {
            try {
              const reference = await seedHubReferenceCache({
                accessToken: auth.session.accessToken,
                hubId: selectedHubId,
              });
              const includeRoleSeed = Boolean(
                args.include_role_seed ?? args.includeRoleSeed ?? false,
              );
              const roleSeed = includeRoleSeed
                ? await seedHubRoleCache({
                    accessToken: auth.session.accessToken,
                    hubId: selectedHubId,
                    region:
                      String(args.region ?? "US").toUpperCase() === "EMEA"
                        ? "EMEA"
                        : "US",
                  })
                : null;
              const payload = {
                reference_cache: reference,
                ...(roleSeed ? { role_cache: roleSeed } : {}),
              };
              queryResult.adminSeedReferenceCache = payload;
              externalContext =
                `${externalContext}\nADMIN_SEED_REFERENCE_CACHE_RESULT: ${JSON.stringify(payload)}`.trim();
            } catch (error) {
              const payload = {
                success: false,
                error: error instanceof Error ? error.message : "Unknown error",
              };
              queryResult.adminSeedReferenceCache = payload;
              externalContext =
                `${externalContext}\nADMIN_SEED_REFERENCE_CACHE_RESULT: ${JSON.stringify(payload)}`.trim();
            }
          }
        }
        if (call.tool === "admin_get_reference_cache_status") {
          const args =
            call.args && typeof call.args === "object" && !Array.isArray(call.args)
              ? (call.args as Record<string, unknown>)
              : {};
          if (!selectedHubId) {
            const payload = {
              success: false,
              error:
                "Select a hub first. Reference cache status requires selectedHubId.",
            };
            queryResult.adminReferenceCacheStatus = payload;
            externalContext =
              `${externalContext}\nADMIN_REFERENCE_CACHE_STATUS: ${JSON.stringify(payload)}`.trim();
          } else {
            try {
              const status = await getHubReferenceCacheStatus({
                hubId: selectedHubId,
                projectNumber:
                  typeof args.project_number === "string"
                    ? args.project_number
                    : typeof args.projectNumber === "string"
                      ? args.projectNumber
                      : undefined,
                projectId:
                  typeof args.project_id === "string"
                    ? args.project_id
                    : typeof args.projectId === "string"
                      ? args.projectId
                      : selectedProjectId,
                businessUnitId:
                  typeof args.business_unit_id === "string"
                    ? args.business_unit_id
                    : typeof args.businessUnitId === "string"
                      ? args.businessUnitId
                      : undefined,
                businessUnitName:
                  typeof args.business_unit_name === "string"
                    ? args.business_unit_name
                    : typeof args.businessUnitName === "string"
                      ? args.businessUnitName
                      : undefined,
                folderName:
                  typeof args.folder_name === "string"
                    ? args.folder_name
                    : typeof args.folderName === "string"
                      ? args.folderName
                      : undefined,
                limit: Number.isFinite(Number(args.limit))
                  ? Number(args.limit)
                  : undefined,
              });
              queryResult.adminReferenceCacheStatus = status;
              externalContext =
                `${externalContext}\nADMIN_REFERENCE_CACHE_STATUS: ${JSON.stringify(status)}`.trim();
            } catch (error) {
              const payload = {
                success: false,
                error: error instanceof Error ? error.message : "Unknown error",
              };
              queryResult.adminReferenceCacheStatus = payload;
              externalContext =
                `${externalContext}\nADMIN_REFERENCE_CACHE_STATUS: ${JSON.stringify(payload)}`.trim();
            }
          }
        }
        if (call.tool === "create_revit_cloud_workshared_model") {
          const args =
            call.args && typeof call.args === "object" && !Array.isArray(call.args)
              ? (call.args as Record<string, unknown>)
              : {};
          if (!selectedHubId) {
            const payload = {
              success: false,
              error:
                "Select a hub first. Revit cloud model creation requires selectedHubId.",
            };
            queryResult.createRevitCloudWorksharedModel = payload;
            externalContext =
              `${externalContext}\nCREATE_REVIT_CLOUD_WORKSHARED_MODEL_RESULT: ${JSON.stringify(payload)}`.trim();
          } else {
            try {
              const result = await createRevitCloudWorksharedModel({
                accessToken: auth.session.accessToken,
                hubId: selectedHubId,
                projectNumber:
                  typeof args.project_number === "string"
                    ? args.project_number
                    : typeof args.projectNumber === "string"
                      ? args.projectNumber
                      : undefined,
                projectId:
                  typeof args.project_id === "string"
                    ? args.project_id
                    : typeof args.projectId === "string"
                      ? args.projectId
                      : selectedProjectId,
                folderId:
                  typeof args.folder_id === "string"
                    ? args.folder_id
                    : typeof args.folderId === "string"
                      ? args.folderId
                      : undefined,
                modelName:
                  typeof args.model_name === "string"
                    ? args.model_name
                    : typeof args.modelName === "string"
                      ? args.modelName
                      : undefined,
                templateKey:
                  typeof args.template_key === "string"
                    ? args.template_key
                    : typeof args.templateKey === "string"
                      ? args.templateKey
                      : typeof args.template === "string"
                        ? args.template
                        : undefined,
                templateModelGuid:
                  typeof args.template_model_guid === "string"
                    ? args.template_model_guid
                    : typeof args.templateModelGuid === "string"
                      ? args.templateModelGuid
                      : undefined,
                templateProjectGuid:
                  typeof args.template_project_guid === "string"
                    ? args.template_project_guid
                    : typeof args.templateProjectGuid === "string"
                      ? args.templateProjectGuid
                      : undefined,
                region:
                  String(args.region ?? "US").toUpperCase() === "EMEA"
                    ? "EMEA"
                    : "US",
                enableWorksharing:
                  args.enable_worksharing == null && args.enableWorksharing == null
                    ? true
                    : Boolean(args.enable_worksharing ?? args.enableWorksharing),
                activityId:
                  typeof args.activity_id === "string"
                    ? args.activity_id
                    : typeof args.activityId === "string"
                      ? args.activityId
                      : undefined,
                dryRun: Boolean(args.dry_run ?? args.dryRun ?? false),
              });
              queryResult.createRevitCloudWorksharedModel = result;
              externalContext =
                `${externalContext}\nCREATE_REVIT_CLOUD_WORKSHARED_MODEL_RESULT: ${JSON.stringify(result)}`.trim();
            } catch (error) {
              const payload = {
                success: false,
                error: error instanceof Error ? error.message : "Unknown error",
              };
              queryResult.createRevitCloudWorksharedModel = payload;
              externalContext =
                `${externalContext}\nCREATE_REVIT_CLOUD_WORKSHARED_MODEL_RESULT: ${JSON.stringify(payload)}`.trim();
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
            const base =
              call.args && typeof call.args === "object" && !Array.isArray(call.args)
                ? { ...(call.args as Record<string, unknown>) }
                : {};
            const hasDiscovery =
              Boolean(activeDiscovery?.externalIds.length);
            const autoExec =
              hasDiscovery &&
              shouldAutoExecuteDesignAutomation(message, true) &&
              !/\b(analyze|verify marks|sameness|grouping)\b/i.test(message);
            if (autoExec) {
              base.confirm = true;
              if (!/\b(analyze|verify marks|sameness|grouping)\b/i.test(message)) {
                base.skip_analysis = true;
                if (!base.operation || String(base.operation).trim() === "") {
                  base.operation = "modify_parameters";
                }
              }
            }
            if (activeDiscovery?.cache_id && base.cache_id == null) {
              base.cache_id = activeDiscovery.cache_id;
            }
            await attachDaExecutionContext(base);
            const daSel =
              activeDiscovery && activeDiscovery.externalIds.length > 0
                ? toDaCachedSelectionPayload(activeDiscovery)
                : null;
            const aug = augmentSkipAnalysisTriggerArgs(message, base, daSel);
            if (aug.blocked) {
              externalContext =
                `${externalContext}\n${aug.blocked}`.trim();
              if (aug.userHint) {
                externalContext =
                  `${externalContext}\nDA_USER_HINT: ${aug.userHint}`.trim();
              }
            } else {
              const payload = await triggerDesignAutomationMarkUpdateContract(
                Object.keys(base).length > 0 ? base : { cache_id: "", confirm: false },
              );
              externalContext =
                `${externalContext}\nPRECAST_DA_MARK_UPDATE: ${JSON.stringify(payload)}`.trim();
              const p = payload as {
                workitem_submitted?: boolean;
                workitem_id?: string;
                status?: string;
                execution_assistant_hint?: string;
                da_audit_summary?: { one_liner?: string };
              };
              if (p.execution_assistant_hint) {
                externalContext =
                  `${externalContext}\nDA_EXECUTION_HINT: ${p.execution_assistant_hint}`.trim();
              }
              if (p.da_audit_summary?.one_liner) {
                externalContext =
                  `${externalContext}\nDA_AUDIT_SUMMARY: ${p.da_audit_summary.one_liner}`.trim();
              }
              const noCloudWrite =
                p.workitem_submitted === false ||
                (p.workitem_submitted === undefined && p.status === "stub");
              if (noCloudWrite) {
                externalContext =
                  `${externalContext}\nCLOUD_WRITE_TRUTH: The central Revit cloud model was NOT modified. Design Automation did not submit a workitem (disabled/stub or pre-confirm). Do NOT tell the user CONTROL_MARK was cleared in Revit or that syncing will show parameter changes. Quote PRECAST_DA_MARK_UPDATE message/note. Selection-based "clear marks" is not a live cloud write unless DA is enabled and the activity applies that change.`.trim();
              }
              if (p.workitem_submitted === true) {
                daSubmittedThisTurn = true;
                if (
                  typeof p.workitem_id === "string" &&
                  p.workitem_id &&
                  !p.workitem_id.startsWith("da-stub")
                ) {
                  const op =
                    typeof base.operation === "string"
                      ? base.operation.trim()
                      : undefined;
                  recordDaSubmitOnSession(
                    p.workitem_id,
                    op || "modify_parameters",
                    activeDiscovery,
                    (row) => {
                      lastDaJobForResponse = row;
                    },
                    requestId,
                  );
                }
              }
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
        if (call.tool === "poll_design_automation_status") {
          try {
            const base =
              call.args && typeof call.args === "object" && !Array.isArray(call.args)
                ? (call.args as Record<string, unknown>)
                : {};
            const fromArgs =
              typeof base.workitem_id === "string" ? base.workitem_id.trim() : "";
            const workitemId = resolveDaWorkitemIdForPoll(
              fromArgs,
              lastDaJobForResponse,
              activeDiscovery,
            );
            if (!workitemId) {
              externalContext =
                `${externalContext}\nPRECAST_DA_POLL_SKIPPED: ${JSON.stringify({
                  message:
                    "No workitem ID available to poll. Client should resend lastDaJob from the submit response, or discovery.pending_workitem after a DA submit.",
                })}`.trim();
            } else {
              const pollPayload = await pollDesignAutomationStatusContract({
                ...base,
                workitem_id: workitemId,
                cache_id: activeDiscovery?.cache_id,
              });
              clearDiscoveryPendingAfterTerminalPoll(
                activeDiscovery,
                pollPayload.workitem_id,
                pollPayload.status,
              );
              externalContext = appendDaPollToExternalContext(
                externalContext,
                pollPayload,
              );
            }
          } catch (error) {
            externalContext =
              `${externalContext}\nPRECAST_DA_POLL_ERROR: ${JSON.stringify({
                message:
                  error instanceof Error ? error.message : "Unknown error",
              })}`.trim();
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

        const isPublishedInspect =
          (call.tool === "get_elements_by_category" ||
            call.tool === "inspect_published_selection") &&
          selectedHubId &&
          selectedProjectId &&
          body.selectedModelUrn;

        if (isPublishedInspect) {
          selectionToolRequestedThisTurn = true;
          if (activeDiscovery?.selection_rules === "viewer-selection-fallback") {
            activeDiscovery = null;
          }
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
            const parsedPrefixes = parseProductPrefixes(baseArgs.product_prefix);
            const prefixesToQuery =
              parsedPrefixes.length > 0 ? parsedPrefixes : [undefined];
            let rawRows: AecdmElementListRow[] = [];
            for (const prefix of prefixesToQuery) {
              const partial = await fetchAecdmElementsByCategory({
                accessToken: auth.session.accessToken,
                hubId: selectedHubId,
                dmProjectId: selectedProjectId,
                modelUrn: body.selectedModelUrn as string,
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
                product_prefix: prefix,
              });
              rawRows = rawRows.concat(partial.elements);
            }
            const mergedRows = mergeAecdmRows(rawRows);
            const result = {
              success: true as const,
              count: mergedRows.length,
              elements: mergedRows,
            };
            const filtered = filterAecdmRowsForDiscovery(
              result.elements,
              baseArgs,
            );
            lastAecdmRows = filtered;
            const rulesLabel = buildSelectionRulesLabel(baseArgs);
            const intendedOp =
              typeof baseArgs.intended_operation === "string"
                ? baseArgs.intended_operation.trim()
                : "future_edit";
            activeDiscovery = buildDiscoveryCachedSelection(
              filtered,
              discoveryProvenance(),
              rulesLabel,
              intendedOp || "future_edit",
            );
            discoveryBuiltFromInspectThisTurn = true;
            const dbIdsForViewer = activeDiscovery.dbIds;
            const highlight =
              baseArgs.highlight_in_viewer !== false &&
              baseArgs.apply_viewer_selection !== false;
            const fit =
              baseArgs.fit_to_view === true ||
              baseArgs.zoom_to_selection === true ||
              baseArgs.zoomToSelection === true;
            if (highlight && dbIdsForViewer.length > 0) {
              selectionToolAppliedThisTurn = true;
              toolViewerActions.push({
                type: "viewer.selectDbIds",
                dbIds: dbIdsForViewer.slice(0, 500),
                clearFirst: baseArgs.clearFirst !== false,
                fitToView: fit,
              });
              if (baseArgs.isolate_in_viewer === true) {
                toolViewerActions.push({
                  type: "viewer.isolateDbIds",
                  dbIds: dbIdsForViewer.slice(0, 500),
                });
              }
            }
            const extCount = activeDiscovery.externalIds.length;
            if (!selectionToolAppliedThisTurn) {
              selectionToolErrorThisTurn =
                filtered.length === 0
                  ? "No matching wall-panel elements were found in the published model query."
                  : "Selection query returned rows but none were eligible for viewer selection.";
            }
            const discPayload = {
              success: extCount > 0 || dbIdsForViewer.length > 0,
              tool: call.tool,
              aecdm_row_count: result.count,
              after_filter_count: filtered.length,
              viewer_db_ids_queued: highlight ? dbIdsForViewer.length : 0,
              cached_selection: toDaCachedSelectionPayload(activeDiscovery),
              discovery_session: activeDiscovery,
              external_id_count: extCount,
              warning:
                extCount === 0 && dbIdsForViewer.length > 0
                  ? "Some elements have no External ID in AECDM — Design Automation cannot target them until External ID is present."
                  : filtered.length === 0
                    ? "No elements matched the query and filters — viewer selection was not changed."
                    : undefined,
            };
            externalContext =
              `${externalContext}\nDISCOVERY_CACHED_SELECTION: ${JSON.stringify(discPayload)}`.trim();
            externalContext =
              `${externalContext}\nDISCOVERY_SELECTION_APPLIED: ${Boolean(highlight && dbIdsForViewer.length > 0)}`.trim();
            externalContext =
              `${externalContext}\nGET_ELEMENTS_BY_CATEGORY: ${JSON.stringify({
                count: result.count,
                filtered_count: filtered.length,
                dbIds: dbIdsForViewer.slice(0, 300),
                preview: filtered.slice(0, 20).map((e) => ({
                  dbId: e.dbId,
                  category: e.category,
                  family: e.family,
                  type: e.type,
                  controlMark: e.controlMark,
                  externalId: e.externalId,
                })),
              })}`.trim();
          } catch (error) {
            selectionToolErrorThisTurn =
              error instanceof Error ? error.message : "Selection query failed.";
            externalContext =
              `${externalContext}\nGET_ELEMENTS_BY_CATEGORY_ERROR: ${JSON.stringify({
                message:
                  error instanceof Error ? error.message : "Unknown error",
              })}`.trim();
          }
        }

        if (call.tool === "select_elements") {
          selectionToolRequestedThisTurn = true;
          const baseArgs =
            call.args &&
            typeof call.args === "object" &&
            !Array.isArray(call.args)
              ? (call.args as Record<string, unknown>)
              : {};
          const dbIds = parseToolDbIds(baseArgs.dbIds);
          const allowDiscoverySubset =
            baseArgs.refine_discovery === true ||
            baseArgs.apply_to_cached_selection === true ||
            !discoveryBuiltFromInspectThisTurn;
          if (dbIds.length > 0) {
            const fit =
              baseArgs.zoomToSelection === true || baseArgs.fit_to_view === true;
            toolViewerActions.push({
              type: "viewer.selectDbIds",
              dbIds,
              clearFirst: baseArgs.clearFirst !== false,
              fitToView: fit,
            });
            selectionToolAppliedThisTurn = true;
            if (baseArgs.isolate_in_viewer === true) {
              toolViewerActions.push({
                type: "viewer.isolateDbIds",
                dbIds,
              });
            }
            if (lastAecdmRows.length > 0 && allowDiscoverySubset) {
              const rows = lastAecdmRows.filter(
                (r) =>
                  r.dbId != null &&
                  dbIds.includes(Math.trunc(r.dbId as number)),
              );
              if (rows.length > 0) {
                const intendedOp =
                  typeof baseArgs.intended_operation === "string"
                    ? baseArgs.intended_operation.trim()
                    : activeDiscovery?.intended_operation ?? "future_edit";
                activeDiscovery = buildDiscoveryCachedSelection(
                  rows,
                  discoveryProvenance(),
                  `${buildSelectionRulesLabel(baseArgs)} (select_elements)`,
                  intendedOp || "future_edit",
                );
                externalContext =
                  `${externalContext}\nDISCOVERY_CACHED_SELECTION: ${JSON.stringify({
                    success: true,
                    tool: "select_elements",
                    cached_selection: toDaCachedSelectionPayload(activeDiscovery),
                    discovery_session: activeDiscovery,
                  })}`.trim();
              } else if (activeDiscovery) {
                activeDiscovery = subsetDiscoveryByDbIds(activeDiscovery, dbIds);
                externalContext =
                  `${externalContext}\nDISCOVERY_CACHED_SELECTION: ${JSON.stringify({
                    success: activeDiscovery.externalIds.length > 0,
                    tool: "select_elements",
                    cached_selection: toDaCachedSelectionPayload(activeDiscovery),
                    discovery_session: activeDiscovery,
                    note: "Subset of prior discovery session by dbIds.",
                  })}`.trim();
              }
            } else if (activeDiscovery && allowDiscoverySubset) {
              activeDiscovery = subsetDiscoveryByDbIds(activeDiscovery, dbIds);
              externalContext =
                `${externalContext}\nDISCOVERY_CACHED_SELECTION: ${JSON.stringify({
                  success: activeDiscovery.externalIds.length > 0,
                  tool: "select_elements",
                  cached_selection: toDaCachedSelectionPayload(activeDiscovery),
                  discovery_session: activeDiscovery,
                })}`.trim();
            } else if (!allowDiscoverySubset && activeDiscovery) {
              externalContext =
                `${externalContext}\nSELECT_ELEMENTS_DISCOVERY_NOT_RESCOPED: ${JSON.stringify({
                  reason:
                    "Skipped auto-rescoping cached_selection because discovery was built from inspect_published_selection in this same turn.",
                  dbIds_count: dbIds.length,
                  cached_external_ids: activeDiscovery.externalIds.length,
                })}`.trim();
            } else {
              externalContext =
                `${externalContext}\nSELECT_ELEMENTS_NO_DISCOVERY_CACHE: ${JSON.stringify({
                  message:
                    "Viewer selection updated but no AECDM row cache in this request — run inspect_published_selection / get_elements_by_category first for externalIds.",
                  dbIds: dbIds.slice(0, 100),
                })}`.trim();
            }
          } else {
            selectionToolErrorThisTurn =
              "Selection tool was called without valid dbIds, so nothing was selected.";
          }
        }

        const ranDa = externalContext.includes("PRECAST_DA_MARK_UPDATE");
        if (
          !ranDa &&
          activeDiscovery &&
          activeDiscovery.externalIds.length > 0 &&
          shouldAutoExecuteDesignAutomation(message, true) &&
          !/\b(analyze|verify marks|sameness|grouping)\b/i.test(message)
        ) {
          try {
            const autoBase: Record<string, unknown> = {
              confirm: true,
              skip_analysis: true,
              operation: "modify_parameters",
              cache_id: activeDiscovery.cache_id,
            };
            await attachDaExecutionContext(autoBase);
            const autoAug = augmentSkipAnalysisTriggerArgs(
              message,
              autoBase,
              toDaCachedSelectionPayload(activeDiscovery),
            );
            if (autoAug.blocked) {
              externalContext =
                `${externalContext}\n${autoAug.blocked}`.trim();
              if (autoAug.userHint) {
                externalContext =
                  `${externalContext}\nDA_USER_HINT: ${autoAug.userHint}`.trim();
              }
            } else {
              const autoPayload =
                await triggerDesignAutomationMarkUpdateContract(autoBase);
              externalContext =
                `${externalContext}\nPRECAST_DA_MARK_UPDATE: ${JSON.stringify(autoPayload)}`.trim();
              const ap = autoPayload as {
                workitem_submitted?: boolean;
                workitem_id?: string;
                status?: string;
                execution_assistant_hint?: string;
                da_audit_summary?: { one_liner?: string };
              };
              if (ap.execution_assistant_hint) {
                externalContext =
                  `${externalContext}\nDA_EXECUTION_HINT: ${ap.execution_assistant_hint}`.trim();
              }
              if (ap.da_audit_summary?.one_liner) {
                externalContext =
                  `${externalContext}\nDA_AUDIT_SUMMARY: ${ap.da_audit_summary.one_liner}`.trim();
              }
              const autoNoWrite =
                ap.workitem_submitted === false ||
                (ap.workitem_submitted === undefined && ap.status === "stub");
              if (ap.workitem_submitted === true) {
                daSubmittedThisTurn = true;
                if (
                  typeof ap.workitem_id === "string" &&
                  ap.workitem_id &&
                  !ap.workitem_id.startsWith("da-stub")
                ) {
                  recordDaSubmitOnSession(
                    ap.workitem_id,
                    "modify_parameters",
                    activeDiscovery,
                    (row) => {
                      lastDaJobForResponse = row;
                    },
                    requestId,
                  );
                }
              }
              if (autoNoWrite) {
                externalContext =
                  `${externalContext}\nCLOUD_WRITE_TRUTH: The central Revit cloud model was NOT modified. Design Automation did not submit a workitem (disabled/stub or pre-confirm). Do NOT tell the user CONTROL_MARK was cleared in Revit or that syncing will show parameter changes. Quote PRECAST_DA_MARK_UPDATE message/note. Selection-based "clear marks" is not a live cloud write unless DA is enabled and the activity applies that change.`.trim();
              }
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
      }
    } catch (error) {
      log("warn", "local-agent-tool-plan-failed", {
        requestId,
        details: error instanceof Error ? error.message : "Unknown error",
      });
    }
  }

  const autoPollWorkitemId = resolveDaWorkitemIdForPoll(
    "",
    lastDaJobForResponse,
    activeDiscovery,
  );
  if (
    env.daEnabled === "true" &&
    shouldAutoPollDaFollowUp(message) &&
    autoPollWorkitemId &&
    !autoPollWorkitemId.startsWith("da-stub") &&
    !externalContext.includes("PRECAST_DA_POLL_RESULT") &&
    !daSubmittedThisTurn
  ) {
    try {
      const pollPayload = await pollDesignAutomationStatusContract({
        workitem_id: autoPollWorkitemId,
        cache_id: activeDiscovery?.cache_id,
        max_attempts: env.daPollMaxAttempts,
        delay_ms_between_attempts: env.daPollDelayMs,
      });
      clearDiscoveryPendingAfterTerminalPoll(
        activeDiscovery,
        pollPayload.workitem_id,
        pollPayload.status,
      );
      externalContext = appendDaPollToExternalContext(
        externalContext,
        pollPayload,
      );
    } catch (error) {
      externalContext =
        `${externalContext}\nPRECAST_DA_POLL_ERROR: ${JSON.stringify({
          message:
            error instanceof Error ? error.message : "Unknown error",
        })}`.trim();
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

  if (externalContext.includes("DISCOVERY_CACHED_SELECTION")) {
    externalContext =
      `${externalContext}\nFINALIZER_HINT_DISCOVERY: When summarizing selection, use after_filter_count / external_id_count / viewer_db_ids_queued from DISCOVERY_CACHED_SELECTION. If filtered_count or after_filter_count is 0, state clearly that nothing matched. Do not claim element types the user did not ask for. If DISCOVERY_SELECTION_APPLIED is true, avoid extra viewer.search actions that would fight selectDbIds.`.trim();
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
      actions = dedupeViewerActions([...toolViewerActions, ...aiIntent.actions]);
      modelQueryRequested =
        aiIntent.requestModelViews || modelViewsFromTool.length > 0;
      responseText = aiIntent.message;
    } else {
      const fallback = fallbackIntent(message);
      actions = dedupeViewerActions([...toolViewerActions, ...fallback.actions]);
      modelQueryRequested = fallback.modelQueryRequested;
      responseText = fallback.message;
    }
  } catch (error) {
    log("warn", "chat-ai-intent-fallback", {
      requestId,
      details: error instanceof Error ? error.message : "Unknown error",
    });
    const fallback = fallbackIntent(message);
    actions = dedupeViewerActions([...toolViewerActions, ...fallback.actions]);
    modelQueryRequested = fallback.modelQueryRequested;
    responseText = fallback.message;
  }

  const hasFallbackSelectionAction = actions.some((a) => {
    if (a.type === "viewer.searchAndSelectMetromontPieces") return true;
    if (a.type === "viewer.selectDbIds") return a.dbIds.length > 0;
    return false;
  });
  if (
    selectionToolRequestedThisTurn &&
    !selectionToolAppliedThisTurn &&
    !hasFallbackSelectionAction
  ) {
    responseText = selectionToolErrorThisTurn
      ? `I couldn't apply the requested selection. ${selectionToolErrorThisTurn}`
      : "I couldn't apply the requested selection in the viewer.";
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
    discoveryCachedSelection: activeDiscovery,
    lastDaJob: lastDaJobForResponse ?? null,
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
    discoveryCacheId: activeDiscovery?.cache_id ?? null,
    discoveryExternalIdCount: activeDiscovery?.externalIds.length ?? 0,
    lastDaWorkitemId: lastDaJobForResponse?.workitem_id ?? null,
    responseText: shouldLogFull() ? responseText : undefined,
    queryResult: shouldLogFull() ? queryResult : undefined,
  });
  return response;
}

