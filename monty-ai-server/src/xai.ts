/**
 * xAI Responses API — same surface as aps-ai-web `callXaiResponsesRaw`.
 * https://docs.x.ai/docs/api-reference
 */

export type XaiResponsesPayload = {
  output?: Array<{
    type?: string;
    content?: Array<{ type?: string; text?: string }>;
  }>;
};

export function extractXaiResponseText(json: XaiResponsesPayload): string {
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

const GROK_LIKE_INSTRUCTIONS = [
  "You are Grok, built by xAI, assisting the user inside the Monty iOS app (Autodesk / construction workflows when relevant).",
  "Be direct, helpful, and genuinely conversational — witty when it fits, never corporate.",
  "Skip filler openers (no “Certainly!”, “Great question!”). Get to the answer.",
  "Use markdown when it helps (headings, lists, code). Keep answers as tight as the topic allows unless the user asks for depth.",
  "If you lack project-specific data, say so and suggest what the user could check in ACC/Revit instead of guessing.",
].join("\n");

export async function callGrokResponses(args: {
  apiKey: string;
  model: string;
  userPrompt: string;
  temperature?: number;
  maxOutputTokens?: number;
}): Promise<string> {
  const {
    apiKey,
    model,
    userPrompt,
    temperature = 0.65,
    maxOutputTokens = 8192,
  } = args;

  const response = await fetch("https://api.x.ai/v1/responses", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${apiKey}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      model: model.trim(),
      temperature,
      store: false,
      max_output_tokens: maxOutputTokens,
      instructions: GROK_LIKE_INSTRUCTIONS,
      input: userPrompt,
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
