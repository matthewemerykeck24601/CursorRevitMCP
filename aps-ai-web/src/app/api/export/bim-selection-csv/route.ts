import { writeFile } from "node:fs/promises";
import path from "node:path";
import { NextResponse } from "next/server";

const FILE_NAME = "bim-selection-export.csv";

/**
 * Writes the same CSV the client downloads into the Next app root (process.cwd()),
 * e.g. aps-ai-web/bim-selection-export.csv — useful for local dev / automation.
 */
export async function POST(request: Request) {
  let body: unknown;
  try {
    body = await request.json();
  } catch {
    return NextResponse.json({ ok: false, error: "Invalid JSON" }, { status: 400 });
  }

  const csv =
    body &&
    typeof body === "object" &&
    "csv" in body &&
    typeof (body as { csv: unknown }).csv === "string"
      ? (body as { csv: string }).csv
      : null;

  if (csv == null) {
    return NextResponse.json(
      { ok: false, error: "Missing csv string in body" },
      { status: 400 },
    );
  }

  const filePath = path.join(process.cwd(), FILE_NAME);
  try {
    await writeFile(filePath, csv, "utf8");
    return NextResponse.json({
      ok: true,
      fileName: FILE_NAME,
      filePath,
    });
  } catch (e) {
    return NextResponse.json(
      {
        ok: false,
        error: e instanceof Error ? e.message : String(e),
      },
      { status: 500 },
    );
  }
}
