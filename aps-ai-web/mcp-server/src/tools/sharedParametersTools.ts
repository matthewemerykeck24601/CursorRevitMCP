import { z } from "zod";
import {
  resolveParametersApiContext,
  searchParametersCollection,
} from "../lib/parametersApiClient.js";
import {
  clearSharedParametersCatalogCache,
  getSharedParameterByGuid,
  getSharedParameterByName,
  getSharedParametersCatalogStats,
  lookupSharedParameters,
} from "../lib/sharedParametersCatalog.js";

const APS_PARAMETERS_OVERVIEW = "https://aps.autodesk.com/developer/overview/parameters-api";

const SharedParametersLookupInput = z.object({
  query: z.string().describe("Parameter name substring, or full Revit shared GUID"),
  mode: z.enum(["exact", "contains", "prefix"]).optional().default("contains"),
  limit: z.number().int().min(1).max(200).optional().default(30),
  by_guid: z.boolean().optional().default(false),
  /**
   * When true (default): if the bundled .txt has no match and APS context is complete
   * (token + account + group + collection), call Parameters API search as fallback.
   */
  parameters_api_fallback: z.boolean().optional().default(true),
  access_token: z.string().optional(),
  accessToken: z.string().optional(),
  parameters_account_id: z.string().optional(),
  parametersAccountId: z.string().optional(),
  parameters_group_id: z.string().optional(),
  parametersGroupId: z.string().optional(),
  parameters_collection_id: z.string().optional(),
  parametersCollectionId: z.string().optional(),
});

const SharedParametersCatalogStatsInput = z.object({
  reload: z
    .boolean()
    .optional()
    .default(false)
    .describe("Clear cache and reload from disk on next access."),
});

export const sharedParametersLookup = {
  name: "shared_parameters_lookup" as const,
  description:
    "Resolve shared parameters from bundled Shared_Params_2015_v01.txt (or SHARED_PARAMETERS_FILE_PATH). Optional fallback: Autodesk Parameters API search when file has no hits — pass access_token + parameters_account_id + parameters_group_id + parameters_collection_id (or set APS_PARAMETERS_* env vars). See " +
    APS_PARAMETERS_OVERVIEW,
  parameters: SharedParametersLookupInput,

  async handler(params: z.infer<typeof SharedParametersLookupInput>) {
    const q = params.query.trim();

    const tryParametersApi = async (searchedText: string) => {
      if (params.parameters_api_fallback === false) return null;
      const ctx = resolveParametersApiContext(params);
      if (!ctx) return null;
      try {
        const { results, pagination } = await searchParametersCollection({
          ...ctx,
          searchedText,
          maxResults: params.limit,
        });
        return {
          searchedText,
          pagination,
          matches: results.map((r) => ({
            parametersServiceId: r.id,
            name: r.name,
            specId: r.specId ?? "",
            description: r.description ?? "",
            readOnly: r.readOnly,
          })),
          warning:
            "These IDs are from the Parameters Service, not Revit shared-parameter GUIDs from a .txt. Do not use parametersServiceId in Design Automation sharedParameterGuidMap unless you have a verified mapping.",
          documentationUrl: APS_PARAMETERS_OVERVIEW,
        };
      } catch (e) {
        return {
          error: e instanceof Error ? e.message : String(e),
          documentationUrl: APS_PARAMETERS_OVERVIEW,
        };
      }
    };

    const apsHitCount = (aps: unknown) => {
      if (!aps || typeof aps !== "object") return 0;
      if ("error" in aps) return 0;
      const m = (aps as { matches?: unknown }).matches;
      return Array.isArray(m) ? m.length : 0;
    };

    if (params.by_guid) {
      const normalized = q.replace(/^\{|\}$/g, "").trim();
      const row = getSharedParameterByGuid(normalized);
      if (row) {
        return {
          success: true,
          source: "file" as const,
          matches: [row],
          count: 1,
          apsParametersApi: null,
        };
      }
      const aps = await tryParametersApi(normalized);
      return {
        success: true,
        source: "file" as const,
        matches: [],
        count: apsHitCount(aps),
        apsParametersApi: aps,
      };
    }

    if (params.mode === "exact") {
      const row = getSharedParameterByName(q);
      if (row) {
        return {
          success: true,
          source: "file" as const,
          matches: [{ ...row, matchKind: "exact" as const }],
          count: 1,
          apsParametersApi: null,
        };
      }
      const aps = await tryParametersApi(q);
      return {
        success: true,
        source: "file" as const,
        matches: [],
        count: apsHitCount(aps),
        apsParametersApi: aps,
      };
    }

    const fileMatches = lookupSharedParameters({
      query: q,
      limit: params.limit,
      mode: params.mode,
    });
    if (fileMatches.length > 0) {
      return {
        success: true,
        source: "file" as const,
        matches: fileMatches,
        count: fileMatches.length,
        apsParametersApi: null,
      };
    }

    const aps = await tryParametersApi(q);
    return {
      success: true,
      source: "file" as const,
      matches: [],
      count: apsHitCount(aps),
      apsParametersApi: aps,
    };
  },
};

export const sharedParametersCatalogStats = {
  name: "shared_parameters_catalog_stats" as const,
  description:
    "Report loaded shared-parameter catalog size and source path. Optional APS Parameters API: set APS_PARAMETERS_ACCOUNT_ID, APS_PARAMETERS_GROUP_ID, APS_PARAMETERS_COLLECTION_ID (+ token on each lookup or APS_PARAMETERS_ACCESS_TOKEN for dev only).",
  parameters: SharedParametersCatalogStatsInput,

  handler(params: z.infer<typeof SharedParametersCatalogStatsInput>) {
    if (params.reload) clearSharedParametersCatalogCache();
    return { success: true, ...getSharedParametersCatalogStats() };
  },
};

export async function runSharedParametersLookup(args: unknown) {
  const parsed = SharedParametersLookupInput.parse(args);
  return sharedParametersLookup.handler(parsed);
}

export function runSharedParametersCatalogStats(args: unknown) {
  const parsed = SharedParametersCatalogStatsInput.parse(args);
  return sharedParametersCatalogStats.handler(parsed);
}
