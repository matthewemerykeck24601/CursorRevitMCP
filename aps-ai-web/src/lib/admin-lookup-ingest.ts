import {
  type LookupCacheStateDoc,
  LOOKUP_SCHEMA_VERSION,
} from "@/lib/admin-lookup-schema";
import { env } from "@/lib/env";

export type LookupIngestPolicy = {
  readOnlyLatestExtraction: boolean;
  allowRequestCreateOverride: boolean;
};

export function getLookupIngestPolicy(): LookupIngestPolicy {
  return {
    readOnlyLatestExtraction: env.adminDataConnectorReadOnlyLatestExtraction,
    allowRequestCreateOverride: env.adminAllowDataConnectorRequestCreation,
  };
}

export function assertReadOnlyLatestExtractionPolicy(
  requestCreateIfMissing: boolean,
): void {
  const policy = getLookupIngestPolicy();
  if (
    requestCreateIfMissing &&
    policy.readOnlyLatestExtraction &&
    !policy.allowRequestCreateOverride
  ) {
    throw new Error(
      "Blocked by policy: lookup ingestion is read-only and cannot trigger Data Connector extraction requests.",
    );
  }
}

export function initLookupCacheState(hubId: string): LookupCacheStateDoc {
  return {
    hubId,
    lastIngestStatus: "idle",
    rowCounts: {},
    schemaVersion: LOOKUP_SCHEMA_VERSION,
    readOnlyLatestExtraction: env.adminDataConnectorReadOnlyLatestExtraction,
  };
}
