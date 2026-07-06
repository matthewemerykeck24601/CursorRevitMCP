import fs from "node:fs";
import { GoogleAuth } from "google-auth-library";
import { env } from "@/lib/env";
import { log } from "@/lib/logger";
import type { NativeFormFieldType } from "@/lib/acc-forms";

type ServiceAccountKey = {
  type?: string;
  project_id?: string;
  client_email?: string;
  private_key?: string;
};

export type DocAiSetupDiagnostics = {
  configured: boolean;
  enabled: boolean;
  keyFilePath?: string;
  keyFileExists?: boolean;
  keyFileReadable?: boolean;
  jsonValid?: boolean;
  hasClientEmail?: boolean;
  hasPrivateKey?: boolean;
  privateKeyIsString?: boolean;
  privateKeyLooksPem?: boolean;
  hasProjectId?: boolean;
  projectId?: string;
  processorId?: string;
  location?: string;
  apiEndpoint?: string;
  clientEmailDomain?: string;
  issues: string[];
};

/**
 * Minimal structural views of the Document AI response we consume. We only read
 * table geometry (normalized bounding boxes + header/body cells) used to group
 * AcroForm fields by spatial containment.
 */
type DocAiSegment = { startIndex?: string | number | null; endIndex?: string | number | null };
type DocAiTextAnchor = { textSegments?: DocAiSegment[] | null; content?: string | null };
type DocAiVertex = { x?: number | null; y?: number | null };
type DocAiBoundingPoly = { normalizedVertices?: DocAiVertex[] | null };
type DocAiLayout = {
  textAnchor?: DocAiTextAnchor | null;
  boundingPoly?: DocAiBoundingPoly | null;
};
type DocAiTableCell = { layout?: DocAiLayout | null };
type DocAiTableRow = { cells?: DocAiTableCell[] | null };
type DocAiTable = {
  layout?: DocAiLayout | null;
  headerRows?: DocAiTableRow[] | null;
  bodyRows?: DocAiTableRow[] | null;
};
type DocAiPage = { tables?: DocAiTable[] | null };
type DocAiDocument = { text?: string | null; pages?: DocAiPage[] | null };

export type DocAiConfig = {
  enabled: boolean;
  keyFile: string;
  projectId: string;
  location: string;
  processorId: string;
};

export function resolveDocAiConfig(): DocAiConfig | null {
  if (!env.googleDocAiEnabled) return null;
  const keyFile = env.googleDocAiKeyFile?.trim();
  const processorId = env.googleDocAiProcessorId?.trim();
  if (!keyFile || !processorId) return null;
  if (!fs.existsSync(keyFile)) return null;
  let projectId = env.googleDocAiProjectId?.trim() ?? "";
  if (!projectId) {
    try {
      let rawKey = fs.readFileSync(keyFile, "utf8");
      if (rawKey.charCodeAt(0) === 0xfeff) rawKey = rawKey.slice(1);
      const raw = JSON.parse(rawKey) as { project_id?: string };
      projectId = (raw.project_id ?? "").trim();
    } catch {
      projectId = "";
    }
  }
  if (!projectId) return null;
  return {
    enabled: true,
    keyFile,
    projectId,
    location: env.googleDocAiLocation?.trim() || "us",
    processorId,
  };
}

export function isDocAiConfigured(): boolean {
  return resolveDocAiConfig() != null;
}

function readServiceAccountKey(keyFile: string): {
  serviceAccount: ServiceAccountKey;
  hadBom: boolean;
} {
  let rawKey = fs.readFileSync(keyFile, "utf8");
  const hadBom = rawKey.charCodeAt(0) === 0xfeff;
  if (hadBom) rawKey = rawKey.slice(1);
  return {
    serviceAccount: JSON.parse(rawKey) as ServiceAccountKey,
    hadBom,
  };
}

/**
 * Non-secret diagnostics for verifying GOOGLE_DOCAI_* env + key file shape.
 * Useful when gRPC returns opaque auth errors like "undefined undefined: undefined".
 */
export function getDocAiSetupDiagnostics(): DocAiSetupDiagnostics {
  const enabled = env.googleDocAiEnabled;
  const keyFile = env.googleDocAiKeyFile?.trim();
  const processorId = env.googleDocAiProcessorId?.trim();
  const location = env.googleDocAiLocation?.trim() || "us";
  const issues: string[] = [];

  if (!enabled) {
    issues.push("GOOGLE_DOCAI_ENABLED is not true.");
  }
  if (!keyFile) {
    issues.push("GOOGLE_DOCAI_KEY_FILE is empty.");
  }
  if (!processorId) {
    issues.push("GOOGLE_DOCAI_PROCESSOR_ID is empty.");
  }

  const diagnostics: DocAiSetupDiagnostics = {
    configured: false,
    enabled,
    keyFilePath: keyFile || undefined,
    processorId: processorId || undefined,
    location,
    issues,
  };

  if (!keyFile) return diagnostics;

  diagnostics.keyFileExists = fs.existsSync(keyFile);
  if (!diagnostics.keyFileExists) {
    issues.push(`Key file not found at configured path.`);
    return diagnostics;
  }

  let serviceAccount: ServiceAccountKey | null = null;
  try {
    const parsed = readServiceAccountKey(keyFile);
    serviceAccount = parsed.serviceAccount;
    diagnostics.keyFileReadable = true;
    diagnostics.jsonValid = true;
    if (parsed.hadBom) {
      issues.push("Key file had a UTF-8 BOM (stripped automatically).");
    }
  } catch (error) {
    diagnostics.keyFileReadable = false;
    diagnostics.jsonValid = false;
    issues.push(
      `Key file is not readable/valid JSON: ${
        error instanceof Error ? error.message : "parse failed"
      }`,
    );
    return diagnostics;
  }

  diagnostics.hasClientEmail = Boolean(serviceAccount.client_email?.trim());
  diagnostics.hasPrivateKey = Boolean(serviceAccount.private_key);
  diagnostics.privateKeyIsString = typeof serviceAccount.private_key === "string";
  diagnostics.privateKeyLooksPem = /BEGIN (RSA )?PRIVATE KEY/.test(
    String(serviceAccount.private_key ?? ""),
  );
  if (!diagnostics.hasClientEmail) {
    issues.push("Key file is missing client_email.");
  }
  if (!diagnostics.hasPrivateKey) {
    issues.push("Key file is missing private_key.");
  } else if (!diagnostics.privateKeyIsString) {
    issues.push("private_key must be a string in the service-account JSON.");
  } else if (!diagnostics.privateKeyLooksPem) {
    issues.push(
      "private_key does not look like a PEM block (expected BEGIN PRIVATE KEY).",
    );
  }

  const projectId =
    env.googleDocAiProjectId?.trim() ||
    String(serviceAccount.project_id ?? "").trim();
  diagnostics.projectId = projectId || undefined;
  diagnostics.hasProjectId = Boolean(projectId);
  if (!diagnostics.hasProjectId) {
    issues.push(
      "Project id missing from GOOGLE_DOCAI_PROJECT_ID and key file project_id.",
    );
  }

  if (serviceAccount.client_email?.includes("@")) {
    diagnostics.clientEmailDomain = serviceAccount.client_email.split("@")[1];
  }

  diagnostics.apiEndpoint = `${location}-documentai.googleapis.com`;
  diagnostics.configured =
    enabled &&
    Boolean(keyFile && processorId && diagnostics.keyFileExists && diagnostics.jsonValid) &&
    diagnostics.hasClientEmail === true &&
    diagnostics.privateKeyIsString === true &&
    diagnostics.privateKeyLooksPem === true &&
    diagnostics.hasProjectId === true;

  if (diagnostics.configured && issues.length === 0) {
    diagnostics.issues = ["none"];
  }

  return diagnostics;
}

function isUselessGrpcMessage(message: string): boolean {
  const trimmed = message.trim();
  if (!trimmed) return true;
  if (trimmed === "undefined undefined: undefined") return true;
  if (/^undefined(\s+undefined)?(?::\s*undefined)?$/i.test(trimmed)) return true;
  return false;
}

/** Extract a readable failure string from gRPC / Google client errors. */
export function formatDocAiError(error: unknown): string {
  if (!(error && typeof error === "object")) {
    return String(error ?? "unknown error");
  }

  const e = error as Record<string, unknown>;
  const parts: string[] = [];

  if (typeof e.code === "number" || (typeof e.code === "string" && e.code.trim())) {
    parts.push(`code=${e.code}`);
  }
  if (typeof e.details === "string" && e.details.trim()) {
    parts.push(e.details.trim());
  }
  if (typeof e.reason === "string" && e.reason.trim()) {
    parts.push(`reason=${e.reason.trim()}`);
  }
  if (typeof e.note === "string" && e.note.trim()) {
    parts.push(`note=${e.note.trim()}`);
  }
  if (typeof e.domain === "string" && e.domain.trim()) {
    parts.push(`domain=${e.domain.trim()}`);
  }
  if (typeof e.message === "string") {
    const message = e.message.trim();
    if (message && !isUselessGrpcMessage(message)) {
      parts.push(message);
    }
  }

  const response = e.response as
    | { status?: number; statusText?: string; data?: unknown }
    | undefined;
  if (typeof response?.status === "number") {
    parts.push(
      `http=${response.status}${
        response.statusText ? ` ${response.statusText}` : ""
      }`,
    );
  }
  if (response?.data && typeof response.data === "object") {
    const data = response.data as Record<string, unknown>;
    const apiError = data.error;
    if (apiError && typeof apiError === "object") {
      const err = apiError as Record<string, unknown>;
      if (typeof err.message === "string" && err.message.trim()) {
        parts.push(err.message.trim());
      }
      if (typeof err.status === "string" && err.status.trim()) {
        parts.push(`status=${err.status.trim()}`);
      }
    }
  }

  if (Array.isArray(e.statusDetails)) {
    for (const item of e.statusDetails) {
      if (item && typeof item === "object") {
        const row = item as Record<string, unknown>;
        const reason = typeof row.reason === "string" ? row.reason.trim() : "";
        const domain = typeof row.domain === "string" ? row.domain.trim() : "";
        if (reason || domain) {
          parts.push(`statusDetail=${domain || "googleapis.com"}:${reason || "unknown"}`);
        }
      }
    }
  }

  if (Array.isArray(e.errors)) {
    for (const item of e.errors) {
      if (item && typeof item === "object") {
        const row = item as Record<string, unknown>;
        if (typeof row.message === "string" && row.message.trim()) {
          parts.push(row.message.trim());
        }
      }
    }
  }

  const cause = e.cause;
  if (cause && typeof cause === "object") {
    const nested = formatDocAiError(cause);
    if (nested && !parts.includes(nested)) {
      parts.push(`cause=${nested}`);
    }
  } else if (typeof cause === "string" && cause.trim()) {
    parts.push(`cause=${cause.trim()}`);
  }

  if (parts.length === 0) {
    const snapshot: Record<string, unknown> = {};
    for (const key of Object.getOwnPropertyNames(e)) {
      if (key === "stack") continue;
      const value = e[key];
      if (value == null) continue;
      if (typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
        snapshot[key] = value;
      }
    }
    if (Object.keys(snapshot).length > 0) {
      parts.push(JSON.stringify(snapshot));
    } else {
      parts.push(
        "unknown auth/API error (empty gRPC metadata — check service-account key file, processor id, and regional endpoint)",
      );
    }
  }

  return parts.join(" | ");
}

function logDocAiSetupDiagnostics(context: string): DocAiSetupDiagnostics {
  const diagnostics = getDocAiSetupDiagnostics();
  log("info", "docai-setup", {
    context,
    ...diagnostics,
  });
  return diagnostics;
}

function toInt(value: string | number | null | undefined): number {
  if (value == null) return 0;
  const n = typeof value === "number" ? value : Number.parseInt(value, 10);
  return Number.isFinite(n) ? n : 0;
}

function textFromAnchor(text: string, anchor: DocAiTextAnchor | null | undefined): string {
  if (!anchor) return "";
  if (typeof anchor.content === "string" && anchor.content.trim()) {
    return anchor.content.trim();
  }
  let out = "";
  for (const segment of anchor.textSegments ?? []) {
    const start = toInt(segment.startIndex);
    const end = toInt(segment.endIndex);
    if (end > start && end <= text.length) out += text.slice(start, end);
  }
  return out.replace(/\s+/g, " ").trim();
}

function cleanLabel(input: string): string {
  return input.replace(/[\r\n]+/g, " ").replace(/\s+/g, " ").replace(/[:*]+\s*$/, "").trim();
}

const CALC_HEADER = /\b(total|subtotal|sum|amount|balance|extended|ext\.?|calc|=)\b/i;

export function inferColumnType(label: string): NativeFormFieldType {
  const lower = label.toLowerCase();
  if (/\bdate\b|mm\/dd/.test(lower)) return "date";
  if (/sign(ature)?|initial/.test(lower)) return "signature";
  if (/\b(qty|quantity|amount|total|count|number|no\.|#|hours|cost|price|rate|psi|mph)\b/.test(lower)) {
    return "number";
  }
  if (/comment|notes?|description|remarks?|details/.test(lower)) return "multiline_text";
  return "text";
}

/** Normalized (top-left origin, 0..1) axis-aligned bounding box. */
export type NormBox = { x0: number; y0: number; x1: number; y1: number };

export type DocAiTableColumn = {
  label: string;
  type: NativeFormFieldType;
  calculated: boolean;
  x0: number;
  x1: number;
};

export type DocAiTableRegion = {
  id: string;
  page: number;
  title: string;
  bbox: NormBox;
  columns: DocAiTableColumn[];
  rowBands: Array<{ y0: number; y1: number }>;
  hasCalculatedColumns: boolean;
};

export type DocAiRegionResult = {
  regions: DocAiTableRegion[];
  pageCount: number;
  tableCount: number;
  notes: string[];
};

function boxFromPoly(poly: DocAiBoundingPoly | null | undefined): NormBox | null {
  const verts = poly?.normalizedVertices ?? [];
  if (verts.length === 0) return null;
  let x0 = Number.POSITIVE_INFINITY;
  let y0 = Number.POSITIVE_INFINITY;
  let x1 = Number.NEGATIVE_INFINITY;
  let y1 = Number.NEGATIVE_INFINITY;
  for (const v of verts) {
    const x = typeof v.x === "number" ? v.x : 0;
    const y = typeof v.y === "number" ? v.y : 0;
    if (x < x0) x0 = x;
    if (y < y0) y0 = y;
    if (x > x1) x1 = x;
    if (y > y1) y1 = y;
  }
  if (!Number.isFinite(x0) || !Number.isFinite(y0)) return null;
  return { x0, y0, x1, y1 };
}

function unionBox(boxes: NormBox[]): NormBox | null {
  if (boxes.length === 0) return null;
  return boxes.reduce((acc, b) => ({
    x0: Math.min(acc.x0, b.x0),
    y0: Math.min(acc.y0, b.y0),
    x1: Math.max(acc.x1, b.x1),
    y1: Math.max(acc.y1, b.y1),
  }));
}

function buildDocAiAccessToken(config: DocAiConfig): Promise<string> {
  const { serviceAccount } = readServiceAccountKey(config.keyFile);
  if (!serviceAccount.client_email || !serviceAccount.private_key) {
    throw new Error("Document AI key file is missing client_email/private_key.");
  }
  if (typeof serviceAccount.private_key !== "string") {
    throw new Error("Document AI private_key must be a string in the key file.");
  }
  const auth = new GoogleAuth({
    credentials: {
      client_email: serviceAccount.client_email,
      private_key: serviceAccount.private_key,
    },
    scopes: ["https://www.googleapis.com/auth/cloud-platform"],
  });
  return auth.getAccessToken().then((response) => {
    if (!response) {
      throw new Error("Failed to obtain Google access token for Document AI.");
    }
    return response;
  });
}

type DocAiRestErrorBody = {
  error?: {
    code?: number;
    message?: string;
    status?: string;
    details?: unknown[];
  };
};

async function processDocumentViaRest(
  config: DocAiConfig,
  buffer: Buffer,
): Promise<{ document?: DocAiDocument }> {
  const token = await buildDocAiAccessToken(config);
  const processorName = `projects/${config.projectId}/locations/${config.location}/processors/${config.processorId}`;
  const url = `https://${config.location}-documentai.googleapis.com/v1/${processorName}:process`;
  const maxAttempts = 3;

  for (let attempt = 1; attempt <= maxAttempts; attempt += 1) {
    const response = await fetch(url, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        rawDocument: {
          content: buffer.toString("base64"),
          mimeType: "application/pdf",
        },
      }),
    });

    const payload = (await response.json().catch(() => ({}))) as DocAiRestErrorBody & {
      document?: DocAiDocument;
    };

    if (response.ok) {
      return payload;
    }

    const retryable = response.status === 408 || response.status === 429 || response.status >= 500;
    const apiMessage =
      typeof payload.error?.message === "string"
        ? payload.error.message
        : response.statusText || "Document AI request failed";
    const apiStatus =
      typeof payload.error?.status === "string" ? payload.error.status : undefined;

    if (retryable && attempt < maxAttempts) {
      log("warn", "docai-rest-retry", {
        attempt,
        status: response.status,
        apiStatus,
        apiMessage,
      });
      await new Promise((resolve) => setTimeout(resolve, attempt * 1000));
      continue;
    }

    const err = new Error(
      `Document AI REST ${response.status}${apiStatus ? ` ${apiStatus}` : ""}: ${apiMessage}`,
    ) as Error & {
      code?: number;
      status?: string;
      response?: { status: number; statusText: string; data: unknown };
    };
    err.code = payload.error?.code ?? response.status;
    err.status = apiStatus;
    err.response = {
      status: response.status,
      statusText: response.statusText,
      data: payload,
    };
    throw err;
  }

  throw new Error("Document AI REST request failed after retries.");
}

/**
 * Run a PDF through Document AI (Form Parser) and return normalized table
 * regions for spatial grouping of AcroForm fields. No fields are synthesized;
 * the interactive inputs still come from the PDF's AcroForm layer.
 */
export async function extractDocAiTableRegions(
  buffer: Buffer,
): Promise<DocAiRegionResult> {
  const config = resolveDocAiConfig();
  if (!config) {
    const diagnostics = getDocAiSetupDiagnostics();
    log("error", "docai-not-configured", diagnostics);
    throw new Error(
      `Document AI is not configured (${diagnostics.issues.join("; ") || "missing env"})`,
    );
  }

  const diagnostics = logDocAiSetupDiagnostics("extractDocAiTableRegions");
  if (!diagnostics.configured) {
    log("error", "docai-setup-invalid", diagnostics);
    throw new Error(
      `Document AI setup is invalid (${diagnostics.issues.join("; ")})`,
    );
  }

  const name = `projects/${config.projectId}/locations/${config.location}/processors/${config.processorId}`;
  const startedAt = Date.now();
  log("info", "docai-process-start", {
    transport: "rest",
    projectId: config.projectId,
    location: config.location,
    processorId: config.processorId,
    apiEndpoint: `${config.location}-documentai.googleapis.com`,
    pdfBytes: buffer.length,
    processorName: name,
  });

  let result: { document?: DocAiDocument };
  try {
    result = await processDocumentViaRest(config, buffer);
  } catch (error) {
    const detail = formatDocAiError(error);
    log("error", "docai-process-failed", {
      detail,
      elapsedMs: Date.now() - startedAt,
      projectId: config.projectId,
      location: config.location,
      processorId: config.processorId,
      apiEndpoint: `${config.location}-documentai.googleapis.com`,
      pdfBytes: buffer.length,
      setup: getDocAiSetupDiagnostics(),
      errorName: error instanceof Error ? error.name : typeof error,
      stack: error instanceof Error ? error.stack : undefined,
    });
    throw error;
  }

  log("info", "docai-process-success", {
    transport: "rest",
    elapsedMs: Date.now() - startedAt,
    projectId: config.projectId,
    processorId: config.processorId,
    pdfBytes: buffer.length,
  });

  const document = (result.document ?? {}) as DocAiDocument;
  const text = document.text ?? "";
  const regions: DocAiTableRegion[] = [];
  const pages = document.pages ?? [];

  pages.forEach((page, pageIndex) => {
    const tables = page.tables ?? [];
    tables.forEach((table, tableIndex) => {
      const headerCells = (table.headerRows ?? [])[0]?.cells ?? [];
      const columns: DocAiTableColumn[] = [];
      for (const cell of headerCells) {
        const box = boxFromPoly(cell.layout?.boundingPoly);
        if (!box) continue;
        const label = cleanLabel(textFromAnchor(text, cell.layout?.textAnchor)) || `Column ${columns.length + 1}`;
        columns.push({
          label,
          type: inferColumnType(label),
          calculated: CALC_HEADER.test(label),
          x0: box.x0,
          x1: box.x1,
        });
      }

      // If no header row, derive columns from the first body row's cell x-ranges.
      const bodyRows = table.bodyRows ?? [];
      if (columns.length === 0 && bodyRows[0]?.cells) {
        bodyRows[0].cells.forEach((cell, idx) => {
          const box = boxFromPoly(cell.layout?.boundingPoly);
          if (!box) return;
          const label = cleanLabel(textFromAnchor(text, cell.layout?.textAnchor)) || `Column ${idx + 1}`;
          columns.push({
            label,
            type: inferColumnType(label),
            calculated: CALC_HEADER.test(label),
            x0: box.x0,
            x1: box.x1,
          });
        });
      }

      const rowBands: Array<{ y0: number; y1: number }> = [];
      const allCellBoxes: NormBox[] = [];
      for (const row of bodyRows) {
        const cellBoxes: NormBox[] = [];
        for (const cell of row.cells ?? []) {
          const box = boxFromPoly(cell.layout?.boundingPoly);
          if (box) {
            cellBoxes.push(box);
            allCellBoxes.push(box);
          }
        }
        const rowBox = unionBox(cellBoxes);
        if (rowBox) rowBands.push({ y0: rowBox.y0, y1: rowBox.y1 });
      }
      for (const cell of headerCells) {
        const box = boxFromPoly(cell.layout?.boundingPoly);
        if (box) allCellBoxes.push(box);
      }

      const tableBox =
        boxFromPoly(table.layout?.boundingPoly) ?? unionBox(allCellBoxes);
      if (!tableBox) return;

      const title =
        columns.length > 0
          ? columns.map((c) => c.label).slice(0, 3).join(" / ")
          : `Table ${tableIndex + 1}`;

      regions.push({
        id: `docai_table_p${pageIndex}_${tableIndex}`,
        page: pageIndex,
        title,
        bbox: tableBox,
        columns,
        rowBands,
        hasCalculatedColumns: columns.some((c) => c.calculated),
      });
    });
  });

  const notes: string[] = [];
  notes.push(
    `Document AI detected ${regions.length} table region(s) across ${pages.length} page(s).`,
  );

  return {
    regions,
    pageCount: pages.length,
    tableCount: regions.length,
    notes,
  };
}
