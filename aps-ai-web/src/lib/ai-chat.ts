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
  | { type: "viewer.markupsClear" };

export type AiIntentResult = {
  message: string;
  actions: ViewerIntentAction[];
  requestModelViews: boolean;
};

export type AgentToolName =
  | "aec_query"
  | "selected_element_parameters"
  | "model_views"
  | "issues_list"
  | "issues_create"
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

const METROMONT_SYSTEM_CONTEXT = `
You are Metromont's precast BIM co-pilot. You have full access to the precast-revit-ontology.mdc above.
When the user asks to "Analyze the WPA's", "run mark verification", or "mark the pieces":
1. Call analyze_products_and_mark with appropriate prefix
2. Use get_product_sameness_report if needed
3. Propose CONTROL_MARK assignments starting at 100
4. Always confirm with tolerances and intersecting-element logic
Never place do_not_use_ families standalone. Always respect nested rules.
`.trim();

const ALICE_AGENT_CHARTER = [
  "Identity: You are Alice.",
  "Response style: Use normal conversational responses; do not self-identify by name unless explicitly asked.",
  "Role: Autodesk data model and design engineering assistant for APS Viewer + Revit cloud models.",
  "Primary objective: Help users understand model content and execute safe viewer operations.",
  "Data policy: Prefer AEC Data Model context when available; use selected element properties as grounded fallback.",
  "Safety: Do not invent model facts; if uncertain, say what is missing and suggest the next best query.",
  "Interaction style: Practical and collaborative. You may write as much detail as useful—headings, bullet lists, markdown, and step-by-step guidance are encouraged when they help the user work with the model.",
  "Domain vocabulary: Piece/Product/Panel refers to Structural Framing precast context; Piece ID can map to CONTROL_MARK.",
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
    }
  }

  return clean.slice(0, 4);
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
    "model_views",
    "issues_list",
    "issues_create",
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
    if (out.length >= 3) break;
  }
  return out;
}

function buildPrompt(
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
    "You are a conversational assistant for an APS Viewer.",
    "You may answer in full natural language (markdown welcome). When you also want the app to run viewer actions, end your reply with a single fenced JSON block (see below).",
    "",
    "Optional machine block — after your conversational text, add one ```json code block containing ONLY:",
    '{ "message": string (short echo optional), "actions": Array<ViewerAction>, "requestModelViews": boolean }',
    "If no viewer actions are needed, you may omit the block entirely or use actions: [].",
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
    "",
    "Rules:",
    "- Never return actions outside the allowlist.",
    "- Keep actions to at most 4 items.",
    "- Do not emit viewer actions for pure Q&A unless user clearly requests a viewer operation.",
    "- Keep existing selection stable unless user explicitly asks to change it.",
    "- Set requestModelViews=true only when user asks for model views/metadata.",
    "- If selected element properties are provided, use them to answer parameter questions directly.",
    "- If PRODUCT_ANALYSIS_CONTEXT is provided, use it for product-level reasoning and code/standards-oriented guidance.",
    "- User vernacular: Piece/Product/Panel usually refers to Structural Framing precast elements.",
    "- Piece ID is an alias of CONTROL_MARK.",
    "- Prefer targeted answers (e.g., only width/height asked) instead of dumping all properties unless user asks for all.",
    "- Be as brief or as detailed as the question warrants; do not artificially shorten helpful explanations.",
    "",
    `Selected model: ${selectedModelName}`,
    `Context JSON: ${JSON.stringify(context)}`,
    `User message: ${userMessage}`,
  ].join("\n");
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
    "",
    "Rules:",
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

function buildFinalizerPrompt(
  userMessage: string,
  selectedModelName: string,
  planner: {
    plan: string[];
    actions: ViewerIntentAction[];
    requestModelViews: boolean;
    messageDraft: string;
  },
): string {
  return [
    ALICE_SYSTEM_BASE,
    "",
    "You are the final responder for an APS Viewer assistant.",
    "Write the full reply the user will see: conversational, clear, and as long or short as appropriate (markdown is fine).",
    "Incorporate the planner's intent and messageDraft; you may expand, clarify, or reorganize freely.",
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
    '{ "toolCalls": Array<{ "tool": "aec_query" | "selected_element_parameters" | "model_views" | "issues_list" | "issues_create" | "analyze_products_and_mark" | "get_product_sameness_report" | "assign_control_marks", "reason": string, "args"?: object }> }',
    "",
    "Tool selection guidance:",
    '- Use "aec_query" for model-wide questions, counts, categories, or when semantic model data is required.',
    '- Use "selected_element_parameters" when the user asks about currently selected elements.',
    '- Use "model_views" only when asked for model views/metadata/sheets listing.',
    '- Use "issues_list" when the user asks to list/show/open project issues.',
    '- Use "issues_create" when the user asks to create a new issue.',
    '- Use "analyze_products_and_mark" for WPA/WPB/COLUMN mark verification, "mark the pieces", or "Analyze the WPA\'s" (include args: { "product_prefix": "WPA"|"WPB"|"CLA"|"ALL", "dry_run": boolean }).',
    '- Use "get_product_sameness_report" when comparing specific element IDs for sameness (args: { "element_ids": string[] }).',
    '- Use "assign_control_marks" only after verified groups (args: { "mark_groups": object[], "start_number"?: number }).',
    "",
    "Rules:",
    "- Keep toolCalls length 0..3.",
    "- If no tool is needed, return empty array.",
    "",
    `Selected model: ${selectedModelName}`,
    `Selected count: ${selectedCount}`,
    `Has hub/project context: ${hasHubProjectHint ? "yes" : "no"}`,
    `Recent history: ${JSON.stringify(history)}`,
    `User message: ${userMessage}`,
  ].join("\n");
}

async function callOpenAi(prompt: string, modelOverride?: string): Promise<AiIntentResult> {
  const raw = await callOpenAiRaw(prompt, modelOverride);
  return parseIntentResponse(raw);
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

  const finalizerPrompt = buildFinalizerPrompt(
    userMessage,
    selectedModelName,
    planner,
  );
  const finalRaw = await callLlmRaw(backend, finalizerPrompt, model);
  return parseIntentResponse(finalRaw);
}

function normalizeAssistantMode(input?: string): AssistantMode {
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

  normalizeAssistantMode(options.assistantMode);
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
