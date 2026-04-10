import { env } from "@/lib/env";

export type AiProvider = "openai" | "xai" | "cursor";
export type AssistantMode = "agent";

export type ViewerIntentAction =
  | { type: "viewer.fitToView" }
  | { type: "viewer.clearSelection" }
  | { type: "viewer.isolateSelection" }
  | { type: "viewer.search"; query: string }
  | { type: "viewer.isolateByQuery"; query: string }
  | { type: "viewer.hideByQuery"; query: string }
  | { type: "viewer.showAll" }
  | { type: "viewer.setGhosting"; enabled: boolean }
  | { type: "viewer.markupsSave" }
  | { type: "viewer.markupsLoad" }
  | { type: "viewer.markupsClear" }
  | {
      type: "viewer.selectDbIds";
      dbIds: number[];
      clearFirst?: boolean;
      fitToView?: boolean;
    }
  | { type: "viewer.isolateDbIds"; dbIds: number[] }
  | {
      type: "viewer.searchAndSelectMetromontPieces";
      query: string;
      pieceKind: "COLUMN" | "WPA" | "WPB" | "CLA" | "ANY_STRUCTURAL_FRAMING";
      clearFirst?: boolean;
      fitToView?: boolean;
      maxSearchMatches?: number;
    };

export type AiIntentResult = {
  message: string;
  actions: ViewerIntentAction[];
  requestModelViews: boolean;
};

export type AgentToolName =
  | "aec_query"
  | "selected_element_parameters"
  | "get_cached_selection"
  | "model_views"
  | "issues_list"
  | "issues_create"
  | "get_elements_by_category"
  | "select_elements"
  | "analyze_published_model_and_cache"
  | "get_cached_mark_analysis"
  | "trigger_design_automation_mark_update"
  | "analyze_products_and_mark"
  | "get_product_sameness_report"
  | "assign_control_marks";

export type AgentToolCall = {
  tool: AgentToolName;
  reason: string;
  args?: Record<string, unknown>;
};

type IntentOptions = {
  provider?: string;
  model?: string;
  assistantMode?: AssistantMode | string;
  selectedCount?: number;
  selectedDbIds?: number[];
  selectedElements?: Array<{
    dbId: number;
    name?: string;
    externalId?: string;
    properties: Array<{
      displayName: string;
      displayValue: string;
      units?: string;
    }>;
  }>;
  chatHistory?: string[];
  externalContext?: string;
};

type OpenAiResponse = {
  choices?: Array<{
    message?: {
      content?: string | Array<{ type?: string; text?: string }>;
    };
  }>;
};

type XaiResponsesPayload = {
  output?: Array<{
    type?: string;
    content?: Array<{ type?: string; text?: string }>;
  }>;
};

function extractXaiResponseText(json: XaiResponsesPayload): string {
  const parts: string[] = [];
  for (const item of json.output ?? []) {
    for (const block of item.content ?? []) {
      if (block.type === "output_text" && typeof block.text === "string") {
        parts.push(block.text);
      }
    }
  }
  return parts.join("\n");
}

const STRONG_TOOL_GUIDANCE = `
CRITICAL INSTRUCTIONS FOR SELECTION AND ANALYSIS:
- Prefer get_elements_by_category (AEC Data Model) then select_elements when hub/project/model context is valid.
- If AECDM is unavailable or errors (e.g. 404), use viewer action searchAndSelectMetromontPieces — NOT raw viewer.search. Example for columns: { "type": "viewer.searchAndSelectMetromontPieces", "query": "CLA", "pieceKind": "COLUMN", "clearFirst": true, "fitToView": false }. That runs Forge text search, then keeps only Structural Framing elements that match column/WPA/WPB/CLA rules.
- For wall panels use pieceKind WPA or WPB with a tight query when appropriate.
- Do not use viewer.search or viewer.isolateByQuery for precast piece selection — they match unrelated elements (any property containing the string).
- Default: clearFirst true, fitToView false unless the user asks to zoom/fit.
- In your reply to the user: be brief; do not paste this policy block or repeat long internal rules — report counts and what was selected.

PARAMETER EDITS VS MARK ANALYSIS (cloud / Design Automation):
- After selection is established, simple clears/sets/toggles on parameters do NOT require analyze_published_model_and_cache or mark grouping.
- For direct cloud writes without analysis: trigger_design_automation_mark_update with confirm: true, skip_analysis: true, and either (1) parameter_patches and/or parameter_updates rows with externalIds, OR (2) cached_selection: { externalIds } plus updates: [{ paramName, action: clear|set|toggle, value? }] — same selection can be reused across turns.
- ONLY use analyze_published_model_and_cache + get_cached_mark_analysis when the user asks to analyze, verify marks, propose groups, sameness, or similar.
- Mark application in Revit uses operation run_mark_analysis / apply_marks (with marks[]); pure edits use modify_parameters (default when skip_analysis is true) — do not conflate clearing a parameter with running mark verification unless the user asked for analysis.
- Before calling DA, state briefly what will run (e.g. "Clearing CONTROL_MARK on N elements — no mark analysis").
`.trim();

const METROMONT_SYSTEM_CONTEXT = [
  STRONG_TOOL_GUIDANCE,
  "",
  "You are Metromont's precast BIM co-pilot.",
  'When the user says "columns", always query Structural Framing category with family filter containing COLUMN or CLA (CONTROL_MARK prefix CLA); never treat precast columns as Revit "Structural Columns" category.',
  "For READ or ANALYZE (counts, verify marks, propose groups, sameness): use analyze_published_model_and_cache when you need published-model mark workflow data.",
  "For WRITE without mark grouping (clear/set/toggle any parameter on current selection): use trigger_design_automation_mark_update with confirm: true, skip_analysis: true, parameter_patches and/or parameter_updates and/or cached_selection+updates from get_cached_selection externalIds — do NOT run analyze_published_model_and_cache first unless the user asked for analysis.",
  "For WRITE that applies proposed CONTROL_MARK groups from cache: run analyze (if needed), preview, then trigger_design_automation_mark_update with confirm: true and cache_id (skip_analysis false or omit).",
  "Use get_cached_selection to confirm dbIds/externalIds before describing a cloud edit.",
  "Cloud writes: Revit central changes only when PRECAST_DA_MARK_UPDATE shows workitem_submitted: true. If stub or CLOUD_WRITE_TRUTH — say ACC was not modified.",
].join("\n");

const ALICE_AGENT_CHARTER = [
  "Identity: You are Alice.",
  "Response style: Use normal conversational responses; do not self-identify by name unless explicitly asked.",
  "Role: Autodesk data model and design engineering assistant for APS Viewer + Revit cloud models.",
  "Primary objective: Help users understand model content and execute safe viewer operations.",
  "Data policy: Prefer AEC Data Model context when available; use selected element properties as grounded fallback.",
  "Safety: Do not invent model facts; if uncertain, say what is missing and suggest the next best query.",
  "UX: Avoid echoing long internal tool instructions to the user; prefer short confirmations with concrete results (counts, names).",
  "Interaction style: Practical and collaborative. You may write as much detail as useful—headings, bullet lists, markdown, and step-by-step guidance are encouraged when they help the user work with the model.",
  "Domain vocabulary: Piece/Product/Panel refers to Structural Framing precast context; Piece ID can map to CONTROL_MARK.",
  "Revit cloud honesty: Never claim parameters changed in ACC or that the user should sync to see updates unless tool context shows a real DA submission (workitem_submitted true). If CLOUD_WRITE_TRUTH is present, follow it over guesses or cached previews.",
].join("\n");

const ALICE_SYSTEM_BASE = [METROMONT_SYSTEM_CONTEXT, "", ALICE_AGENT_CHARTER].join("\n");

function sanitizeActions(actions: unknown): ViewerIntentAction[] {
  if (!Array.isArray(actions)) return [];
  const clean: ViewerIntentAction[] = [];

  for (const action of actions) {
    if (!action || typeof action !== "object") continue;
    const candidate = action as { type?: string; query?: string };
    if (candidate.type === "viewer.fitToView") {
      clean.push({ type: "viewer.fitToView" });
    } else if (candidate.type === "viewer.clearSelection") {
      clean.push({ type: "viewer.clearSelection" });
    } else if (candidate.type === "viewer.isolateSelection") {
      clean.push({ type: "viewer.isolateSelection" });
    } else if (
      candidate.type === "viewer.search" &&
      typeof candidate.query === "string" &&
      candidate.query.trim().length > 0
    ) {
      clean.push({ type: "viewer.search", query: candidate.query.trim() });
    } else if (
      candidate.type === "viewer.isolateByQuery" &&
      typeof candidate.query === "string" &&
      candidate.query.trim().length > 0
    ) {
      clean.push({ type: "viewer.isolateByQuery", query: candidate.query.trim() });
    } else if (
      candidate.type === "viewer.hideByQuery" &&
      typeof candidate.query === "string" &&
      candidate.query.trim().length > 0
    ) {
      clean.push({ type: "viewer.hideByQuery", query: candidate.query.trim() });
    } else if (candidate.type === "viewer.showAll") {
      clean.push({ type: "viewer.showAll" });
    } else if (
      candidate.type === "viewer.setGhosting" &&
      typeof (candidate as { enabled?: unknown }).enabled === "boolean"
    ) {
      clean.push({
        type: "viewer.setGhosting",
        enabled: Boolean((candidate as { enabled?: boolean }).enabled),
      });
    } else if (candidate.type === "viewer.markupsSave") {
      clean.push({ type: "viewer.markupsSave" });
    } else if (candidate.type === "viewer.markupsLoad") {
      clean.push({ type: "viewer.markupsLoad" });
    } else if (candidate.type === "viewer.markupsClear") {
      clean.push({ type: "viewer.markupsClear" });
    } else if (candidate.type === "viewer.selectDbIds") {
      const raw = (candidate as { dbIds?: unknown }).dbIds;
      const dbIds = Array.isArray(raw)
        ? raw
            .map((x) => (typeof x === "number" ? x : Number(x)))
            .filter((n) => Number.isFinite(n))
            .map((n) => Math.trunc(n))
        : [];
      if (dbIds.length > 0) {
        const c = candidate as {
          clearFirst?: unknown;
          fitToView?: unknown;
        };
        clean.push({
          type: "viewer.selectDbIds",
          dbIds: dbIds.slice(0, 500),
          clearFirst:
            c.clearFirst === undefined ? true : Boolean(c.clearFirst),
          fitToView: Boolean(c.fitToView),
        });
      }
    } else if (candidate.type === "viewer.isolateDbIds") {
      const raw = (candidate as { dbIds?: unknown }).dbIds;
      const dbIds = Array.isArray(raw)
        ? raw
            .map((x) => (typeof x === "number" ? x : Number(x)))
            .filter((n) => Number.isFinite(n))
            .map((n) => Math.trunc(n))
        : [];
      if (dbIds.length > 0) {
        clean.push({
          type: "viewer.isolateDbIds",
          dbIds: dbIds.slice(0, 500),
        });
      }
    } else if (candidate.type === "viewer.searchAndSelectMetromontPieces") {
      const c = candidate as {
        query?: unknown;
        pieceKind?: unknown;
        clearFirst?: unknown;
        fitToView?: unknown;
        maxSearchMatches?: unknown;
      };
      const query = typeof c.query === "string" ? c.query.trim() : "";
      const kinds = [
        "COLUMN",
        "WPA",
        "WPB",
        "CLA",
        "ANY_STRUCTURAL_FRAMING",
      ] as const;
      const pk = kinds.includes(c.pieceKind as (typeof kinds)[number])
        ? (c.pieceKind as (typeof kinds)[number])
        : "COLUMN";
      const maxRaw = c.maxSearchMatches;
      const maxSearchMatches =
        typeof maxRaw === "number" && Number.isFinite(maxRaw)
          ? Math.min(Math.max(Math.trunc(maxRaw), 1), 8000)
          : undefined;
      if (query.length > 0) {
        clean.push({
          type: "viewer.searchAndSelectMetromontPieces",
          query,
          pieceKind: pk,
          clearFirst: c.clearFirst !== false,
          fitToView: Boolean(c.fitToView),
          ...(maxSearchMatches != null ? { maxSearchMatches } : {}),
        });
      }
    }
  }

  return clean.slice(0, 8);
}

function extractJsonObject(raw: string): string | null {
  const fencedMatch = raw.match(/```(?:json)?\s*([\s\S]*?)```/i);
  const source = fencedMatch?.[1] ?? raw;
  const firstBrace = source.indexOf("{");
  const lastBrace = source.lastIndexOf("}");
  if (firstBrace < 0 || lastBrace < 0 || lastBrace <= firstBrace) return null;
  return source.slice(firstBrace, lastBrace + 1);
}

/** Last fenced ``` / ```json block (models often put machine JSON at the end). */
function findLastFencedCodeBlock(raw: string): { before: string; inner: string } | null {
  const trimmed = raw.trim();
  const re = /```(?:json)?\s*([\s\S]*?)```/gi;
  let match: RegExpExecArray | null;
  let last: RegExpExecArray | null = null;
  let lastIndex = 0;
  while ((match = re.exec(trimmed)) !== null) {
    last = match;
    lastIndex = match.index;
  }
  if (!last) return null;
  return {
    before: trimmed.slice(0, lastIndex).trim(),
    inner: last[1].trim(),
  };
}

function parseIntentPayload(jsonText: string): {
  message: string;
  actions: ViewerIntentAction[];
  requestModelViews: boolean;
} {
  const parsed = JSON.parse(jsonText) as {
    message?: unknown;
    actions?: unknown;
    requestModelViews?: unknown;
  };
  const fromJson =
    typeof parsed.message === "string" && parsed.message.trim()
      ? parsed.message.trim()
      : "";
  return {
    message: fromJson,
    actions: sanitizeActions(parsed.actions),
    requestModelViews: Boolean(parsed.requestModelViews),
  };
}

function parseIntentResponse(raw: string): AiIntentResult {
  const fenced = findLastFencedCodeBlock(raw);
  if (fenced) {
    const jsonText =
      extractJsonObject(fenced.inner) ??
      (fenced.inner.startsWith("{") ? fenced.inner : null);
    if (jsonText) {
      try {
        const payload = parseIntentPayload(jsonText);
        const message =
          fenced.before ||
          payload.message ||
          "Understood. I processed your request.";
        return {
          message,
          actions: payload.actions,
          requestModelViews: payload.requestModelViews,
        };
      } catch {
        return {
          message: fenced.before || raw.trim() || "Understood.",
          actions: [],
          requestModelViews: false,
        };
      }
    }
    return {
      message: fenced.before || raw.trim() || "Understood.",
      actions: [],
      requestModelViews: false,
    };
  }

  const t = raw.trim();
  if (t.startsWith("{")) {
    try {
      const payload = parseIntentPayload(t);
      return {
        message:
          payload.message || "Understood. I processed your request.",
        actions: payload.actions,
        requestModelViews: payload.requestModelViews,
      };
    } catch {
      /* fall through */
    }
  }

  return {
    message: t || "Understood.",
    actions: [],
    requestModelViews: false,
  };
}

function parsePlannerResponse(raw: string): {
  plan: string[];
  actions: ViewerIntentAction[];
  requestModelViews: boolean;
  messageDraft: string;
} {
  const fenced = findLastFencedCodeBlock(raw);
  if (fenced) {
    const jsonText =
      extractJsonObject(fenced.inner) ??
      (fenced.inner.startsWith("{") ? fenced.inner : null);
    if (jsonText) {
      try {
        const parsed = JSON.parse(jsonText) as {
          plan?: unknown;
          actions?: unknown;
          requestModelViews?: unknown;
          messageDraft?: unknown;
        };
        const plan = Array.isArray(parsed.plan)
          ? parsed.plan
              .map((s) => String(s))
              .filter((s) => s.trim().length > 0)
              .slice(0, 6)
          : [];
        const messageDraftFromJson =
          typeof parsed.messageDraft === "string"
            ? parsed.messageDraft.trim()
            : "";
        return {
          plan,
          actions: sanitizeActions(parsed.actions),
          requestModelViews: Boolean(parsed.requestModelViews),
          messageDraft: fenced.before || messageDraftFromJson,
        };
      } catch {
        return {
          plan: [],
          actions: [],
          requestModelViews: false,
          messageDraft: fenced.before || raw.trim(),
        };
      }
    }
    return {
      plan: [],
      actions: [],
      requestModelViews: false,
      messageDraft: fenced.before || raw.trim(),
    };
  }

  const t = raw.trim();
  if (t.startsWith("{")) {
    try {
      const parsed = JSON.parse(t) as {
        plan?: unknown;
        actions?: unknown;
        requestModelViews?: unknown;
        messageDraft?: unknown;
      };
      return {
        plan: Array.isArray(parsed.plan)
          ? parsed.plan
              .map((s) => String(s))
              .filter((s) => s.trim().length > 0)
              .slice(0, 6)
          : [],
        actions: sanitizeActions(parsed.actions),
        requestModelViews: Boolean(parsed.requestModelViews),
        messageDraft:
          typeof parsed.messageDraft === "string"
            ? parsed.messageDraft.trim()
            : "",
      };
    } catch {
      /* fall through */
    }
  }

  return {
    plan: [],
    actions: [],
    requestModelViews: false,
    messageDraft: t,
  };
}

function parseToolPlannerResponse(raw: string): AgentToolCall[] {
  const fenced = findLastFencedCodeBlock(raw);
  const slice = fenced ? fenced.inner : raw;
  const jsonText =
    extractJsonObject(slice.trim()) ??
    (slice.trim().startsWith("{") ? slice.trim() : null);
  if (!jsonText) return [];
  let parsed: {
    toolCalls?: Array<{ tool?: unknown; reason?: unknown; args?: unknown }>;
  };
  try {
    parsed = JSON.parse(jsonText) as {
      toolCalls?: Array<{ tool?: unknown; reason?: unknown; args?: unknown }>;
    };
  } catch {
    return [];
  }
  if (!Array.isArray(parsed.toolCalls)) return [];
  const allowed: AgentToolName[] = [
    "aec_query",
    "selected_element_parameters",
    "get_cached_selection",
    "model_views",
    "issues_list",
    "issues_create",
    "get_elements_by_category",
    "select_elements",
    "analyze_published_model_and_cache",
    "get_cached_mark_analysis",
    "trigger_design_automation_mark_update",
    "analyze_products_and_mark",
    "get_product_sameness_report",
    "assign_control_marks",
  ];
  const out: AgentToolCall[] = [];
  for (const item of parsed.toolCalls) {
    const tool = String(item.tool ?? "") as AgentToolName;
    if (!allowed.includes(tool)) continue;
    const reason = String(item.reason ?? "").trim() || "No reason provided.";
    const rawArgs = item.args;
    const args =
      rawArgs && typeof rawArgs === "object" && !Array.isArray(rawArgs)
        ? (rawArgs as Record<string, unknown>)
        : undefined;
    out.push({ tool, reason, args });
    if (out.length >= 6) break;
  }
  return out;
}

function buildPlannerPrompt(
  userMessage: string,
  selectedModelName: string,
  options: IntentOptions,
): string {
  const history = Array.isArray(options.chatHistory)
    ? options.chatHistory.slice(-8)
    : [];
  const selectedElements = Array.isArray(options.selectedElements)
    ? options.selectedElements.slice(0, 3).map((el) => ({
        dbId: el.dbId,
        name: el.name,
        externalId: el.externalId,
        properties: (el.properties ?? []).slice(0, 80),
      }))
    : [];
  const context = {
    selectedCount: options.selectedCount ?? 0,
    selectedDbIds: (options.selectedDbIds ?? []).slice(0, 20),
    selectedElements,
    history,
    externalContext: options.externalContext ?? "",
  };

  return [
    ALICE_SYSTEM_BASE,
    "",
    "You are an agentic planner for an APS Viewer assistant.",
    "Think in natural language: outline your approach, note BIM/viewer considerations, and draft how Alice should answer the user.",
    "When your reasoning is complete, end with a single ```json code block containing ONLY:",
    '{ "plan": string[], "actions": Array<ViewerAction>, "requestModelViews": boolean, "messageDraft": string }',
    "The messageDraft is an optional short note; the final user-facing reply will be written in a later step.",
    "",
    "Allowed ViewerAction values:",
    '- { "type": "viewer.fitToView" }',
    '- { "type": "viewer.clearSelection" }',
    '- { "type": "viewer.isolateSelection" }',
    '- { "type": "viewer.search", "query": string }',
    '- { "type": "viewer.isolateByQuery", "query": string }',
    '- { "type": "viewer.hideByQuery", "query": string }',
    '- { "type": "viewer.showAll" }',
    '- { "type": "viewer.setGhosting", "enabled": boolean }',
    '- { "type": "viewer.markupsSave" }',
    '- { "type": "viewer.markupsLoad" }',
    '- { "type": "viewer.markupsClear" }',
    '- { "type": "viewer.selectDbIds", "dbIds": number[], "clearFirst"?: boolean, "fitToView"?: boolean }',
    '- { "type": "viewer.isolateDbIds", "dbIds": number[] }',
    '- { "type": "viewer.searchAndSelectMetromontPieces", "query": string, "pieceKind": "COLUMN"|"WPA"|"WPB"|"CLA"|"ANY_STRUCTURAL_FRAMING", "clearFirst"?: boolean, "fitToView"?: boolean, "maxSearchMatches"?: number }',
    "",
    "Rules:",
    "- Prefer viewer.selectDbIds over viewer.search when exact dbIds are known (e.g. from get_elements_by_category). Default clearFirst true, fitToView false unless user asks to zoom/fit.",
    "- For precast columns/panels when AECDM dbIds are unavailable, use viewer.searchAndSelectMetromontPieces (not viewer.search).",
    "- Keep selection stable unless user explicitly asks to change it.",
    "- For pure question answering, prefer actions=[] and answer from provided context.",
    "- If PRODUCT_ANALYSIS_CONTEXT is provided, use it for product-level reasoning and code/standards-oriented guidance.",
    "- Use user vernacular: Piece/Product/Panel usually means Structural Framing precast.",
    "- Piece ID can map to CONTROL_MARK.",
    "- Plan and prose may be as detailed as needed; the JSON block is only for structured fields.",
    "",
    `Selected model: ${selectedModelName}`,
    `Context JSON: ${JSON.stringify(context)}`,
    `User message: ${userMessage}`,
  ].join("\n");
}

function extractRevitWriteFactsForFinalizer(externalContext: string): string {
  if (!externalContext) return "";
  const markers = [
    "CLOUD_WRITE_TRUTH:",
    "PRECAST_DA_MARK_UPDATE:",
    "PRECAST_DA_MARK_UPDATE_ERROR:",
  ] as const;
  const chunks: string[] = [];
  for (const m of markers) {
    const idx = externalContext.indexOf(m);
    if (idx >= 0) {
      chunks.push(externalContext.slice(idx, idx + 2500).trim());
    }
  }
  return chunks.join("\n\n");
}

function buildFinalizerPrompt(
  userMessage: string,
  selectedModelName: string,
  planner: {
    plan: string[];
    actions: ViewerIntentAction[];
    requestModelViews: boolean;
    messageDraft: string;
  },
  revitWriteFacts?: string,
): string {
  return [
    ALICE_SYSTEM_BASE,
    "",
    "You are the final responder for an APS Viewer assistant.",
    "Write the full reply the user will see: conversational, clear, and as long or short as appropriate (markdown is fine).",
    "Incorporate the planner's intent and messageDraft; you may expand, clarify, or reorganize freely.",
    revitWriteFacts
      ? [
          "",
          "Authoritative facts from tools (must match user-visible claims; override messageDraft if it conflicts):",
          revitWriteFacts,
          "",
        ].join("\n")
      : "",
    "If viewer actions or a model-views request should still apply, end with one ```json code block containing ONLY:",
    '{ "actions": Array<ViewerAction>, "requestModelViews": boolean }',
    "Use actions: [] when no viewer automation is needed. Do not repeat your whole essay inside the JSON.",
    "",
    "Allowed ViewerAction values:",
    '- { "type": "viewer.fitToView" }',
    '- { "type": "viewer.clearSelection" }',
    '- { "type": "viewer.isolateSelection" }',
    '- { "type": "viewer.search", "query": string }',
    '- { "type": "viewer.isolateByQuery", "query": string }',
    '- { "type": "viewer.hideByQuery", "query": string }',
    '- { "type": "viewer.showAll" }',
    '- { "type": "viewer.setGhosting", "enabled": boolean }',
    '- { "type": "viewer.markupsSave" }',
    '- { "type": "viewer.markupsLoad" }',
    '- { "type": "viewer.markupsClear" }',
    '- { "type": "viewer.selectDbIds", "dbIds": number[], "clearFirst"?: boolean, "fitToView"?: boolean }',
    '- { "type": "viewer.isolateDbIds", "dbIds": number[] }',
    '- { "type": "viewer.searchAndSelectMetromontPieces", "query": string, "pieceKind": "COLUMN"|"WPA"|"WPB"|"CLA"|"ANY_STRUCTURAL_FRAMING", "clearFirst"?: boolean, "fitToView"?: boolean, "maxSearchMatches"?: number }',
    "",
    `Selected model: ${selectedModelName}`,
    `User message: ${userMessage}`,
    `Planner JSON: ${JSON.stringify(planner)}`,
  ].join("\n");
}

function buildToolPlannerPrompt(
  userMessage: string,
  selectedModelName: string,
  options: IntentOptions,
): string {
  const history = Array.isArray(options.chatHistory)
    ? options.chatHistory.slice(-6)
    : [];
  const selectedCount = options.selectedCount ?? 0;
  const hasHubProjectHint =
    typeof options.externalContext === "string" &&
    options.externalContext.includes("HUB_PROJECT_AVAILABLE");

  return [
    ALICE_SYSTEM_BASE,
    "",
    "You are a local tool planner for an APS Viewer AI assistant.",
    "Briefly note why tools may or may not be needed (plain text is fine).",
    "Then end with a single ```json code block containing ONLY:",
    '{ "toolCalls": Array<{ "tool": "aec_query" | "selected_element_parameters" | "get_cached_selection" | "model_views" | "issues_list" | "issues_create" | "get_elements_by_category" | "select_elements" | "analyze_published_model_and_cache" | "get_cached_mark_analysis" | "trigger_design_automation_mark_update" | "analyze_products_and_mark" | "get_product_sameness_report" | "assign_control_marks", "reason": string, "args"?: object }> }',
    "",
    "Tool selection guidance:",
    '- Use "get_elements_by_category" first when the user wants to select/find/highlight elements by category/family/type; args: category?, family?, type?, limit?, product_prefix? (WPA|WPB|CLA|COLUMN|ALL). Requires hub/project/model context — web injects token and URNs.',
    '- Use "select_elements" with dbIds from AECDM results; args: { "dbIds": (string|number)[], "clearFirst"?: boolean (default true), "zoomToSelection"?: boolean (default false) }.',
    '- Use "aec_query" for model-wide questions, counts, categories, or when semantic model data is required.',
    '- Use "selected_element_parameters" when the user asks about currently selected elements.',
    '- Use "get_cached_selection" to list dbIds, externalIds, and names for the current viewer selection before cloud parameter edits (no args).',
    '- Use "model_views" only when asked for model views/metadata/sheets listing.',
    '- Use "issues_list" when the user asks to list/show/open project issues.',
    '- Use "issues_create" when the user asks to create a new issue.',
    '- Use "analyze_published_model_and_cache" ONLY when the user wants mark analysis, grouping, verification, or sameness preview — not for simple parameter clears/sets.',
    '- Use "get_cached_mark_analysis" to show latest cached marks/sameness preview (no args).',
    '- Use "trigger_design_automation_mark_update" after user confirms: (A) Direct edits — args: { "confirm": true, "skip_analysis": true, "parameter_patches" / "parameter_updates" and/or "cached_selection": { externalIds }, "updates": [{ paramName, action }], "cache_id" optional }. (B) Cached marks / mark path — args: { "cache_id", "confirm": true } without skip_analysis; operation run_mark_analysis only when applying proposed marks, not for simple param clears.',
    '- Read PRECAST_DA_MARK_UPDATE in context: if workitem_submitted is false or status is "stub", no cloud Revit write occurred — never imply marks were cleared or sync will show DA changes.',
    '- Use "analyze_products_and_mark" for granular legacy mark analysis (args: { "product_prefix", "dry_run" }).',
    '- Use "get_product_sameness_report" when comparing specific element IDs (args: { "element_ids": string[] }).',
    '- Use "assign_control_marks" only after verified groups (args: { "mark_groups": object[], "start_number"?: number }).',
    "",
    "Rules:",
    "- Keep toolCalls length 0..6.",
    "- If no tool is needed, return empty array.",
    "",
    `Selected model: ${selectedModelName}`,
    `Selected count: ${selectedCount}`,
    `Has hub/project context: ${hasHubProjectHint ? "yes" : "no"}`,
    `Recent history: ${JSON.stringify(history)}`,
    `User message: ${userMessage}`,
  ].join("\n");
}

async function callXaiResponsesRaw(
  prompt: string,
  modelOverride?: string,
): Promise<string> {
  if (!env.aiXaiKey) throw new Error("XAI_API_KEY is not configured.");

  const response = await fetch("https://api.x.ai/v1/responses", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${env.aiXaiKey}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      model: modelOverride?.trim() || env.aiXaiModel,
      temperature: 0.25,
      store: false,
      max_output_tokens: 8192,
      instructions: [
        METROMONT_SYSTEM_CONTEXT,
        "",
        "You are Alice, assisting with Autodesk APS Viewer and BIM models. Follow the user prompt: reply helpfully and conversationally when asked, and include structured JSON in a fenced ```json block only when the prompt specifies it.",
      ].join("\n"),
      input: prompt,
    }),
    cache: "no-store",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`xAI request failed (${response.status}): ${text}`);
  }

  const json = (await response.json()) as XaiResponsesPayload;
  return extractXaiResponseText(json);
}

async function callOpenAiRaw(
  prompt: string,
  modelOverride?: string,
): Promise<string> {
  if (!env.aiOpenAiKey) throw new Error("OPENAI_API_KEY is not configured.");

  const response = await fetch("https://api.openai.com/v1/chat/completions", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${env.aiOpenAiKey}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      model: modelOverride?.trim() || env.aiOpenAiModel,
      temperature: 0.25,
      messages: [
        {
          role: "system",
          content: [
            METROMONT_SYSTEM_CONTEXT,
            "",
            "You are Alice's reasoning layer for APS Viewer / BIM. Be conversational when the user prompt asks for it; output fenced ```json blocks when the prompt requires structured data.",
          ].join("\n"),
        },
        { role: "user", content: prompt },
      ],
    }),
    cache: "no-store",
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(`OpenAI request failed (${response.status}): ${text}`);
  }

  const json = (await response.json()) as OpenAiResponse;
  const content = json.choices?.[0]?.message?.content;
  return typeof content === "string"
    ? content
    : Array.isArray(content)
      ? content.map((c) => c.text ?? "").join("\n")
      : "";
}

type LlmBackend = Exclude<AiProvider, "cursor">;

async function callLlmRaw(
  backend: LlmBackend,
  prompt: string,
  modelOverride?: string,
): Promise<string> {
  if (backend === "xai") {
    return callXaiResponsesRaw(prompt, modelOverride);
  }
  return callOpenAiRaw(prompt, modelOverride);
}

async function runAgenticLoop(
  backend: LlmBackend,
  model: string | undefined,
  userMessage: string,
  selectedModelName: string,
  options: IntentOptions,
): Promise<AiIntentResult> {
  const plannerPrompt = buildPlannerPrompt(userMessage, selectedModelName, options);
  const plannerRaw = await callLlmRaw(backend, plannerPrompt, model);
  const planner = parsePlannerResponse(plannerRaw);

  const revitWriteFacts = extractRevitWriteFactsForFinalizer(
    options.externalContext ?? "",
  );
  const finalizerPrompt = buildFinalizerPrompt(
    userMessage,
    selectedModelName,
    planner,
    revitWriteFacts || undefined,
  );
  const finalRaw = await callLlmRaw(backend, finalizerPrompt, model);
  return parseIntentResponse(finalRaw);
}

function normalizeAssistantMode(): AssistantMode {
  return "agent";
}

function normalizeProvider(input?: string): AiProvider {
  const value = (input ?? "").trim().toLowerCase();
  if (value === "anthropic") {
    throw new Error(
      "Anthropic is no longer supported. Set AI_PROVIDER to xai or openai and configure the matching API key.",
    );
  }
  if (value === "cursor") return "cursor";
  if (value === "xai") return "xai";
  return "openai";
}

export async function resolveViewerIntent(
  userMessage: string,
  selectedModelName: string,
  options: IntentOptions = {},
): Promise<AiIntentResult> {
  const provider = normalizeProvider(options.provider || env.aiProvider);
  const model = options.model?.trim();

  if (provider === "cursor") {
    throw new Error(
      "AI_PROVIDER=cursor is not wired yet in this app. Use xai or openai.",
    );
  }

  normalizeAssistantMode();
  return runAgenticLoop(provider, model, userMessage, selectedModelName, options);
}

export async function planLocalAgentTools(
  userMessage: string,
  selectedModelName: string,
  options: IntentOptions = {},
): Promise<AgentToolCall[]> {
  const provider = normalizeProvider(options.provider || env.aiProvider);
  const model = options.model?.trim();
  if (provider === "cursor") {
    throw new Error(
      "AI_PROVIDER=cursor is not wired yet in this app. Use xai or openai.",
    );
  }
  const prompt = buildToolPlannerPrompt(userMessage, selectedModelName, options);
  const raw = await callLlmRaw(provider, prompt, model);
  return parseToolPlannerResponse(raw);
}
