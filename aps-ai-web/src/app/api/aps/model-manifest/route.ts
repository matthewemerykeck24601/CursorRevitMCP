import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";

export const runtime = "nodejs";

const APS_BASE = "https://developer.api.autodesk.com";

export async function GET(request: NextRequest) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const urn = request.nextUrl.searchParams.get("urn");
  if (!urn) {
    return NextResponse.json(
      { error: "Missing required query parameter: urn" },
      { status: 400 },
    );
  }

  const response = await fetch(
    `${APS_BASE}/modelderivative/v2/designdata/${encodeURIComponent(urn)}/manifest`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${auth.session.accessToken}`,
        "Content-Type": "application/json",
      },
      cache: "no-store",
    },
  );

  if (!response.ok) {
    const body = await response.text();
    const next = NextResponse.json(
      {
        ok: false,
        status: response.status,
        error:
          response.status === 404
            ? "Model derivative manifest not found. The model may not be translated yet."
            : "Failed to fetch model derivative manifest.",
        details: body,
      },
      { status: response.status },
    );
    for (const cookie of auth.response.cookies.getAll()) {
      next.cookies.set(cookie);
    }
    return next;
  }

  const manifest = (await response.json()) as {
    status?: string;
    progress?: string;
  };

  const next = NextResponse.json({
    ok: true,
    status: manifest.status ?? "unknown",
    progress: manifest.progress ?? "unknown",
  });
  for (const cookie of auth.response.cookies.getAll()) {
    next.cookies.set(cookie);
  }
  return next;
}
