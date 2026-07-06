import fs from "node:fs/promises";
import path from "node:path";
import crypto from "node:crypto";
import {
  PDFCheckBox,
  PDFDict,
  PDFDropdown,
  PDFName,
  PDFOptionList,
  PDFRadioGroup,
  PDFRef,
  PDFSignature,
  PDFString,
  PDFTextField,
  PDFDocument,
  type PDFField,
} from "pdf-lib";
import { buildVersionOssGetArgument } from "@/lib/aps";
import { env } from "@/lib/env";
import {
  extractDocAiTableRegions,
  formatDocAiError,
  getDocAiSetupDiagnostics,
  isDocAiConfigured,
  type DocAiTableRegion,
} from "@/lib/google-docai";
import { log } from "@/lib/logger";

const APS_BASE = "https://developer.api.autodesk.com";
const FORMS_CACHE_DIR = path.join(process.cwd(), ".design-files", "form-builder");
const FORMS_UPLOADS_DIR = path.join(FORMS_CACHE_DIR, "uploads");
const FORMS_ANALYSIS_DIR = path.join(FORMS_CACHE_DIR, "analysis");

export type FormTemplateType =
  | "custom"
  | "inspection"
  | "quality"
  | "safety"
  | "daily_report";

export type NativeFormFieldType =
  | "text"
  | "multiline_text"
  | "number"
  | "checkbox"
  | "radio"
  | "dropdown"
  | "date"
  | "signature"
  | "section_heading"
  | "table";

export type FormBuilderReviewState = "auto" | "needs_review" | "approved" | "rejected";

/** Geometry of a field's widget on the source PDF page (PDF user-space units). */
export type FieldRect = {
  page: number;
  x: number;
  y: number;
  width: number;
  height: number;
};

/**
 * Autodesk Forms (Forma) form-builder element kinds we translate to.
 * - `field`: a single native question.
 * - `section`: a grouped block of questions; `entryMode: "multiple"` maps a
 *   heading-followed-by-fillable-rows pattern to a Forms "multiple entries" section.
 * - `table`: a tabular block with columns, including calculated columns.
 */
export type FormaElementKind = "field" | "section" | "table";

export type FormaTableColumn = {
  key: string;
  label: string;
  type: NativeFormFieldType;
  calculated: boolean;
  /** When calculated, the detected/declared formula expression (best-effort). */
  formula?: string;
};

export type FormaFieldElement = {
  kind: "field";
  fieldId: string;
  label: string;
  type: NativeFormFieldType;
  required: boolean;
  options: string[];
};

export type FormaSectionElement = {
  kind: "section";
  sectionId: string;
  title: string;
  entryMode: "single" | "multiple";
  fieldIds: string[];
  fields: FormaFieldElement[];
  rationale: string;
};

export type FormaTableElement = {
  kind: "table";
  tableId: string;
  title: string;
  columns: FormaTableColumn[];
  fieldIds: string[];
  hasCalculatedColumns: boolean;
  rationale: string;
};

export type FormaElement =
  | FormaFieldElement
  | FormaSectionElement
  | FormaTableElement;

export type FormaSchema = {
  elements: FormaElement[];
  sectionCount: number;
  tableCount: number;
  calculatedColumnCount: number;
  notes: string[];
};

export type FormBuilderSource =
  | {
      kind: "upload_token";
      uploadToken: string;
    }
  | {
      kind: "acc_version";
      projectId: string;
      versionId: string;
    }
  | {
      kind: "pdf_base64";
      base64: string;
      fileName?: string;
    }
  | {
      kind: "pdf_url";
      url: string;
      headers?: Record<string, string>;
    };

export type ParsedFormField = {
  id: string;
  sourceName: string;
  label: string;
  type: NativeFormFieldType;
  required: boolean;
  options: string[];
  confidence: number;
  reviewState: FormBuilderReviewState;
  notes: string[];
  /** Widget geometry on the source PDF, used for deterministic grouping. */
  rect?: FieldRect;
  /** AcroForm read-only flag (often indicates a calculated/derived cell). */
  readOnly?: boolean;
  /** True when a calculation action/formula was detected on the field. */
  calculated?: boolean;
  /** Best-effort detected/declared calculation expression. */
  formula?: string;
  /** Name-hierarchy prefix shared by sibling fields (e.g. table/section key). */
  groupKey?: string;
  /** Column key within a repeated row group (table column identity). */
  columnKey?: string;
  /** Row index within a repeated row group, when detected. */
  rowIndex?: number;
};

export type FormBuilderAnalysisResult = {
  analysisId: string;
  accountId: string;
  templateName: string;
  templateType: FormTemplateType;
  source: FormBuilderSource;
  extractedTextSnippet: string;
  extractedFieldCount: number;
  confidenceScore: number;
  status: "ready" | "needs_review";
  fields: ParsedFormField[];
  formaSchema: FormaSchema;
  reviewNotes: string[];
  createdAt: string;
};

export type FormBuilderCreateResult = {
  success: boolean;
  dryRun: boolean;
  analysisId: string;
  accountId: string;
  templateName: string;
  templateType: FormTemplateType;
  fieldsSubmitted: number;
  sectionsCreated: number;
  tablesCreated: number;
  calculatedColumns: number;
  templateId?: string;
  response?: unknown;
  message: string;
};

export type FieldOverride = {
  id: string;
  label?: string;
  type?: NativeFormFieldType;
  required?: boolean;
  options?: string[];
  reviewState?: FormBuilderReviewState;
  calculated?: boolean;
  formula?: string;
  /** Explicit grouping override so reviewers can force a section/table. */
  groupKey?: string;
};

type StoredUpload = {
  uploadToken: string;
  fileName: string;
  mimeType: string;
  sizeBytes: number;
  filePath: string;
  createdAt: string;
  hubId?: string;
  projectId?: string;
};

type StoredAnalysis = FormBuilderAnalysisResult & {
  pdfFilePath: string;
  sourceFileName: string;
  sourceMimeType: string;
};

function inferAccountId(params: { accountId?: string; hubId?: string }): string {
  const direct = params.accountId?.trim();
  if (direct) return direct;
  const hub = params.hubId?.trim() ?? "";
  if (!hub) {
    throw new Error("accountId or hubId is required.");
  }
  return hub.startsWith("b.") ? hub.slice(2) : hub;
}

function normalizeTemplateType(raw: string | undefined): FormTemplateType {
  const candidate = (raw ?? "custom").trim().toLowerCase();
  switch (candidate) {
    case "inspection":
    case "quality":
    case "safety":
    case "daily_report":
    case "custom":
      return candidate;
    default:
      return "custom";
  }
}

function normalizeFieldLabel(input: string): string {
  const text = input.trim();
  if (!text) return "Untitled Field";
  return text
    .replace(/[._-]+/g, " ")
    .replace(/\s+/g, " ")
    .replace(/\b\w/g, (m) => m.toUpperCase());
}

function normalizeFieldOptions(values: string[]): string[] {
  const seen = new Set<string>();
  const out: string[] = [];
  for (const value of values) {
    const item = value.trim();
    if (!item) continue;
    if (seen.has(item.toLowerCase())) continue;
    seen.add(item.toLowerCase());
    out.push(item);
  }
  return out;
}

function makeId(prefix: string): string {
  return `${prefix}_${crypto.randomUUID().replace(/-/g, "")}`;
}

async function ensureCacheDirs(): Promise<void> {
  await fs.mkdir(FORMS_UPLOADS_DIR, { recursive: true });
  await fs.mkdir(FORMS_ANALYSIS_DIR, { recursive: true });
}

function uploadMetaPath(uploadToken: string): string {
  return path.join(FORMS_UPLOADS_DIR, `${uploadToken}.json`);
}

function analysisMetaPath(analysisId: string): string {
  return path.join(FORMS_ANALYSIS_DIR, `${analysisId}.json`);
}

async function writeJson(filePath: string, payload: unknown): Promise<void> {
  await fs.writeFile(filePath, JSON.stringify(payload, null, 2), "utf8");
}

async function readJson<T>(filePath: string): Promise<T> {
  const raw = await fs.readFile(filePath, "utf8");
  return JSON.parse(raw) as T;
}

function mapPdfFieldType(field: unknown): {
  type: NativeFormFieldType;
  options: string[];
  notes: string[];
} {
  if (field instanceof PDFTextField) {
    const isMultiline = field.isMultiline();
    return {
      type: isMultiline ? "multiline_text" : "text",
      options: [],
      notes: [],
    };
  }
  if (field instanceof PDFCheckBox) {
    return { type: "checkbox", options: [], notes: [] };
  }
  if (field instanceof PDFRadioGroup) {
    return {
      type: "radio",
      options: normalizeFieldOptions(field.getOptions()),
      notes: [],
    };
  }
  if (field instanceof PDFDropdown) {
    return {
      type: "dropdown",
      options: normalizeFieldOptions(field.getOptions()),
      notes: [],
    };
  }
  if (field instanceof PDFOptionList) {
    return {
      type: "dropdown",
      options: normalizeFieldOptions(field.getOptions()),
      notes: ["Option list mapped to dropdown."],
    };
  }
  if (field instanceof PDFSignature) {
    return { type: "signature", options: [], notes: [] };
  }
  return {
    type: "text",
    options: [],
    notes: ["Unsupported/unknown PDF field type mapped to text."],
  };
}

/** Best-effort read of an AcroForm calculation action (`/AA /C`) and its JS formula. */
function detectCalculation(field: PDFField): { calculated: boolean; formula?: string } {
  try {
    const dict = field.acroField.dict;
    const aa = dict.lookupMaybe(PDFName.of("AA"), PDFDict);
    if (!aa) return { calculated: false };
    const calc = aa.lookupMaybe(PDFName.of("C"), PDFDict);
    if (!calc) return { calculated: false };
    const js = calc.lookup(PDFName.of("JS"));
    let formula: string | undefined;
    if (js instanceof PDFString) {
      formula = js.decodeText().trim().slice(0, 400);
    }
    return { calculated: true, ...(formula ? { formula } : {}) };
  } catch {
    return { calculated: false };
  }
}

/** Resolve widget geometry + page index for a field's first widget annotation. */
function detectFieldRect(
  field: PDFField,
  pageRefIndex: Map<string, number>,
): FieldRect | undefined {
  try {
    const widgets = field.acroField.getWidgets();
    if (!widgets || widgets.length === 0) return undefined;
    const widget = widgets[0];
    const rect = widget.getRectangle();
    let page = 0;
    const pRef = widget.dict.get(PDFName.of("P"));
    if (pRef instanceof PDFRef) {
      const idx = pageRefIndex.get(pRef.toString());
      if (typeof idx === "number") page = idx;
    }
    return {
      page,
      x: Number(rect.x.toFixed(2)),
      y: Number(rect.y.toFixed(2)),
      width: Number(rect.width.toFixed(2)),
      height: Number(rect.height.toFixed(2)),
    };
  } catch {
    return undefined;
  }
}

async function extractPdfText(buffer: Buffer): Promise<string> {
  try {
    const mod = (await import("pdf-parse")) as unknown as {
      default: (raw: Buffer) => Promise<{ text?: string }>;
    };
    const parsed = await mod.default(buffer);
    return (parsed.text ?? "").trim();
  } catch {
    return "";
  }
}

function buildDefaultTemplateName(fileName: string): string {
  const base = fileName.replace(/\.pdf$/i, "").trim();
  return base || `Template ${new Date().toISOString()}`;
}

async function resolveSourcePdf(
  accessToken: string,
  source: FormBuilderSource,
): Promise<{ buffer: Buffer; fileName: string; mimeType: string }> {
  if (source.kind === "upload_token") {
    const meta = await readJson<StoredUpload>(uploadMetaPath(source.uploadToken));
    const bytes = await fs.readFile(meta.filePath);
    return { buffer: bytes, fileName: meta.fileName, mimeType: meta.mimeType };
  }
  if (source.kind === "pdf_base64") {
    const clean = source.base64.replace(/^data:application\/pdf;base64,/i, "").trim();
    const bytes = Buffer.from(clean, "base64");
    if (bytes.length === 0) {
      throw new Error("pdf_base64 was empty or invalid.");
    }
    return {
      buffer: bytes,
      fileName: source.fileName?.trim() || `upload_${Date.now()}.pdf`,
      mimeType: "application/pdf",
    };
  }
  if (source.kind === "pdf_url") {
    const res = await fetch(source.url, {
      method: "GET",
      headers: source.headers ?? {},
      cache: "no-store",
    });
    if (!res.ok) {
      throw new Error(`Failed downloading pdf_url (${res.status}).`);
    }
    const contentType = res.headers.get("content-type") || "application/pdf";
    const arr = await res.arrayBuffer();
    return {
      buffer: Buffer.from(arr),
      fileName: `url_${Date.now()}.pdf`,
      mimeType: contentType,
    };
  }
  const inputArg = await buildVersionOssGetArgument({
    accessToken,
    projectId: source.projectId,
    versionId: source.versionId,
  });
  if (!inputArg?.url) {
    throw new Error("Unable to resolve signed download URL for ACC version.");
  }
  const res = await fetch(inputArg.url, {
    method: "GET",
    headers: inputArg.headers ?? {},
    cache: "no-store",
  });
  if (!res.ok) {
    throw new Error(`Failed downloading ACC version PDF (${res.status}).`);
  }
  const arr = await res.arrayBuffer();
  return {
    buffer: Buffer.from(arr),
    fileName: `acc_version_${source.versionId}.pdf`,
    mimeType: res.headers.get("content-type") || "application/pdf",
  };
}

/**
 * Parse a PDF AcroForm field name into a grouping hierarchy.
 * Recognizes dotted, bracketed, and trailing-index conventions, e.g.
 *   "Inspection.Row2.Result" -> { groupKey: "Inspection", rowIndex: 2, columnKey: "Result" }
 *   "items[0].qty"           -> { groupKey: "items", rowIndex: 0, columnKey: "qty" }
 *   "lineItem_3_total"       -> { groupKey: "lineItem", rowIndex: 3, columnKey: "total" }
 */
export function parseFieldNameHierarchy(name: string): {
  groupKey?: string;
  columnKey?: string;
  rowIndex?: number;
} {
  const raw = (name ?? "").trim();
  if (!raw) return {};
  const segments = raw
    .replace(/\]/g, "")
    .split(/[.[_\-/]+/)
    .map((s) => s.trim())
    .filter(Boolean);
  if (segments.length < 2) return {};

  let rowIndex: number | undefined;
  const nonIndexSegments: string[] = [];
  for (const segment of segments) {
    const rowMatch = segment.match(/^(?:row|item|line|r|n)?(\d{1,4})$/i);
    if (rowMatch && /\d/.test(segment)) {
      const parsed = Number.parseInt(rowMatch[1], 10);
      if (Number.isFinite(parsed)) {
        rowIndex = parsed;
        continue;
      }
    }
    nonIndexSegments.push(segment);
  }
  if (nonIndexSegments.length === 0) return { rowIndex };
  const groupKey = nonIndexSegments[0];
  const columnKey =
    nonIndexSegments.length > 1
      ? nonIndexSegments.slice(1).join(" ")
      : undefined;
  return {
    groupKey: groupKey || undefined,
    columnKey: columnKey || undefined,
    ...(rowIndex != null ? { rowIndex } : {}),
  };
}

function fieldToFormaField(field: ParsedFormField): FormaFieldElement {
  return {
    kind: "field",
    fieldId: field.id,
    label: field.label,
    type: field.type,
    required: field.required,
    options: field.options,
  };
}

/**
 * Deterministically translate reviewed fields into an Autodesk Forms (Forma)
 * builder structure:
 * - repeated-row groups (or groups with calculated cells) become `table` elements,
 *   with read-only/calculated cells flagged as calculated columns;
 * - a heading followed by fillable fields becomes a `section`, using
 *   `entryMode: "multiple"` when the rows repeat (the "multiple entries" pattern);
 * - everything else stays a standalone `field`.
 */
export function buildFormaSchema(fields: ParsedFormField[]): FormaSchema {
  const notes: string[] = [];
  const elements: FormaElement[] = [];

  // Preserve first-seen order of groups while bucketing members.
  const groupOrder: string[] = [];
  const groups = new Map<string, ParsedFormField[]>();
  const standalone: ParsedFormField[] = [];

  for (const field of fields) {
    const key = field.groupKey?.trim();
    if (!key) {
      standalone.push(field);
      continue;
    }
    if (!groups.has(key)) {
      groups.set(key, []);
      groupOrder.push(key);
    }
    groups.get(key)!.push(field);
  }

  const emittedGroups = new Set<string>();

  const flushStandalonePreceding = (beforeFieldId: string | null) => {
    // Standalone fields are emitted in their original order via the main loop;
    // this helper is intentionally a no-op placeholder for clarity.
    void beforeFieldId;
  };

  // Walk original order; when we hit the first member of a group, emit the
  // whole group as a section/table at that position. Standalone fields emit inline.
  for (const field of fields) {
    const key = field.groupKey?.trim();
    if (!key) {
      flushStandalonePreceding(field.id);
      elements.push(fieldToFormaField(field));
      continue;
    }
    if (emittedGroups.has(key)) continue;
    emittedGroups.add(key);

    const members = groups.get(key) ?? [];
    const headingField = members.find((m) => m.type === "section_heading");
    const fillable = members.filter((m) => m.type !== "section_heading");
    const distinctRows = new Set(
      members
        .map((m) => m.rowIndex)
        .filter((r): r is number => typeof r === "number"),
    );
    const calculatedMembers = members.filter(
      (m) => m.calculated || m.readOnly,
    );
    const isTable = distinctRows.size >= 2 || calculatedMembers.length > 0;

    if (isTable) {
      // Build columns from distinct columnKey (fallback to label) order.
      const columns: FormaTableColumn[] = [];
      const seenColumns = new Set<string>();
      for (const member of fillable.length > 0 ? fillable : members) {
        const colKey =
          member.columnKey?.trim() ||
          member.label.trim() ||
          member.sourceName.trim();
        const dedupeKey = colKey.toLowerCase();
        if (seenColumns.has(dedupeKey)) continue;
        seenColumns.add(dedupeKey);
        const calculated = Boolean(member.calculated || member.readOnly);
        columns.push({
          key: colKey,
          label: member.label,
          type: member.type === "section_heading" ? "text" : member.type,
          calculated,
          ...(member.formula ? { formula: member.formula } : {}),
        });
      }
      const hasCalculatedColumns = columns.some((c) => c.calculated);
      elements.push({
        kind: "table",
        tableId: `table_${key}`,
        title: headingField?.label || normalizeFieldLabel(key),
        columns,
        fieldIds: members.map((m) => m.id),
        hasCalculatedColumns,
        rationale:
          distinctRows.size >= 2
            ? "Repeated row indices in field names indicate a tabular block."
            : "Calculated/read-only cells indicate a computed table block.",
      });
      notes.push(
        `Group "${key}" mapped to a table${
          hasCalculatedColumns ? " with calculated columns" : ""
        }.`,
      );
      continue;
    }

    const headingDetected = Boolean(headingField);
    const homogeneousFillable =
      fillable.length >= 2 &&
      new Set(fillable.map((m) => m.type)).size === 1;
    const entryMode: "single" | "multiple" =
      distinctRows.size >= 2 || (headingDetected && homogeneousFillable)
        ? "multiple"
        : "single";

    elements.push({
      kind: "section",
      sectionId: `section_${key}`,
      title: headingField?.label || normalizeFieldLabel(key),
      entryMode,
      fieldIds: fillable.map((m) => m.id),
      fields: fillable.map((m) => fieldToFormaField(m)),
      rationale:
        entryMode === "multiple"
          ? "Heading followed by repeating fillable fields maps to a multiple-entries section."
          : "Grouped fields under a shared heading map to a single section.",
    });
    notes.push(
      `Group "${key}" mapped to a ${entryMode}-entry section.`,
    );
  }

  const sectionCount = elements.filter((e) => e.kind === "section").length;
  const tableCount = elements.filter((e) => e.kind === "table").length;
  const calculatedColumnCount = elements.reduce((sum, element) => {
    if (element.kind !== "table") return sum;
    return sum + element.columns.filter((c) => c.calculated).length;
  }, 0);

  return {
    elements,
    sectionCount,
    tableCount,
    calculatedColumnCount,
    notes,
  };
}

function applyFieldOverrides(
  fields: ParsedFormField[],
  overrides: FieldOverride[] | undefined,
): ParsedFormField[] {
  if (!overrides || overrides.length === 0) return fields;
  const byId = new Map(overrides.map((o) => [o.id, o]));
  return fields.map((field) => {
    const patch = byId.get(field.id);
    if (!patch) return field;
    return {
      ...field,
      label:
        typeof patch.label === "string"
          ? normalizeFieldLabel(patch.label)
          : field.label,
      type: patch.type ?? field.type,
      required: patch.required ?? field.required,
      options: patch.options ? normalizeFieldOptions(patch.options) : field.options,
      reviewState: patch.reviewState ?? field.reviewState,
      calculated: patch.calculated ?? field.calculated,
      formula: typeof patch.formula === "string" ? patch.formula : field.formula,
      groupKey:
        typeof patch.groupKey === "string" ? patch.groupKey.trim() : field.groupKey,
    };
  });
}

/**
 * Apply reviewer-driven ordering and removal.
 * When `orderedFieldIds` is provided, only those ids are kept, in that order;
 * any stored field omitted from the list is treated as removed.
 */
function applyFieldOrdering(
  fields: ParsedFormField[],
  orderedFieldIds: string[] | undefined,
): ParsedFormField[] {
  if (!orderedFieldIds || orderedFieldIds.length === 0) return fields;
  const byId = new Map(fields.map((f) => [f.id, f]));
  const ordered: ParsedFormField[] = [];
  const seen = new Set<string>();
  for (const id of orderedFieldIds) {
    const field = byId.get(id);
    if (!field || seen.has(id)) continue;
    seen.add(id);
    ordered.push(field);
  }
  return ordered;
}

function toFormsEndpointPath(accountId: string): string {
  const configured =
    env.apsFormsTemplateCreatePath?.trim() || "/accounts/{accountId}/form-templates";
  return configured.replaceAll("{accountId}", encodeURIComponent(accountId));
}

async function callCreateTemplateApi(params: {
  accessToken: string;
  accountId: string;
  templateName: string;
  templateType: FormTemplateType;
  fields: ParsedFormField[];
  formaSchema: FormaSchema;
  analysisId: string;
}): Promise<{ templateId?: string; response: unknown }> {
  const base = env.apsFormsApiBaseUrl?.trim() || `${APS_BASE}/construction/forms/v1`;
  const endpoint = `${base.replace(/\/+$/, "")}${toFormsEndpointPath(params.accountId)}`;
  const body = {
    name: params.templateName,
    templateType: params.templateType,
    source: "ai_pdf_builder",
    fields: params.fields.map((field) => ({
      key: field.id,
      label: field.label,
      type: field.type,
      required: field.required,
      options: field.options,
      metadata: {
        source_name: field.sourceName,
        confidence: field.confidence,
        calculated: Boolean(field.calculated),
        ...(field.formula ? { formula: field.formula } : {}),
      },
    })),
    // Forma builder structure: sections (incl. multiple-entries) and tables with
    // calculated columns, translated deterministically from the reviewed fields.
    builder: {
      elements: params.formaSchema.elements,
      sectionCount: params.formaSchema.sectionCount,
      tableCount: params.formaSchema.tableCount,
      calculatedColumnCount: params.formaSchema.calculatedColumnCount,
    },
    metadata: {
      analysis_id: params.analysisId,
      created_by: "aps-ai-web-mcp",
    },
  };

  const response = await fetch(endpoint, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${params.accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
    cache: "no-store",
  });
  const text = await response.text();
  let parsed: unknown = text;
  try {
    parsed = text ? (JSON.parse(text) as unknown) : {};
  } catch {
    // keep raw text
  }
  if (!response.ok) {
    throw new Error(
      `Forms template create failed (${response.status}): ${
        typeof parsed === "string" ? parsed : JSON.stringify(parsed)
      }`,
    );
  }

  const templateId =
    typeof (parsed as { id?: unknown }).id === "string"
      ? ((parsed as { id: string }).id ?? "").trim()
      : typeof (parsed as { templateId?: unknown }).templateId === "string"
        ? ((parsed as { templateId: string }).templateId ?? "").trim()
        : "";

  return {
    templateId: templateId || undefined,
    response: parsed,
  };
}

export async function saveUploadedPdf(params: {
  fileName: string;
  mimeType: string;
  buffer: Buffer;
  hubId?: string;
  projectId?: string;
}): Promise<StoredUpload> {
  await ensureCacheDirs();
  const uploadToken = makeId("pdf");
  const safeFileName = params.fileName.trim() || `${uploadToken}.pdf`;
  const filePath = path.join(FORMS_UPLOADS_DIR, `${uploadToken}.pdf`);
  await fs.writeFile(filePath, params.buffer);
  const payload: StoredUpload = {
    uploadToken,
    fileName: safeFileName,
    mimeType: params.mimeType || "application/pdf",
    sizeBytes: params.buffer.length,
    filePath,
    createdAt: new Date().toISOString(),
    ...(params.hubId ? { hubId: params.hubId } : {}),
    ...(params.projectId ? { projectId: params.projectId } : {}),
  };
  await writeJson(uploadMetaPath(uploadToken), payload);
  return payload;
}

export async function analyzePdfForFormTemplate(params: {
  accessToken: string;
  source: FormBuilderSource;
  templateName?: string;
  templateType?: string;
  accountId?: string;
  hubId?: string;
}): Promise<FormBuilderAnalysisResult> {
  await ensureCacheDirs();
  const accountId = inferAccountId({
    accountId: params.accountId,
    hubId: params.hubId,
  });
  const sourcePdf = await resolveSourcePdf(params.accessToken, params.source);
  const templateName = params.templateName?.trim() || buildDefaultTemplateName(sourcePdf.fileName);
  const templateType = normalizeTemplateType(params.templateType);
  const parsedText = await extractPdfText(sourcePdf.buffer);
  const reviewNotes: string[] = [];

  const pdfDoc = await PDFDocument.load(sourcePdf.buffer, { ignoreEncryption: true });
  const form = pdfDoc.getForm();
  const rawFields = form.getFields();

  const pageRefIndex = new Map<string, number>();
  const pageSizes = new Map<number, { width: number; height: number }>();
  pdfDoc.getPages().forEach((page, idx) => {
    pageRefIndex.set(page.ref.toString(), idx);
    const size = page.getSize();
    pageSizes.set(idx, { width: size.width, height: size.height });
  });

  const fields: ParsedFormField[] = rawFields.map((field, index) => {
    const sourceName = field.getName();
    const mapped = mapPdfFieldType(field);
    const notes = [...mapped.notes];
    let confidence = 0.95;
    let reviewState: FormBuilderReviewState = "auto";
    if (mapped.notes.length > 0) {
      confidence = 0.6;
      reviewState = "needs_review";
    }
    const normalizedLabel = normalizeFieldLabel(sourceName);
    if (!sourceName.trim()) {
      notes.push("Field had no source name; generated fallback.");
      reviewState = "needs_review";
      confidence = Math.min(confidence, 0.5);
    }

    const hierarchy = parseFieldNameHierarchy(sourceName);
    const rect = detectFieldRect(field, pageRefIndex);
    let readOnly = false;
    try {
      readOnly = field.isReadOnly();
    } catch {
      readOnly = false;
    }
    const calc = detectCalculation(field);
    const calculated = calc.calculated || readOnly;
    if (calculated) {
      notes.push(
        calc.calculated
          ? "Calculation action detected; mapped as a calculated cell."
          : "Read-only field detected; treated as a calculated/derived cell.",
      );
    }

    return {
      id: `${sourceName || "field"}_${index + 1}`,
      sourceName: sourceName || `field_${index + 1}`,
      label: normalizedLabel,
      type: mapped.type,
      required: false,
      options: mapped.options,
      confidence,
      reviewState,
      notes,
      ...(rect ? { rect } : {}),
      readOnly,
      calculated,
      ...(calc.formula ? { formula: calc.formula } : {}),
      ...(hierarchy.groupKey ? { groupKey: hierarchy.groupKey } : {}),
      ...(hierarchy.columnKey ? { columnKey: hierarchy.columnKey } : {}),
      ...(hierarchy.rowIndex != null ? { rowIndex: hierarchy.rowIndex } : {}),
    };
  });

  // Hybrid layout pass: keep all AcroForm inputs, then group them into the
  // Document AI-detected table regions by bounding-box containment.
  let parser: "hybrid_docai" | "acroform" = "acroform";
  if (isDocAiConfigured()) {
    try {
      const docAi = await extractDocAiTableRegions(sourcePdf.buffer);
      reviewNotes.push(...docAi.notes);
      if (docAi.regions.length > 0) {
        const mapped = assignFieldsToDocAiTables(fields, docAi.regions, pageSizes);
        reviewNotes.push(
          `Mapped ${mapped} field(s) into Document AI table regions by containment.`,
        );
        parser = "hybrid_docai";
      }
    } catch (error) {
      const detail = formatDocAiError(error);
      const setup = getDocAiSetupDiagnostics();
      log("warn", "docai-layout-pass-failed", {
        detail,
        setupIssues: setup.issues,
        configured: setup.configured,
        projectId: setup.projectId,
        processorId: setup.processorId,
        location: setup.location,
        apiEndpoint: setup.apiEndpoint,
        keyFileExists: setup.keyFileExists,
        privateKeyLooksPem: setup.privateKeyLooksPem,
        stack: error instanceof Error ? error.stack : undefined,
      });
      reviewNotes.push(
        `Document AI layout pass failed; used local AcroForm extraction only (${detail}).`,
      );
      if (setup.issues.length > 0 && setup.issues[0] !== "none") {
        reviewNotes.push(`Document AI setup check: ${setup.issues.join("; ")}.`);
      }
    }
  }

  return finalizeAnalysis({
    source: params.source,
    accountId,
    templateName,
    templateType,
    fields,
    parsedText,
    reviewNotes,
    sourcePdf,
    parser,
  });
}

/**
 * Assign AcroForm fields to Document AI table regions by spatial containment in
 * normalized (top-left origin) coordinates, deriving column + row for each
 * contained field. Returns the number of fields mapped into a table.
 */
function assignFieldsToDocAiTables(
  fields: ParsedFormField[],
  regions: DocAiTableRegion[],
  pageSizes: Map<number, { width: number; height: number }>,
): number {
  let mappedCount = 0;
  // Track row bands discovered per region so fields without a body-row match
  // still get a stable rowIndex by vertical order.
  const regionRowOrder = new Map<string, number[]>();

  for (const field of fields) {
    const rect = field.rect;
    if (!rect) continue;
    const size = pageSizes.get(rect.page);
    if (!size || size.width <= 0 || size.height <= 0) continue;

    // AcroForm rect is bottom-left origin; convert center to normalized top-left.
    const cx = (rect.x + rect.width / 2) / size.width;
    const cyTop = 1 - (rect.y + rect.height / 2) / size.height;

    const region = regions.find(
      (r) =>
        r.page === rect.page &&
        cx >= r.bbox.x0 &&
        cx <= r.bbox.x1 &&
        cyTop >= r.bbox.y0 &&
        cyTop <= r.bbox.y1,
    );
    if (!region) continue;

    // Column by x-containment, else nearest column center.
    let column = region.columns.find((c) => cx >= c.x0 && cx <= c.x1);
    if (!column && region.columns.length > 0) {
      column = region.columns.reduce((best, c) => {
        const bestDist = Math.abs((best.x0 + best.x1) / 2 - cx);
        const cDist = Math.abs((c.x0 + c.x1) / 2 - cx);
        return cDist < bestDist ? c : best;
      });
    }

    // Row by y-containment, else by quantized vertical order within the region.
    let rowIndex = region.rowBands.findIndex((b) => cyTop >= b.y0 && cyTop <= b.y1);
    if (rowIndex < 0) {
      const order = regionRowOrder.get(region.id) ?? [];
      // Bucket by ~1% bands to keep same-row fields aligned without geometry noise.
      const bucket = Math.round(cyTop * 100);
      let idx = order.indexOf(bucket);
      if (idx < 0) {
        order.push(bucket);
        order.sort((a, b) => a - b);
        regionRowOrder.set(region.id, order);
        idx = order.indexOf(bucket);
      }
      rowIndex = idx;
    }

    field.groupKey = region.id;
    field.columnKey = column?.label ?? field.columnKey ?? field.label;
    field.rowIndex = rowIndex < 0 ? 0 : rowIndex;
    if (column?.calculated) {
      field.calculated = true;
      field.notes.push(
        `Mapped to calculated column "${column.label}" via Document AI table layout.`,
      );
    } else {
      field.notes.push(
        `Mapped to Document AI table "${region.title}" (column "${column?.label ?? "?"}").`,
      );
    }
    mappedCount += 1;
  }

  return mappedCount;
}

/**
 * Shared finalize step for both the Document AI and AcroForm parse paths:
 * flags duplicate labels, scores confidence, builds the Forma schema, persists
 * the analysis artifact, and returns the public result.
 */
async function finalizeAnalysis(params: {
  source: FormBuilderSource;
  accountId: string;
  templateName: string;
  templateType: FormTemplateType;
  fields: ParsedFormField[];
  parsedText: string;
  reviewNotes: string[];
  sourcePdf: { buffer: Buffer; fileName: string; mimeType: string };
  parser: "hybrid_docai" | "acroform";
}): Promise<FormBuilderAnalysisResult> {
  const { fields, reviewNotes } = params;

  if (fields.length === 0) {
    reviewNotes.push(
      "No form fields were detected in this PDF. Review is required before creating a native template.",
    );
  }

  const duplicateLabelSet = new Set<string>();
  const seenLabels = new Set<string>();
  for (const field of fields) {
    const key = field.label.toLowerCase();
    if (seenLabels.has(key)) duplicateLabelSet.add(key);
    seenLabels.add(key);
  }
  if (duplicateLabelSet.size > 0) {
    // Repeated table column labels across rows are expected; only flag dupes
    // that are NOT part of a recognized table/row group.
    for (const field of fields) {
      const inRowGroup = field.rowIndex != null && Boolean(field.groupKey);
      if (!inRowGroup && duplicateLabelSet.has(field.label.toLowerCase())) {
        field.reviewState = "needs_review";
        field.confidence = Math.min(field.confidence, 0.5);
        field.notes.push("Duplicate label detected; confirm unique mapping.");
      }
    }
  }

  const confidenceScore =
    fields.length > 0
      ? Number(
          (
            fields.reduce((sum, field) => sum + field.confidence, 0) / fields.length
          ).toFixed(3),
        )
      : 0;
  const needsReview =
    fields.length === 0 || fields.some((field) => field.reviewState === "needs_review");
  const status: FormBuilderAnalysisResult["status"] = needsReview ? "needs_review" : "ready";
  const analysisId = makeId("analysis");
  const pdfFilePath = path.join(FORMS_ANALYSIS_DIR, `${analysisId}.pdf`);
  await fs.writeFile(pdfFilePath, params.sourcePdf.buffer);

  const formaSchema = buildFormaSchema(fields);
  if (formaSchema.tableCount > 0) {
    reviewNotes.push(
      `Detected ${formaSchema.tableCount} table block(s)${
        formaSchema.calculatedColumnCount > 0
          ? ` with ${formaSchema.calculatedColumnCount} calculated column(s)`
          : ""
      }.`,
    );
  }
  if (formaSchema.sectionCount > 0) {
    reviewNotes.push(`Detected ${formaSchema.sectionCount} section block(s).`);
  }
  reviewNotes.push(
    params.parser === "hybrid_docai"
      ? "Parsed with AcroForm inputs grouped by Google Document AI table layout."
      : "Parsed with local AcroForm field extraction.",
  );

  const analysis: StoredAnalysis = {
    analysisId,
    accountId: params.accountId,
    templateName: params.templateName,
    templateType: params.templateType,
    source: params.source,
    extractedTextSnippet: params.parsedText.slice(0, 5000),
    extractedFieldCount: fields.length,
    confidenceScore,
    status,
    fields,
    formaSchema,
    reviewNotes,
    createdAt: new Date().toISOString(),
    pdfFilePath,
    sourceFileName: params.sourcePdf.fileName,
    sourceMimeType: params.sourcePdf.mimeType,
  };
  await writeJson(analysisMetaPath(analysisId), analysis);

  return {
    analysisId: analysis.analysisId,
    accountId: analysis.accountId,
    templateName: analysis.templateName,
    templateType: analysis.templateType,
    source: analysis.source,
    extractedTextSnippet: analysis.extractedTextSnippet,
    extractedFieldCount: analysis.extractedFieldCount,
    confidenceScore: analysis.confidenceScore,
    status: analysis.status,
    fields: analysis.fields,
    formaSchema: analysis.formaSchema,
    reviewNotes: analysis.reviewNotes,
    createdAt: analysis.createdAt,
  };
}

export async function getFormTemplateAnalysis(
  analysisId: string,
): Promise<FormBuilderAnalysisResult> {
  const stored = await readJson<StoredAnalysis>(analysisMetaPath(analysisId));
  return {
    analysisId: stored.analysisId,
    accountId: stored.accountId,
    templateName: stored.templateName,
    templateType: stored.templateType,
    source: stored.source,
    extractedTextSnippet: stored.extractedTextSnippet,
    extractedFieldCount: stored.extractedFieldCount,
    confidenceScore: stored.confidenceScore,
    status: stored.status,
    fields: stored.fields,
    formaSchema: stored.formaSchema ?? buildFormaSchema(stored.fields),
    reviewNotes: stored.reviewNotes,
    createdAt: stored.createdAt,
  };
}

export async function createFormTemplateFromAnalysis(params: {
  accessToken: string;
  analysisId: string;
  accountId?: string;
  hubId?: string;
  templateName?: string;
  templateType?: string;
  fieldOverrides?: FieldOverride[];
  /** Reviewer-defined order of kept field ids; omitted ids are removed. */
  orderedFieldIds?: string[];
  dryRun?: boolean;
}): Promise<FormBuilderCreateResult> {
  const stored = await readJson<StoredAnalysis>(analysisMetaPath(params.analysisId));
  const accountId = inferAccountId({
    accountId: params.accountId || stored.accountId,
    hubId: params.hubId,
  });
  const templateName = params.templateName?.trim() || stored.templateName;
  const templateType = normalizeTemplateType(params.templateType || stored.templateType);
  const patched = applyFieldOverrides(stored.fields, params.fieldOverrides);
  const overridden = applyFieldOrdering(patched, params.orderedFieldIds);
  const formaSchema = buildFormaSchema(overridden);

  if (overridden.length === 0) {
    return {
      success: false,
      dryRun: Boolean(params.dryRun),
      analysisId: stored.analysisId,
      accountId,
      templateName,
      templateType,
      fieldsSubmitted: 0,
      sectionsCreated: 0,
      tablesCreated: 0,
      calculatedColumns: 0,
      message: "Template creation blocked: no fields remain after review edits.",
    };
  }

  const hasReviewBlocks = overridden.some((f) => f.reviewState === "needs_review");
  if (hasReviewBlocks) {
    return {
      success: false,
      dryRun: Boolean(params.dryRun),
      analysisId: stored.analysisId,
      accountId,
      templateName,
      templateType,
      fieldsSubmitted: overridden.length,
      sectionsCreated: formaSchema.sectionCount,
      tablesCreated: formaSchema.tableCount,
      calculatedColumns: formaSchema.calculatedColumnCount,
      message:
        "Template creation blocked: one or more fields are still marked needs_review.",
    };
  }

  if (Boolean(params.dryRun)) {
    return {
      success: true,
      dryRun: true,
      analysisId: stored.analysisId,
      accountId,
      templateName,
      templateType,
      fieldsSubmitted: overridden.length,
      sectionsCreated: formaSchema.sectionCount,
      tablesCreated: formaSchema.tableCount,
      calculatedColumns: formaSchema.calculatedColumnCount,
      message: "Dry run complete. Template payload validated and ready to submit.",
    };
  }

  const created = await callCreateTemplateApi({
    accessToken: params.accessToken,
    accountId,
    templateName,
    templateType,
    fields: overridden,
    formaSchema,
    analysisId: stored.analysisId,
  });

  return {
    success: true,
    dryRun: false,
    analysisId: stored.analysisId,
    accountId,
    templateName,
    templateType,
    fieldsSubmitted: overridden.length,
    sectionsCreated: formaSchema.sectionCount,
    tablesCreated: formaSchema.tableCount,
    calculatedColumns: formaSchema.calculatedColumnCount,
    ...(created.templateId ? { templateId: created.templateId } : {}),
    response: created.response,
    message: "Form template created successfully.",
  };
}

