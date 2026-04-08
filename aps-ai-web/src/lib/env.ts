import fs from "node:fs";
import path from "node:path";

type EnvMap = Record<string, string>;

function readDotEnv(filePath: string): EnvMap {
  try {
    if (!fs.existsSync(filePath)) return {};
    const raw = fs.readFileSync(filePath, "utf8");
    const map: EnvMap = {};
    for (const line of raw.split(/\r?\n/)) {
      const trimmed = line.trim();
      if (!trimmed || trimmed.startsWith("#")) continue;
      const idx = trimmed.indexOf("=");
      if (idx <= 0) continue;
      const key = trimmed.slice(0, idx).trim();
      const value = trimmed.slice(idx + 1).trim();
      map[key] = value;
    }
    return map;
  } catch {
    return {};
  }
}

const projectLocalEnv = readDotEnv(path.join(process.cwd(), ".env.local"));
const projectEnv = readDotEnv(path.join(process.cwd(), ".env"));
const parentEnv = readDotEnv(path.join(process.cwd(), "..", ".env"));

function getEnv(name: string, fallback = ""): string {
  return (
    projectLocalEnv[name] ??
    process.env[name] ??
    projectEnv[name] ??
    parentEnv[name] ??
    fallback
  );
}

export const env = {
  apsClientId: getEnv("APS_CLIENT_ID"),
  apsClientSecret: getEnv("APS_CLIENT_SECRET"),
  apsCallbackUrl: getEnv(
    "APS_CALLBACK_URL",
    "http://localhost:3000/auth/callback",
  ),
  appBaseUrl: getEnv("APP_BASE_URL", "http://localhost:3000"),
  apsScope:
    getEnv("APS_SCOPE") ||
    [
      "data:read",
      "data:write",
      "data:create",
      "data:search",
      "user:read",
      "offline_access",
      "viewables:read",
      "account:read",
    ].join(" "),
  aiProvider: getEnv("AI_PROVIDER", "xai").toLowerCase(),
  aiOpenAiKey: getEnv("OPENAI_API_KEY"),
  aiOpenAiModel: getEnv("OPENAI_MODEL", "gpt-4.1-mini"),
  aiXaiKey: getEnv("XAI_API_KEY"),
  aiXaiModel: getEnv("XAI_MODEL", "grok-4.20-multi-agent-0309"),
  chatLogLevel: getEnv("CHAT_LOG_LEVEL", "full").toLowerCase(),
  apsIssuesContainerId: getEnv("APS_ISSUES_CONTAINER_ID"),
  apsIssuesDefaultTypeId: getEnv("APS_ISSUES_DEFAULT_TYPE_ID"),
  apsDesignFilesBucket: getEnv("APS_DESIGN_FILES_BUCKET", ""),
};

export function assertApsCredentials(): void {
  if (!env.apsClientId) {
    throw new Error("Missing required environment variable: APS_CLIENT_ID");
  }
  if (!env.apsClientSecret) {
    throw new Error("Missing required environment variable: APS_CLIENT_SECRET");
  }
}

export function hasAnyAiProviderKey(): boolean {
  return Boolean(env.aiOpenAiKey || env.aiXaiKey);
}

