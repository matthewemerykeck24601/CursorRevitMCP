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

type FolderEntry = {
  id: string;
  type: "folders";
  attributes?: { name?: string };
};

type ItemEntry = {
  id: string;
  type: "items";
  attributes?: {
    displayName?: string;
    name?: string;
    extension?: { type?: string };
  };
  relationships?: {
    tip?: {
      data?: { id?: string };
    };
  };
};

type BrowserNode = {
  id: string;
  name: string;
  kind: "folder" | "file";
  parentId: string | null;
  itemId?: string;
  extensionType?: string;
  versionId?: string;
  viewerUrn?: string;
  children: BrowserNode[];
  childrenLoaded?: boolean;
};

export const runtime = "nodejs";

function isQuotaLimitError(error: unknown): boolean {
  const message = error instanceof Error ? error.message : String(error ?? "");
  return message.includes("(429)") || message.toLowerCase().includes("quota limit exceeded");
}

function encodePathForAps(hrefOrPath: string): string {
  if (hrefOrPath.startsWith("http://") || hrefOrPath.startsWith("https://")) {
    const url = new URL(hrefOrPath);
    return `${url.pathname}${url.search}`;
  }
  return hrefOrPath;
}

function itemDisplayName(item: ItemEntry): string {
  return item.attributes?.displayName ?? item.attributes?.name ?? item.id;
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

  const folderId = request.nextUrl.searchParams.get("folderId")?.trim() ?? "";
  const safeHubId = encodeURIComponent(hubId);
  const safeProjectId = encodeURIComponent(projectId);
  const safeFolderId = folderId ? encodeURIComponent(folderId) : "";
  let partial = false;
  let warning = "";

  try {
    const nodes: BrowserNode[] = [];
    if (!folderId) {
      let topFolders: ApsCollection<FolderEntry>;
      try {
        topFolders = await apsGet<ApsCollection<FolderEntry>>(
          `/project/v1/hubs/${safeHubId}/projects/${safeProjectId}/topFolders`,
          auth.session.accessToken,
        );
      } catch (error) {
        if (isQuotaLimitError(error)) {
          partial = true;
          warning = "APS quota limit exceeded while loading top folders. Showing partial data.";
          topFolders = { data: [] };
        } else {
          throw error;
        }
      }

      for (const folder of topFolders.data) {
        nodes.push({
          id: folder.id,
          name: folder.attributes?.name ?? folder.id,
          kind: "folder",
          parentId: null,
          children: [],
          childrenLoaded: false,
        });
      }
    } else {
      let nextPath: string | null = `/data/v1/projects/${safeProjectId}/folders/${safeFolderId}/contents`;
      while (nextPath) {
        let page: ApsCollection<FolderEntry | ItemEntry>;
        try {
          page = await apsGet<ApsCollection<FolderEntry | ItemEntry>>(
            encodePathForAps(nextPath),
            auth.session.accessToken,
          );
        } catch (error) {
          if (isQuotaLimitError(error)) {
            partial = true;
            warning = "APS quota limit exceeded while loading this folder. Showing partial data.";
            break;
          }
          throw error;
        }

        for (const entry of page.data) {
          if (entry.type === "folders") {
            nodes.push({
              id: entry.id,
              name: entry.attributes?.name ?? entry.id,
              kind: "folder",
              parentId: folderId,
              children: [],
              childrenLoaded: false,
            });
            continue;
          }
          const tipId = entry.relationships?.tip?.data?.id ?? "";
          nodes.push({
            id: `${folderId}:${entry.id}`,
            name: itemDisplayName(entry),
            kind: "file",
            parentId: folderId,
            itemId: entry.id,
            extensionType: entry.attributes?.extension?.type ?? "unknown",
            ...(tipId ? { versionId: tipId } : {}),
            ...(tipId ? { viewerUrn: toViewerUrn(tipId) } : {}),
            children: [],
          });
        }
        nextPath = page.links?.next?.href ?? null;
      }
    }

    nodes.sort((a, b) => {
      if (a.kind !== b.kind) return a.kind === "folder" ? -1 : 1;
      return a.name.localeCompare(b.name, undefined, { sensitivity: "base" });
    });

    const folders = nodes
      .filter((node) => node.kind === "folder")
      .map((node) => ({ folderId: node.id, folderName: node.name }));
    if (folders.length > 0) {
      try {
        await upsertHubReferenceFolders({
          hubId,
          projectId,
          folders,
        });
      } catch (cacheError) {
        log("warn", "aps-project-folders-cache-upsert-failed", {
          requestId,
          hubId,
          projectId,
          details: cacheError instanceof Error ? cacheError.message : "Unknown cache upsert error",
        });
      }
    }

    const response = NextResponse.json({
      requestId,
      hubId,
      projectId,
      nodes,
      scope: folderId ? "folder" : "top",
      parentFolderId: folderId || null,
      folderCount: nodes.filter((node) => node.kind === "folder").length,
      fileCount: nodes.filter((node) => node.kind === "file").length,
      partial,
      ...(warning ? { warning } : {}),
    });
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    log("error", "aps-project-folders-failed", {
      requestId,
      hubId,
      projectId,
      folderId: folderId || null,
      details: error instanceof Error ? error.message : "Unknown error",
    });
    return NextResponse.json(
      {
        error: "Failed to fetch project folder tree",
        requestId,
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 500 },
    );
  }
}
