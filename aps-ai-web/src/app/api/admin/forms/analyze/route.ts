import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { getRequestId } from "@/lib/request";
import {
  analyzePdfForFormTemplate,
  type FormBuilderSource,
  type FormTemplateType,
} from "@/lib/acc-forms";

export const runtime = "nodejs";

type AnalyzeBody = {
  source?: unknown;
  templateName?: unknown;
  templateType?: unknown;
  accountId?: unknown;
  hubId?: unknown;
};

function parseSource(raw: unknown): FormBuilderSource | null {
  if (!raw || typeof raw !== "object" || Array.isArray(raw)) return null;
  const source = raw as Record<string, unknown>;
  const kind = String(source.kind ?? "").trim();
  if (kind === "upload_token") {
    const uploadToken = String(source.uploadToken ?? "").trim();
    return uploadToken ? { kind, uploadToken } : null;
  }
  if (kind === "acc_version") {
    const projectId = String(source.projectId ?? "").trim();
    const versionId = String(source.versionId ?? "").trim();
    return projectId && versionId ? { kind, projectId, versionId } : null;
  }
  if (kind === "pdf_base64") {
    const base64 = String(source.base64 ?? "").trim();
    const fileName = String(source.fileName ?? "").trim();
    return base64 ? { kind, base64, ...(fileName ? { fileName } : {}) } : null;
  }
  return null;
}

export async function POST(request: NextRequest) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  let body: AnalyzeBody;
  try {
    body = (await request.json()) as AnalyzeBody;
  } catch {
    return NextResponse.json(
      { error: "Invalid JSON body.", requestId },
      { status: 400 },
    );
  }

  const source = parseSource(body.source);
  if (!source) {
    return NextResponse.json(
      { error: "A valid source payload is required.", requestId },
      { status: 400 },
    );
  }

  const templateName = String(body.templateName ?? "").trim() || undefined;
  const templateType = String(body.templateType ?? "").trim() as FormTemplateType;
  const accountId = String(body.accountId ?? "").trim() || undefined;
  const hubId = String(body.hubId ?? "").trim() || undefined;

  try {
    const result = await analyzePdfForFormTemplate({
      accessToken: auth.session.accessToken,
      source,
      templateName,
      templateType,
      accountId,
      hubId,
    });
    const response = NextResponse.json({
      requestId,
      success: true,
      analysis: result,
    });
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    return NextResponse.json(
      {
        error: error instanceof Error ? error.message : "Analyze request failed.",
        requestId,
      },
      { status: 400 },
    );
  }
}

