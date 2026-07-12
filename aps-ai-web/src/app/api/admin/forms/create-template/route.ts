import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { getRequestId } from "@/lib/request";
import {
  createFormTemplateFromAnalysis,
  type FieldOverride,
} from "@/lib/acc-forms";

export const runtime = "nodejs";

type CreateTemplateBody = {
  analysisId?: unknown;
  accountId?: unknown;
  hubId?: unknown;
  templateName?: unknown;
  templateType?: unknown;
  dryRun?: unknown;
  fieldOverrides?: unknown;
  orderedFieldIds?: unknown;
};

function parseOverrides(raw: unknown): FieldOverride[] {
  if (!Array.isArray(raw)) return [];
  return raw
    .map((item) => {
      if (!item || typeof item !== "object" || Array.isArray(item)) return null;
      const row = item as Record<string, unknown>;
      const id = String(row.id ?? "").trim();
      if (!id) return null;
      const label = String(row.label ?? "").trim();
      const type = String(row.type ?? "").trim();
      const groupKey = String(row.groupKey ?? "").trim();
      const formula = String(row.formula ?? "").trim();
      const required =
        typeof row.required === "boolean" ? row.required : undefined;
      const calculated =
        typeof row.calculated === "boolean" ? row.calculated : undefined;
      const options = Array.isArray(row.options)
        ? row.options.map((v) => String(v)).filter(Boolean)
        : undefined;
      return {
        id,
        ...(label ? { label } : {}),
        ...(type ? { type: type as FieldOverride["type"] } : {}),
        ...(typeof required === "boolean" ? { required } : {}),
        ...(typeof calculated === "boolean" ? { calculated } : {}),
        ...(formula ? { formula } : {}),
        ...(groupKey ? { groupKey } : {}),
        ...(options ? { options } : {}),
      };
    })
    .filter((row): row is FieldOverride => row != null);
}

function parseOrderedFieldIds(raw: unknown): string[] | undefined {
  if (!Array.isArray(raw)) return undefined;
  const ids = raw.map((v) => String(v).trim()).filter(Boolean);
  return ids.length > 0 ? ids : undefined;
}

export async function POST(request: NextRequest) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  let body: CreateTemplateBody;
  try {
    body = (await request.json()) as CreateTemplateBody;
  } catch {
    return NextResponse.json(
      { error: "Invalid JSON body.", requestId },
      { status: 400 },
    );
  }

  const analysisId = String(body.analysisId ?? "").trim();
  if (!analysisId) {
    return NextResponse.json(
      { error: "analysisId is required.", requestId },
      { status: 400 },
    );
  }

  const accountId = String(body.accountId ?? "").trim() || undefined;
  const hubId = String(body.hubId ?? "").trim() || undefined;
  const templateName = String(body.templateName ?? "").trim() || undefined;
  const templateType = String(body.templateType ?? "").trim() || undefined;
  const dryRun = Boolean(body.dryRun);
  const fieldOverrides = parseOverrides(body.fieldOverrides);
  const orderedFieldIds = parseOrderedFieldIds(body.orderedFieldIds);

  try {
    const result = await createFormTemplateFromAnalysis({
      accessToken: auth.session.accessToken,
      analysisId,
      accountId,
      hubId,
      templateName,
      templateType,
      dryRun,
      fieldOverrides,
      orderedFieldIds,
    });
    const status = result.success ? 200 : 409;
    const response = NextResponse.json(
      {
        requestId,
        ...result,
      },
      { status },
    );
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    return NextResponse.json(
      {
        error: error instanceof Error ? error.message : "Template creation failed.",
        requestId,
      },
      { status: 400 },
    );
  }
}

