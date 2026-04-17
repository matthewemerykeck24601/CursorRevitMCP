import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { apsGet, toViewerUrn } from "@/lib/aps";
import { upsertHubReferenceFolders } from "@/lib/aps-admin";
import { getRequestId } from "@/lib/request";
import { log } from "@/lib/logger";

type ApsCollection<T> = {
  data: T[];
  links?: {
    next?: {
      href?: string;
    };
  };
};

type Folder = {
  id: string;
  type: "folders";
  attributes: { name?: string };
};

type Item = {
  id: string;
  type: "items";
  attributes: {
    displayName?: string;
    name?: string;
    extension?: { type?: string };
  };
  relationships?: {
    tip?: {
      data?: {
        id?: string;
      };
    };
  };
};

export const runtime = "nodejs";

function isQuotaLimitError(error: unknown): boolean {
  const message = error instanceof Error ? error.message : String(error ?? "");
  return message.includes("(429)") || message.toLowerCase().includes("quota limit exceeded");
}

function isFolder(entry: Folder | Item): entry is Folder {
  return entry.type === "folders";
}

function isItem(entry: Folder | Item): entry is Item {
  return entry.type === "items";
}

function isC4RModel(item: Item): boolean {
  const ext = (item.attributes.extension?.type ?? "").toLowerCase();
  const name = (
    item.attributes.displayName ??
    item.attributes.name ??
    ""
  ).toLowerCase();

  // Strong C4R/Revit cloud indicators across APS/BIM 360 variants.
  if (ext.includes("c4r") || ext.includes("revitcloudmodel")) {
    return true;
  }

  // Some hubs expose Revit cloud items with generic extensions.
  if (ext.includes("cloudmodel") && name.endsWith(".rvt")) {
    return true;
  }

  // BIM 360 file fallback.
  return ext.includes("bim360:file") && name.endsWith(".rvt");
}

function encodePathForAps(hrefOrPath: string): string {
  if (hrefOrPath.startsWith("http://") || hrefOrPath.startsWith("https://")) {
    const url = new URL(hrefOrPath);
    return `${url.pathname}${url.search}`;
  }
  return hrefOrPath;
}

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ projectId: string }> },
) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const { projectId } = await context.params;
  const hubId = request.nextUrl.searchParams.get("hubId");
  if (!hubId) {
    return NextResponse.json(
      { error: "Missing required query parameter: hubId", requestId },
      { status: 400 },
    );
  }

  const safeHubId = encodeURIComponent(hubId);
  const safeProjectId = encodeURIComponent(projectId);

  try {
    let topFolders: ApsCollection<Folder>;
    try {
      topFolders = await apsGet<ApsCollection<Folder>>(
        `/project/v1/hubs/${safeHubId}/projects/${safeProjectId}/topFolders`,
        auth.session.accessToken,
      );
    } catch (error) {
      if (isQuotaLimitError(error)) {
        const response = NextResponse.json({
          requestId,
          models: [],
          partial: true,
          warning: "APS quota limit exceeded while loading model folders. Showing partial data.",
        });
        for (const cookie of auth.response.cookies.getAll()) {
          response.cookies.set(cookie);
        }
        return response;
      }
      throw error;
    }

    const models: Array<{
      itemId: string;
      versionId: string;
      viewerUrn: string;
      name: string;
      sourceFolder: string;
      extensionType: string;
    }> = [];

    const folderQueue = topFolders.data.map((f) => ({
      id: f.id,
      name: f.attributes?.name ?? f.id,
    }));
    const discoveredFolders = new Map<string, string>(
      folderQueue.map((f) => [f.id, f.name]),
    );
    const visitedFolders = new Set<string>();
    let partial = false;
    let warning = "";

    while (folderQueue.length > 0) {
      const current = folderQueue.shift()!;
      if (visitedFolders.has(current.id)) continue;
      visitedFolders.add(current.id);

      let nextPath:
        | string
        | null = `/data/v1/projects/${safeProjectId}/folders/${encodeURIComponent(current.id)}/contents`;

      while (nextPath) {
        let contents: ApsCollection<Folder | Item>;
        try {
          contents = await apsGet<ApsCollection<Folder | Item>>(
            encodePathForAps(nextPath),
            auth.session.accessToken,
          );
        } catch (error) {
          if (isQuotaLimitError(error)) {
            partial = true;
            warning = "APS quota limit exceeded while traversing project files. Showing partial data.";
            folderQueue.length = 0;
            break;
          }
          throw error;
        }

        for (const entry of contents.data) {
          if (isFolder(entry)) {
            if (!visitedFolders.has(entry.id)) {
              const folderName = entry.attributes?.name ?? entry.id;
              discoveredFolders.set(entry.id, folderName);
              folderQueue.push({
                id: entry.id,
                name: folderName,
              });
            }
            continue;
          }
          if (!isItem(entry)) continue;
          if (!isC4RModel(entry)) continue;

          const tipId = entry.relationships?.tip?.data?.id;
          if (!tipId) continue;

          models.push({
            itemId: entry.id,
            versionId: tipId,
            viewerUrn: toViewerUrn(tipId),
            name: entry.attributes.displayName ?? entry.attributes.name ?? entry.id,
            sourceFolder: current.name,
            extensionType: entry.attributes.extension?.type ?? "unknown",
          });
        }

        nextPath = contents.links?.next?.href ?? null;
      }
    }

    try {
      await upsertHubReferenceFolders({
        hubId,
        projectId,
        folders: Array.from(discoveredFolders.entries()).map(([folderId, folderName]) => ({
          folderId,
          folderName,
        })),
      });
    } catch (error) {
      log("warn", "aps-models-folder-cache-upsert-failed", {
        requestId,
        hubId,
        projectId,
        details: error instanceof Error ? error.message : "Unknown error",
      });
    }

    models.sort((a, b) => a.name.localeCompare(b.name));

    const response = NextResponse.json({
      requestId,
      models,
      partial,
      ...(warning ? { warning } : {}),
    });
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    log("error", "aps-models-failed", {
      requestId,
      details: error instanceof Error ? error.message : "Unknown error",
    });
    return NextResponse.json(
      {
        error: "Failed to fetch models",
        requestId,
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 500 },
    );
  }
}

