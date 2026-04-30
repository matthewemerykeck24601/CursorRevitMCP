import { NextRequest, NextResponse } from "next/server";
import { getNativeAuthPublicConfig } from "@/lib/aps";
import { getRequestId } from "@/lib/request";

export const runtime = "nodejs";

/** Public config for Monty iOS native OAuth (no secrets). */
export async function GET(request: NextRequest) {
  const requestId = getRequestId(request);
  try {
    const cfg = getNativeAuthPublicConfig();
    return NextResponse.json({ requestId, ...cfg });
  } catch (error) {
    return NextResponse.json(
      {
        error:
          error instanceof Error
            ? error.message
            : "APS credentials not configured",
        requestId,
      },
      { status: 500 },
    );
  }
}
