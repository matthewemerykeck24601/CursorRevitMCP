import test from "node:test";
import assert from "node:assert/strict";
import { PDFDocument } from "pdf-lib";
import {
  analyzePdfForFormTemplate,
  buildFormaSchema,
  createFormTemplateFromAnalysis,
  parseFieldNameHierarchy,
  type ParsedFormField,
} from "@/lib/acc-forms";

function makeField(overrides: Partial<ParsedFormField>): ParsedFormField {
  return {
    id: overrides.id ?? "f",
    sourceName: overrides.sourceName ?? "f",
    label: overrides.label ?? "F",
    type: overrides.type ?? "text",
    required: overrides.required ?? false,
    options: overrides.options ?? [],
    confidence: overrides.confidence ?? 0.95,
    reviewState: overrides.reviewState ?? "auto",
    notes: overrides.notes ?? [],
    ...overrides,
  };
}

async function createSamplePdfBase64(): Promise<string> {
  const doc = await PDFDocument.create();
  const page = doc.addPage([612, 792]);
  const form = doc.getForm();

  const inspector = form.createTextField("inspector_name");
  inspector.addToPage(page, {
    x: 48,
    y: 700,
    width: 220,
    height: 22,
  });
  const approved = form.createCheckBox("approved");
  approved.addToPage(page, {
    x: 48,
    y: 660,
    width: 16,
    height: 16,
  });
  const bytes = await doc.save();
  return Buffer.from(bytes).toString("base64");
}

test("analyze_pdf_for_form_template parses acroform fields", async () => {
  const base64 = await createSamplePdfBase64();
  const result = await analyzePdfForFormTemplate({
    accessToken: "test-token",
    accountId: "test-account",
    templateName: "QA Walkthrough",
    templateType: "quality",
    source: {
      kind: "pdf_base64",
      base64,
      fileName: "qa.pdf",
    },
  });

  assert.equal(result.templateType, "quality");
  assert.equal(result.templateName, "QA Walkthrough");
  assert.ok(result.extractedFieldCount >= 2);
  assert.ok(result.fields.some((field) => field.type === "text"));
  assert.ok(result.fields.some((field) => field.type === "checkbox"));
});

test("create_form_template_from_pdf blocks unresolved review rows", async () => {
  const base64 = await createSamplePdfBase64();
  const analysis = await analyzePdfForFormTemplate({
    accessToken: "test-token",
    accountId: "test-account",
    templateName: "Safety Checklist",
    templateType: "safety",
    source: {
      kind: "pdf_base64",
      base64,
      fileName: "safety.pdf",
    },
  });

  const firstField = analysis.fields[0];
  assert.ok(firstField);

  const blocked = await createFormTemplateFromAnalysis({
    accessToken: "test-token",
    analysisId: analysis.analysisId,
    accountId: analysis.accountId,
    dryRun: true,
    fieldOverrides: [{ id: firstField.id, reviewState: "needs_review" }],
  });
  assert.equal(blocked.success, false);
  assert.match(blocked.message, /blocked/i);

  const dryRunOk = await createFormTemplateFromAnalysis({
    accessToken: "test-token",
    analysisId: analysis.analysisId,
    accountId: analysis.accountId,
    dryRun: true,
    fieldOverrides: [{ id: firstField.id, reviewState: "approved" }],
  });
  assert.equal(dryRunOk.success, true);
  assert.equal(dryRunOk.dryRun, true);
});

test("parseFieldNameHierarchy detects group/row/column", () => {
  assert.deepEqual(parseFieldNameHierarchy("Inspection.Row2.Result"), {
    groupKey: "Inspection",
    columnKey: "Result",
    rowIndex: 2,
  });
  assert.deepEqual(parseFieldNameHierarchy("items[0].qty"), {
    groupKey: "items",
    columnKey: "qty",
    rowIndex: 0,
  });
  assert.deepEqual(parseFieldNameHierarchy("single"), {});
});

test("buildFormaSchema maps repeating rows to multiple-entries section", () => {
  const fields: ParsedFormField[] = [
    makeField({
      id: "punch_head",
      label: "Punch List",
      type: "section_heading",
      groupKey: "punch",
    }),
    makeField({ id: "punch_0_item", label: "Item", groupKey: "punch", columnKey: "item", rowIndex: 0 }),
    makeField({ id: "punch_1_item", label: "Item", groupKey: "punch", columnKey: "item", rowIndex: 1 }),
  ];
  const schema = buildFormaSchema(fields);
  // Repeated row indices => table per translator precedence.
  assert.equal(schema.tableCount, 1);
  assert.equal(schema.elements[0]?.kind, "table");
});

test("buildFormaSchema flags calculated columns in a table", () => {
  const fields: ParsedFormField[] = [
    makeField({ id: "row_qty", label: "Qty", type: "number", groupKey: "lineitem", columnKey: "qty" }),
    makeField({ id: "row_price", label: "Price", type: "number", groupKey: "lineitem", columnKey: "price" }),
    makeField({
      id: "row_total",
      label: "Total",
      type: "number",
      groupKey: "lineitem",
      columnKey: "total",
      calculated: true,
      formula: "AFSimple_Calculate",
    }),
  ];
  const schema = buildFormaSchema(fields);
  assert.equal(schema.tableCount, 1);
  assert.equal(schema.calculatedColumnCount, 1);
  const table = schema.elements.find((e) => e.kind === "table");
  assert.ok(table && table.kind === "table");
  assert.ok(table.columns.some((c) => c.calculated));
});

test("buildFormaSchema keeps standalone fields as field elements", () => {
  const fields: ParsedFormField[] = [
    makeField({ id: "name", label: "Name" }),
    makeField({ id: "date", label: "Date", type: "date" }),
  ];
  const schema = buildFormaSchema(fields);
  assert.equal(schema.sectionCount, 0);
  assert.equal(schema.tableCount, 0);
  assert.equal(schema.elements.length, 2);
  assert.ok(schema.elements.every((e) => e.kind === "field"));
});

