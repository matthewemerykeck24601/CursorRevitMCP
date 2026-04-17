import { env } from "@/lib/env";

function detectRevitVersionMajorFromCloudModel(
  cloudModel: unknown,
): number | undefined {
  if (!cloudModel || typeof cloudModel !== "object" || Array.isArray(cloudModel)) {
    return undefined;
  }
  const fromCloud = Number(
    (cloudModel as { revitVersionMajor?: unknown }).revitVersionMajor,
  );
  if (!Number.isFinite(fromCloud)) return undefined;
  return Math.trunc(fromCloud);
}

export function detectRevitVersionMajorFromWorkitemArgs(
  workitemArguments: Record<string, unknown>,
): number | undefined {
  const direct = Number(workitemArguments.revitVersionMajor);
  if (Number.isFinite(direct)) return Math.trunc(direct);
  return detectRevitVersionMajorFromCloudModel(workitemArguments.cloud_model);
}

export function resolveDaActivityIdFromWorkitemArgs(
  workitemArguments: Record<string, unknown>,
): { activityId: string; source: string; revitVersionMajor?: number } {
  const explicit = String(workitemArguments.activityId ?? "").trim();
  if (explicit) {
    return { activityId: explicit, source: "workitemArguments.activityId" };
  }

  const revitVersionMajor = detectRevitVersionMajorFromWorkitemArgs(workitemArguments);
  if (revitVersionMajor) {
    if (revitVersionMajor >= 2027 && env.daActivityIdNet10) {
      return {
        activityId: env.daActivityIdNet10,
        source: "DA_ACTIVITY_ID_NET10",
        revitVersionMajor,
      };
    }
    if (revitVersionMajor >= 2025 && env.daActivityIdNet8) {
      return {
        activityId: env.daActivityIdNet8,
        source: "DA_ACTIVITY_ID_NET8",
        revitVersionMajor,
      };
    }
    const byYear =
      revitVersionMajor >= 2027
        ? env.daActivityId2027
        : revitVersionMajor === 2026
          ? env.daActivityId2026
          : revitVersionMajor === 2025
            ? env.daActivityId2025
            : revitVersionMajor === 2024
              ? env.daActivityId2024
              : "";
    if (byYear) {
      return {
        activityId: byYear,
        source: `DA_ACTIVITY_ID_${revitVersionMajor}`,
        revitVersionMajor,
      };
    }
  }

  if (env.daActivityId) {
    return {
      activityId: env.daActivityId,
      source: "DA_ACTIVITY_ID",
      ...(revitVersionMajor ? { revitVersionMajor } : {}),
    };
  }
  throw new Error(
    "No Design Automation activity configured. Set DA_ACTIVITY_ID, or set DA_ACTIVITY_ID_2024/2025/2026/2027 (and optionally DA_ACTIVITY_ID_NET8/NET10).",
  );
}
