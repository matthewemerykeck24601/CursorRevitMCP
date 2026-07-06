import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { getRequestId } from "@/lib/request";
import { saveUploadedPdf } from "@/lib/acc-forms";

export const runtime = "nodejs";

export async function POST(request: NextRequest) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  let formData: FormData;
  try {
    formData = await request.formData();
  } catch {
    return NextResponse.json(
      { error: "Invalid multipart form data.", requestId },
      { status: 400 },
    );
  }

  const file = formData.get("file");
  if (!(file instanceof File)) {
    return NextResponse.json(
      { error: "file is required.", requestId },
      { status: 400 },
    );
  }

  const fileName = file.name?.trim() || "upload.pdf";
  const lowerName = fileName.toLowerCase();
  if (!lowerName.endsWith(".pdf")) {
    return NextResponse.json(
      { error: "Only PDF uploads are supported.", requestId },
      { status: 400 },
    );
  }

  const arrayBuffer = await file.arrayBuffer();
  const buffer = Buffer.from(arrayBuffer);
  if (buffer.byteLength === 0) {
    return NextResponse.json(
      { error: "Uploaded file is empty.", requestId },
      { status: 400 },
    );
  }

  const hubId = String(formData.get("hubId") ?? "").trim() || undefined;
  const projectId = String(formData.get("projectId") ?? "").trim() || undefined;

  try {
    const uploaded = await saveUploadedPdf({
      fileName,
      mimeType: file.type || "application/pdf",
      buffer,
      hubId,
      projectId,
    });
    const response = NextResponse.json({
      requestId,
      success: true,
      uploadToken: uploaded.uploadToken,
      fileName: uploaded.fileName,
      sizeBytes: uploaded.sizeBytes,
      createdAt: uploaded.createdAt,
    });
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    return NextResponse.json(
      {
        error: error instanceof Error ? error.message : "Upload failed.",
        requestId,
      },
      { status: 500 },
    );
  }
}

