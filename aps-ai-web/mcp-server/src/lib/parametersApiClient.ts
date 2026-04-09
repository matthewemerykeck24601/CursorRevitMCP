/**
 * Autodesk Parameters API (Parameters Service) — search fallback for MCP lookup.
 * @see https://aps.autodesk.com/developer/overview/parameters-api
 * @see https://aps.autodesk.com/en/docs/parameters/v1/tutorials/parameters/search-parameters/
 *
 * Search returns Parameters Service `id` values (e.g. parameters.&lt;account&gt;:…-1.0.0).
 * Those are not the same as Revit shared-parameter GUIDs from a .txt file; do not
 * put API `id` into Design Automation `sharedParameterGuidMap` without a verified mapping.
 */

const APS_PARAMETERS_V1 = "https://developer.api.autodesk.com/parameters/v1";

export type ApsParameterSearchRow = {
  id: string;
  name: string;
  description?: string;
  specId?: string;
  readOnly?: boolean;
  metadata?: unknown[];
};

export type ApsParameterSearchResponse = {
  pagination?: {
    offset?: number;
    limit?: number;
    totalResults?: number;
    nextUrl?: string;
  };
  results?: ApsParameterSearchRow[];
};

export type SearchParametersCollectionParams = {
  accessToken: string;
  accountId: string;
  groupId: string;
  collectionId: string;
  /** searchedText filter — substring in parameter name (per APS docs). */
  searchedText: string;
  /** Max rows to return after fetch (API default page size applies). */
  maxResults?: number;
};

/**
 * POST …/accounts/{accountId}/groups/{groupId}/collections/{collectionId}/parameters:search
 * Requires a token with appropriate Parameters / data access (tutorials use data:search).
 */
export async function searchParametersCollection(
  params: SearchParametersCollectionParams,
): Promise<{ results: ApsParameterSearchRow[]; pagination?: ApsParameterSearchResponse["pagination"] }> {
  const url = `${APS_PARAMETERS_V1}/accounts/${encodeURIComponent(params.accountId)}/groups/${encodeURIComponent(params.groupId)}/collections/${encodeURIComponent(params.collectionId)}/parameters:search`;

  const body = {
    searchedText: params.searchedText,
    isArchived: false,
    sort: "NAME_ASCENDING",
  };

  const res = await fetch(url, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${params.accessToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
    cache: "no-store",
  });

  const text = await res.text();
  let json: ApsParameterSearchResponse;
  try {
    json = JSON.parse(text) as ApsParameterSearchResponse;
  } catch {
    throw new Error(`Parameters API: invalid JSON (${res.status}): ${text.slice(0, 400)}`);
  }

  if (!res.ok) {
    throw new Error(`Parameters API search failed (${res.status}): ${text.slice(0, 600)}`);
  }

  let results = json.results ?? [];
  const max = Math.min(Math.max(params.maxResults ?? 40, 1), 100);
  results = results.slice(0, max);

  return { results, pagination: json.pagination };
}

export function resolveParametersApiContext(args: {
  access_token?: string;
  accessToken?: string;
  parameters_account_id?: string;
  parametersAccountId?: string;
  parameters_group_id?: string;
  parametersGroupId?: string;
  parameters_collection_id?: string;
  parametersCollectionId?: string;
}): {
  accessToken: string;
  accountId: string;
  groupId: string;
  collectionId: string;
} | null {
  const accessToken = String(
    args.access_token ?? args.accessToken ?? process.env.APS_PARAMETERS_ACCESS_TOKEN ?? "",
  ).trim();
  const accountId = String(
    args.parameters_account_id ??
      args.parametersAccountId ??
      process.env.APS_PARAMETERS_ACCOUNT_ID ??
      "",
  ).trim();
  const groupId = String(
    args.parameters_group_id ??
      args.parametersGroupId ??
      process.env.APS_PARAMETERS_GROUP_ID ??
      "",
  ).trim();
  const collectionId = String(
    args.parameters_collection_id ??
      args.parametersCollectionId ??
      process.env.APS_PARAMETERS_COLLECTION_ID ??
      "",
  ).trim();

  if (!accessToken || !accountId || !groupId || !collectionId) return null;
  return { accessToken, accountId, groupId, collectionId };
}
