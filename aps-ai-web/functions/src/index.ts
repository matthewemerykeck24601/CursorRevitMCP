import { onRequest } from "firebase-functions/v2/https";
import { defineSecret } from "firebase-functions/params";

const XAI_API_KEY = defineSecret("XAI_API_KEY");
const OPENAI_API_KEY = defineSecret("OPENAI_API_KEY");
const AI_GATEWAY_SHARED_SECRET = defineSecret("AI_GATEWAY_SHARED_SECRET");

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
  return parts.join("\n").trim();
}

export const aiGateway = onRequest(
  {
    region: "us-central1",
    secrets: [XAI_API_KEY, OPENAI_API_KEY, AI_GATEWAY_SHARED_SECRET],
    cors: true,
  },
  async (req, res) => {
    if (req.method !== "POST") {
      res.status(405).json({ error: "Method not allowed. Use POST." });
      return;
    }

    const expectedSecret = AI_GATEWAY_SHARED_SECRET.value().trim();
    if (!expectedSecret) {
      res.status(500).json({
        error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
      });
      return;
    }
    const receivedSecret = (req.header("x-ai-gateway-secret") ?? "").trim();
    if (!receivedSecret || receivedSecret !== expectedSecret) {
      res.status(401).json({ error: "Unauthorized AI gateway request." });
      return;
    }

    const body =
      req.body && typeof req.body === "object"
        ? (req.body as Record<string, unknown>)
        : {};
    const provider = String(body.provider ?? "xai").trim().toLowerCase();
    const prompt = String(body.prompt ?? "");
    const modelOverride = String(body.model ?? "").trim();
    const xaiDefaultModel = String(body.xaiDefaultModel ?? "grok-4.20-multi-agent-0309");
    const openAiDefaultModel = String(body.openAiDefaultModel ?? "gpt-4.1-mini");

    if (!prompt.trim()) {
      res.status(400).json({ error: "Missing prompt." });
      return;
    }

    try {
      if (provider === "xai") {
        const key = XAI_API_KEY.value();
        if (!key) {
          res.status(500).json({ error: "XAI_API_KEY secret is not configured." });
          return;
        }
        const response = await fetch("https://api.x.ai/v1/responses", {
          method: "POST",
          headers: {
            Authorization: `Bearer ${key}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            model: modelOverride || xaiDefaultModel,
            temperature: 0.25,
            store: false,
            max_output_tokens: 8192,
            input: prompt,
          }),
        });
        if (!response.ok) {
          const text = await response.text();
          res.status(response.status).json({ error: `xAI request failed: ${text}` });
          return;
        }
        const json = (await response.json()) as XaiResponsesPayload;
        res.status(200).json({ text: extractXaiResponseText(json) });
        return;
      }

      if (provider === "openai") {
        const key = OPENAI_API_KEY.value();
        if (!key) {
          res.status(500).json({ error: "OPENAI_API_KEY secret is not configured." });
          return;
        }
        const response = await fetch("https://api.openai.com/v1/chat/completions", {
          method: "POST",
          headers: {
            Authorization: `Bearer ${key}`,
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            model: modelOverride || openAiDefaultModel,
            temperature: 0.25,
            messages: [{ role: "user", content: prompt }],
          }),
        });
        if (!response.ok) {
          const text = await response.text();
          res
            .status(response.status)
            .json({ error: `OpenAI request failed: ${text}` });
          return;
        }
        const json = (await response.json()) as {
          choices?: Array<{
            message?: { content?: string | Array<{ text?: string }> };
          }>;
        };
        const content = json.choices?.[0]?.message?.content;
        const text =
          typeof content === "string"
            ? content
            : Array.isArray(content)
              ? content.map((c) => c.text ?? "").join("\n")
              : "";
        res.status(200).json({ text });
        return;
      }

      res.status(400).json({ error: `Unsupported provider: ${provider}` });
    } catch (error) {
      res.status(500).json({
        error: error instanceof Error ? error.message : "Unknown aiGateway error",
      });
    }
  },
);
