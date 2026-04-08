import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.join(__dirname, "..");

function readXaiKeyFromDotEnv(filePath) {
  if (!fs.existsSync(filePath)) return "";
  const raw = fs.readFileSync(filePath, "utf8");
  for (const line of raw.split(/\r?\n/)) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith("#")) continue;
    const idx = trimmed.indexOf("=");
    if (idx <= 0) continue;
    const key = trimmed.slice(0, idx).trim();
    if (key !== "XAI_API_KEY") continue;
    let val = trimmed.slice(idx + 1).trim();
    if (
      (val.startsWith('"') && val.endsWith('"')) ||
      (val.startsWith("'") && val.endsWith("'"))
    ) {
      val = val.slice(1, -1);
    }
    return val;
  }
  return "";
}

const key =
  readXaiKeyFromDotEnv(path.join(root, ".env.local")) ||
  readXaiKeyFromDotEnv(path.join(root, "..", ".env")) ||
  process.env.XAI_API_KEY ||
  "";

if (!key) {
  console.log("SKIP: set XAI_API_KEY in .env.local (or parent .env) to run this test.");
  process.exit(0);
}

const body = {
  model: "grok-4.20-multi-agent-0309",
  input: "What is 2+2? Reply with one digit only.",
  store: false,
};

const res = await fetch("https://api.x.ai/v1/responses", {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
    Authorization: `Bearer ${key}`,
  },
  body: JSON.stringify(body),
});

const json = await res.json();
const text = (json.output ?? [])
  .flatMap((item) =>
    (item.content ?? [])
      .filter((c) => c.type === "output_text")
      .map((c) => c.text ?? ""),
  )
  .join("")
  .slice(0, 200);

console.log(
  JSON.stringify({
    httpStatus: res.status,
    apiStatus: json.status ?? null,
    error: json.error ?? null,
    outputTextLen: text.length,
    outputTextPreview: text,
  }),
);

if (!res.ok) process.exit(1);
