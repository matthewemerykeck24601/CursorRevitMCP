import "dotenv/config";
import { serve } from "@hono/node-server";
import { Hono } from "hono";
import { cors } from "hono/cors";
import { randomUUID } from "node:crypto";
import { callGrokResponses } from "./xai.js";

const PORT = Number(process.env.PORT || "8787");
const XAI_API_KEY = (process.env.XAI_API_KEY || "").trim();
const XAI_MODEL = (process.env.XAI_MODEL || "grok-3-latest").trim();
const REQUIRE_BEARER =
  (process.env.MONTY_REQUIRE_BEARER || "1").trim() === "1" ||
  (process.env.MONTY_REQUIRE_BEARER || "").toLowerCase() === "true";
const APS_USER_PROFILE_URL = "https://developer.api.autodesk.com/userprofile/v1/users/@me";

function bearerToken(authHeader: string | undefined): string | null {
  if (!authHeader) return null;
  const m = /^Bearer\s+(.+)$/i.exec(authHeader.trim());
  return m?.[1]?.trim() || null;
}

async function isValidApsBearerToken(token: string): Promise<boolean> {
  try {
    const response = await fetch(APS_USER_PROFILE_URL, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    });
    return response.ok;
  } catch {
    return false;
  }
}

async function requireValidBearer(authHeader: string | undefined): Promise<boolean> {
  if (!REQUIRE_BEARER) return true;
  const token = bearerToken(authHeader);
  if (!token) return false;
  return isValidApsBearerToken(token);
}

type ChatBody = {
  message?: string;
  workspaceMode?: string;
  chatHistory?: string[];
  selectedHubId?: string;
  aiProvider?: string;
  aiModel?: string;
};

function buildChatPrompt(body: ChatBody): string {
  const chunks: string[] = [];
  if (Array.isArray(body.chatHistory) && body.chatHistory.length > 0) {
    chunks.push("Conversation so far:\n" + body.chatHistory.join("\n"));
  }
  if (body.selectedHubId && String(body.selectedHubId).trim()) {
    chunks.push(
      `Context: selected Autodesk hub id = ${String(body.selectedHubId).trim()}`,
    );
  }
  if (body.workspaceMode === "admin") {
    chunks.push("(Workspace: admin — user may ask about ACC projects, users, or general help.)");
  }
  const msg = (body.message ?? "").trim();
  chunks.push(`User message:\n${msg || "(empty)"}`);
  return chunks.join("\n\n");
}

const app = new Hono();

app.use(
  "/*",
  cors({
    origin: "*",
    allowMethods: ["GET", "POST", "OPTIONS"],
    allowHeaders: ["Authorization", "Content-Type"],
  }),
);

app.get("/health", (c) =>
  c.json({
    ok: true,
    service: "monty-ai-server",
    hasXaiKey: Boolean(XAI_API_KEY),
  }),
);

/** Matches aps-ai-web `GET /api/auth/session` enough for Monty `APSAPIClient.fetchSessionStatus`. */
app.get("/api/auth/session", async (c) => {
  if (!(await requireValidBearer(c.req.header("Authorization")))) {
    return c.json({ error: "Not authenticated" }, 401);
  }
  const requestId = randomUUID();
  return c.json({
    requestId,
    authenticated: true,
    expiresAt: Number.MAX_SAFE_INTEGER,
    scope: "",
  });
});

app.post("/api/chat", async (c) => {
  const requestId = randomUUID();
  if (!(await requireValidBearer(c.req.header("Authorization")))) {
    return c.json({ error: "Not authenticated", requestId }, 401);
  }

  if (!XAI_API_KEY) {
    return c.json(
      {
        error: "Server misconfiguration: XAI_API_KEY is not set",
        requestId,
      },
      500,
    );
  }

  let body: ChatBody;
  try {
    body = (await c.req.json()) as ChatBody;
  } catch {
    return c.json({ error: "Invalid JSON", requestId }, 400);
  }

  const message = (body.message ?? "").trim();
  if (!message) {
    return c.json({ error: "message is required", requestId }, 400);
  }

  const modelOverride = (body.aiModel ?? "").trim();
  const model = modelOverride || XAI_MODEL;

  const prompt = buildChatPrompt(body);

  try {
    const reply = await callGrokResponses({
      apiKey: XAI_API_KEY,
      model,
      userPrompt: prompt,
    });
    return c.json({
      message: reply || "(no text in model response)",
      requestId,
    });
  } catch (e) {
    const details = e instanceof Error ? e.message : String(e);
    return c.json(
      {
        error: "Upstream model error",
        details,
        requestId,
      },
      502,
    );
  }
});

serve({ fetch: app.fetch, port: PORT }, (info) => {
  console.log(
    `[monty-ai-server] http://127.0.0.1:${info.port}  (Grok/xAI)  requireBearer=${REQUIRE_BEARER}`,
  );
  if (!XAI_API_KEY) {
    console.warn("[monty-ai-server] WARNING: XAI_API_KEY is empty — /api/chat will 500");
  }
});
