import path from "node:path";
import { existsSync, mkdirSync } from "node:fs";
import xlsx from "xlsx";
import type { IParameterSource } from "./IParameterSource.js";
import type { InventorParameterRecord, ParameterSyncRequest } from "../types/contracts.js";

const SHEET_NAME = "parameters";

export class ExcelParameterSource implements IParameterSource {
  public readonly sourceName = "excel";

  public constructor(private readonly workbookPath: string) {}

  public async pull(request: ParameterSyncRequest): Promise<InventorParameterRecord[]> {
    this.ensureWorkbook();
    const workbook = xlsx.readFile(this.workbookPath);
    const sheet = workbook.Sheets[SHEET_NAME];
    if (!sheet) {
      return [];
    }

    const rows = xlsx.utils.sheet_to_json<Record<string, unknown>>(sheet);
    return rows
      .filter((r) => String(r.parameterSetId ?? "") === request.parameterSetId)
      .map((r) => ({
        name: String(r.name ?? "").trim(),
        units: this.cleanString(r.units),
        expression: this.cleanString(r.expression),
        value: this.toValue(r.value),
        isKey: typeof r.isKey === "boolean" ? r.isKey : undefined,
        comment: this.cleanString(r.comment),
      }))
      .filter((r) => r.name.length > 0);
  }

  public async push(request: ParameterSyncRequest): Promise<{ updated: number }> {
    this.ensureWorkbook();
    const workbook = xlsx.readFile(this.workbookPath);
    const rows = this.readRows(workbook).filter(
      (r) => String(r.parameterSetId ?? "") !== request.parameterSetId,
    );

    const now = new Date().toISOString();
    for (const p of request.parameters ?? []) {
      rows.push({
        parameterSetId: request.parameterSetId,
        documentName: request.documentName ?? "",
        assemblyName: request.assemblyName ?? "",
        name: p.name,
        units: p.units ?? "",
        expression: p.expression ?? "",
        value: p.value ?? "",
        isKey: p.isKey ?? false,
        comment: p.comment ?? "",
        updatedAtUtc: now,
      });
    }

    workbook.Sheets[SHEET_NAME] = xlsx.utils.json_to_sheet(rows);
    if (!workbook.SheetNames.includes(SHEET_NAME)) {
      workbook.SheetNames.push(SHEET_NAME);
    }
    xlsx.writeFile(workbook, this.workbookPath);
    return { updated: request.parameters?.length ?? 0 };
  }

  private readRows(workbook: xlsx.WorkBook): Record<string, unknown>[] {
    const sheet = workbook.Sheets[SHEET_NAME];
    if (!sheet) {
      return [];
    }
    return xlsx.utils.sheet_to_json<Record<string, unknown>>(sheet);
  }

  private ensureWorkbook(): void {
    const folder = path.dirname(this.workbookPath);
    if (!existsSync(folder)) {
      mkdirSync(folder, { recursive: true });
    }
    if (!existsSync(this.workbookPath)) {
      const wb = xlsx.utils.book_new();
      wb.SheetNames.push(SHEET_NAME);
      wb.Sheets[SHEET_NAME] = xlsx.utils.json_to_sheet([]);
      xlsx.writeFile(wb, this.workbookPath);
    }
  }

  private cleanString(value: unknown): string | undefined {
    const text = String(value ?? "").trim();
    return text.length > 0 ? text : undefined;
  }

  private toValue(value: unknown): string | number | boolean | undefined {
    if (typeof value === "string" || typeof value === "number" || typeof value === "boolean") {
      return value;
    }
    return undefined;
  }
}
