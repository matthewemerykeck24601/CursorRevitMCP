import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { apsGet } from "@/lib/aps";
import { getRequestId } from "@/lib/request";
import { log } from "@/lib/logger";

type ApsCollection<T> = {
  data: T[];
};

type Hub = {
  id: string;
  attributes: {
    name?: string;
    extension?: {
      type?: string;
    };
  };
};

export const runtime = "nodejs";

export async function GET(request: NextRequest) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  try {
    const hubs = await apsGet<ApsCollection<Hub>>(
      "/project/v1/hubs",
      auth.session.accessToken,
    );

    const response = NextResponse.json({
      requestId,
      hubs: hubs.data.map((hub) => ({
        id: hub.id,
        name: hub.attributes?.name ?? hub.id,
        type: hub.attributes?.extension?.type ?? "unknown",
      })),
    });

    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    log("error", "aps-hubs-failed", {
      requestId,
      details: error instanceof Error ? error.message : "Unknown error",
    });
    return NextResponse.json(
      {
        error: "Failed to fetch hubs",
        requestId,
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 500 },
    );
  }
}

