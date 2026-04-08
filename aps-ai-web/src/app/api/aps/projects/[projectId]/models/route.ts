import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { apsGet, toViewerUrn } from "@/lib/aps";
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
    const topFolders = await apsGet<ApsCollection<Folder>>(
      `/project/v1/hubs/${safeHubId}/projects/${safeProjectId}/topFolders`,
      auth.session.accessToken,
    );

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
    const visitedFolders = new Set<string>();

    while (folderQueue.length > 0) {
      const current = folderQueue.shift()!;
      if (visitedFolders.has(current.id)) continue;
      visitedFolders.add(current.id);

      let nextPath:
        | string
        | null = `/data/v1/projects/${safeProjectId}/folders/${encodeURIComponent(current.id)}/contents`;

      while (nextPath) {
        const contents: ApsCollection<Folder | Item> =
          await apsGet<ApsCollection<Folder | Item>>(
          encodePathForAps(nextPath),
          auth.session.accessToken,
        );

        for (const entry of contents.data) {
          if (isFolder(entry)) {
            if (!visitedFolders.has(entry.id)) {
              folderQueue.push({
                id: entry.id,
                name: entry.attributes?.name ?? entry.id,
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

    models.sort((a, b) => a.name.localeCompare(b.name));

    const response = NextResponse.json({ requestId, models });
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

