import { NextRequest, NextResponse } from "next/server";
import fs from "node:fs/promises";
import path from "node:path";
import { requireSession } from "@/lib/auth-guard";
import {
  fetchProjectObjectJson,
  getDesignFilesBucketKey,
  listProjectObjects,
  uploadProjectJsonObject,
} from "@/lib/aps-oss";

export const runtime = "nodejs";

function sanitizeFileName(name: string): string {
  return name
    .trim()
    .replace(/[^a-zA-Z0-9._-]+/g, "_")
    .replace(/^_+|_+$/g, "")
    .slice(0, 80);
}

function sanitizeSegment(value: string): string {
  return value
    .trim()
    .replace(/[^a-zA-Z0-9._-]+/g, "_")
    .replace(/^_+|_+$/g, "")
    .slice(0, 80);
}

function buildPrefix(projectNumber: string, hubId: string, projectId: string): string {
  const safeProjectNumber = sanitizeSegment(projectNumber) || "UnknownProject";
  const safeHubId = sanitizeSegment(hubId) || "UnknownHub";
  const safeProjectId = sanitizeSegment(projectId) || "UnknownProjectId";
  return `${safeHubId}/${safeProjectId}/${safeProjectNumber}/ProductDesignCalcs/`;
}

function buildLegacyPrefix(projectNumber: string): string {
  const safeProjectNumber = sanitizeSegment(projectNumber) || "UnknownProject";
  return `${safeProjectNumber}/ProductDesignCalcs/`;
}

function parseVersionedName(name: string): { baseName: string; version: number } {
  const clean = name.replace(/\.json$/i, "");
  const match = clean.match(/^(.*)__v(\d+)$/i);
  if (!match) {
    return { baseName: clean, version: 1 };
  }
  const parsed = Number.parseInt(match[2] ?? "1", 10);
  return {
    baseName: (match[1] ?? clean).trim() || clean,
    version: Number.isFinite(parsed) && parsed > 0 ? parsed : 1,
  };
}

function formatVersion(version: number): string {
  return `v${String(Math.max(1, version)).padStart(4, "0")}`;
}

function buildVersionedFileName(baseName: string, version: number): string {
  return `${baseName}__${formatVersion(version)}`;
}

function stripVersionSuffix(fileName: string): string {
  return fileName.replace(/__v\d+$/i, "");
}

function getLocalRoot(): string {
  return path.join(process.cwd(), ".design-files");
}

async function ensureLocalFolder(prefix: string): Promise<string> {
  const folder = path.join(getLocalRoot(), prefix);
  await fs.mkdir(folder, { recursive: true });
  return folder;
}

async function listLocalFiles(
  prefix: string,
): Promise<Array<{ objectKey: string; name: string; source: "local-fallback" }>> {
  try {
    const folder = await ensureLocalFolder(prefix);
    const entries = await fs.readdir(folder, { withFileTypes: true });
    return entries
      .filter((e) => e.isFile() && e.name.toLowerCase().endsWith(".json"))
      .map((e) => ({
        objectKey: `${prefix}${e.name}`,
        name: e.name.replace(/\.json$/i, ""),
        source: "local-fallback" as const,
      }));
  } catch {
    return [];
  }
}

async function readLocalJson(prefix: string, objectKey: string): Promise<unknown> {
  const relative = objectKey.startsWith(prefix) ? objectKey.slice(prefix.length) : objectKey;
  const fileName = path.basename(relative);
  const folder = await ensureLocalFolder(prefix);
  const fullPath = path.join(folder, fileName);
  const raw = await fs.readFile(fullPath, "utf8");
  return JSON.parse(raw) as unknown;
}

async function writeLocalJson(prefix: string, fileName: string, data: unknown): Promise<string> {
  const safeName = sanitizeFileName(fileName);
  const folder = await ensureLocalFolder(prefix);
  const fullPath = path.join(folder, `${safeName}.json`);
  await fs.writeFile(fullPath, JSON.stringify(data, null, 2), "utf8");
  return `${prefix}${safeName}.json`;
}

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ projectId: string }> },
) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;
  const { projectId } = await context.params;
  const bucket = getDesignFilesBucketKey();

  const objectKey = request.nextUrl.searchParams.get("objectKey");
  const projectNumber =
    request.nextUrl.searchParams.get("projectNumber") ?? projectId;
  const hubId = request.nextUrl.searchParams.get("hubId") ?? "";
  const diagnostics: string[] = [];
  try {
    if (objectKey) {
      let data: unknown;
      let storage = "oss";
      try {
        data = await fetchProjectObjectJson<unknown>(bucket, objectKey);
      } catch {
        const scopedPrefix = buildPrefix(projectNumber, hubId, projectId);
        const legacyPrefix = buildLegacyPrefix(projectNumber);
        const relative = objectKey
          .replace(scopedPrefix, "")
          .replace(legacyPrefix, "")
          .replace(/\.json$/i, "");
        const nonVersionedRelative = stripVersionSuffix(relative);
        try {
          data = await readLocalJson(scopedPrefix, objectKey);
        } catch {
          try {
            data = await readLocalJson(legacyPrefix, objectKey);
          } catch {
            // Backward compatibility: older local fallback writes might have
            // dropped the __vNNNN suffix; try loading that variant too.
            const scopedLegacyKey = `${scopedPrefix}${nonVersionedRelative}.json`;
            try {
              data = await readLocalJson(scopedPrefix, scopedLegacyKey);
            } catch {
              const legacyKey = `${legacyPrefix}${nonVersionedRelative}.json`;
              data = await readLocalJson(legacyPrefix, legacyKey);
            }
          }
        }
        storage = "local-fallback";
      }
      const response = NextResponse.json({ objectKey, data, storage });
      response.headers.set("x-design-file-storage", storage);
      for (const cookie of auth.response.cookies.getAll()) {
        response.cookies.set(cookie);
      }
      return response;
    }

    const prefix = buildPrefix(projectNumber, hubId, projectId);
    const legacyPrefix = buildLegacyPrefix(projectNumber);
    const files: Array<{
      objectKey: string;
      name: string;
      baseName: string;
      version: number;
      isLatest: boolean;
      source: "oss" | "local-fallback";
    }> = [];
    let storage: "oss" | "local-fallback" = "oss";
    let ossCount = 0;
    let localCount = 0;
    try {
      const objectKeys = [
        ...(await listProjectObjects(bucket, prefix)),
        ...(await listProjectObjects(bucket, legacyPrefix)),
      ];
      for (const key of objectKeys) {
        const normalizedPrefix = key.startsWith(prefix) ? prefix : legacyPrefix;
        files.push({
          objectKey: key,
          name: key.slice(normalizedPrefix.length).replace(/\.json$/i, ""),
          baseName: "",
          version: 1,
          isLatest: false,
          source: "oss",
        });
      }
      ossCount = objectKeys.length;
    } catch (error) {
      diagnostics.push(
        `OSS list failed: ${error instanceof Error ? error.message : "unknown error"}`,
      );
      storage = "local-fallback";
    }
    const localFiles = [
      ...(await listLocalFiles(prefix)),
      ...(await listLocalFiles(legacyPrefix)),
    ];
    localCount = localFiles.length;
    files.push(...localFiles);

    const dedup = new Map<
      string,
      {
        objectKey: string;
        name: string;
        baseName: string;
        version: number;
        isLatest: boolean;
        source: "oss" | "local-fallback";
      }
    >();
    for (const file of files) {
      const existing = dedup.get(file.objectKey);
      // Prefer OSS source when the same key exists in both locations.
      if (!existing || (existing.source === "local-fallback" && file.source === "oss")) {
        dedup.set(file.objectKey, file);
      }
    }
    const mergedWithVersion = Array.from(dedup.values()).map((file) => {
      const parsed = parseVersionedName(file.name);
      return {
        ...file,
        baseName: parsed.baseName,
        version: parsed.version,
        name: parsed.baseName,
        isLatest: false,
      };
    });
    const latestByBase = new Map<string, number>();
    for (const file of mergedWithVersion) {
      const current = latestByBase.get(file.baseName) ?? 0;
      if (file.version > current) {
        latestByBase.set(file.baseName, file.version);
      }
    }
    const mergedFiles = mergedWithVersion
      .map((file) => ({
        ...file,
        isLatest: (latestByBase.get(file.baseName) ?? 0) === file.version,
      }))
      .sort((a, b) => {
        const byName = a.baseName.localeCompare(b.baseName, undefined, {
          numeric: true,
          sensitivity: "base",
        });
        if (byName !== 0) return byName;
        return b.version - a.version;
      });
    const response = NextResponse.json({
      projectId,
      projectNumber,
      bucket,
      prefix,
      files: mergedFiles,
      count: mergedFiles.length,
      storage,
      ossCount,
      localCount,
      diagnostics,
    });
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    return NextResponse.json(
      {
        error: "Failed to load design files",
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 500 },
    );
  }
}

export async function POST(
  request: NextRequest,
  context: { params: Promise<{ projectId: string }> },
) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;
  const { projectId } = await context.params;
  const bucket = getDesignFilesBucketKey();

  const body = (await request.json()) as {
    fileName?: string;
    projectNumber?: string;
    hubId?: string;
    createNewVersion?: boolean;
    data?: unknown;
  };
  const baseName = sanitizeFileName(body.fileName ?? "");
  if (!baseName || !body.data) {
    return NextResponse.json(
      { error: "Missing required fields: fileName and data" },
      { status: 400 },
    );
  }

  const projectNumber = body.projectNumber ?? projectId;
  const hubId = body.hubId ?? "";
  const prefix = buildPrefix(projectNumber, hubId, projectId);
  const legacyPrefix = buildLegacyPrefix(projectNumber);
  const diagnostics: string[] = [];
  try {
    let nextVersion = 1;
    if (body.createNewVersion !== false) {
      try {
        const existingKeys = [
          ...(await listProjectObjects(bucket, prefix)),
          ...(await listProjectObjects(bucket, legacyPrefix)),
        ];
        let maxVersion = 0;
        for (const key of existingKeys) {
          const relative = key.startsWith(prefix)
            ? key.slice(prefix.length)
            : key.startsWith(legacyPrefix)
              ? key.slice(legacyPrefix.length)
              : key;
          const parsed = parseVersionedName(relative);
          if (parsed.baseName === baseName) {
            maxVersion = Math.max(maxVersion, parsed.version);
          }
        }
        nextVersion = Math.max(1, maxVersion + 1);
      } catch (error) {
        diagnostics.push(
          `Version scan failed, defaulting to v0001: ${
            error instanceof Error ? error.message : "unknown error"
          }`,
        );
      }
    }
    const versionedName = buildVersionedFileName(baseName, nextVersion);
    const objectKey = `${prefix}${versionedName}.json`;
    let storage = "oss";
    try {
      await uploadProjectJsonObject(bucket, objectKey, body.data);
    } catch (error) {
      diagnostics.push(
        `OSS save failed: ${error instanceof Error ? error.message : "unknown error"}`,
      );
      await writeLocalJson(prefix, versionedName, body.data);
      storage = "local-fallback";
    }
    const response = NextResponse.json({
      projectId,
      projectNumber,
      bucket,
      prefix,
      objectKey,
      name: baseName,
      baseName,
      version: nextVersion,
      isLatest: true,
      storage,
      diagnostics,
    });
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    return NextResponse.json(
      {
        error: "Failed to save design file",
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 500 },
    );
  }
}
