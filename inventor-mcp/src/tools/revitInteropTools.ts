import {
  prepareInformedDesignPublish,
  replicateParametersFromRevitPayload,
} from "../bridgeClient.js";
import type { JsonObject } from "../types/contracts.js";
import { mapRevitToInventorParameters, normalizeRevitPayload } from "../utils/validation.js";

export const revitInteropToolDefs = [
  {
    name: "revit_extract_family_parameters",
    description:
      "Normalize a Revit family parameter payload for downstream Inventor parameter replication.",
    inputSchema: {
      type: "object",
      properties: {
        revitPayload: { type: "object" },
      },
      required: ["revitPayload"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_replicate_from_revit_payload",
    description:
      "Map Revit family parameters to Inventor parameter schema and apply to active document.",
    inputSchema: {
      type: "object",
      properties: {
        revitPayload: { type: "object" },
        runRuleAfterSet: { type: "string" },
      },
      required: ["revitPayload"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_prepare_informed_design_publish",
    description:
      "Validate active Inventor document for manual Informed Design publish and return checklist.",
    inputSchema: {
      type: "object",
      properties: {
        runLabel: { type: "string" },
      },
      additionalProperties: false,
    },
  },
] as const;

export async function invokeRevitInteropTool(
  name: string,
  args: JsonObject,
): Promise<JsonObject | null> {
  if (name === "revit_extract_family_parameters") {
    const payload = normalizeRevitPayload(args.revitPayload);
    const mappedParameters = mapRevitToInventorParameters(payload);
    const paritySelectors = summarizeParitySelectors(mappedParameters);
    return {
      source: payload.source,
      familyName: payload.familyName ?? null,
      typeName: payload.typeName ?? null,
      parameterCount: mappedParameters.length,
      parameters: mappedParameters,
      paritySelectors,
    };
  }

  if (name === "inventor_replicate_from_revit_payload") {
    const payload = normalizeRevitPayload(args.revitPayload);
    const mappedParameters = mapRevitToInventorParameters(payload);
    const paritySelectors = summarizeParitySelectors(mappedParameters);
    return replicateParametersFromRevitPayload({
      payload: {
        ...payload,
        parameters: payload.parameters,
      },
      runRuleAfterSet:
        typeof args.runRuleAfterSet === "string" ? args.runRuleAfterSet : undefined,
    }).then((result) => ({
      ...result,
      source: payload.source,
      mappedCount: mappedParameters.length,
      paritySelectors,
    }));
  }

  if (name === "inventor_prepare_informed_design_publish") {
    const result = await prepareInformedDesignPublish({
      runLabel: typeof args.runLabel === "string" ? args.runLabel : undefined,
    });
    return {
      ...result,
      mode: "manual_publish_phase_1",
    };
  }

  return null;
}

function summarizeParitySelectors(parameters: Array<{ name: string; value?: unknown }>): JsonObject {
  const lookup = new Map<string, unknown>();
  for (const p of parameters) {
    const name = typeof p.name === "string" ? p.name : "";
    if (!name) {
      continue;
    }
    lookup.set(name.toUpperCase(), p.value);
  }

  return {
    routeCip: lookup.get("ROUTE_CIP") === true,
    routeErection: lookup.get("ROUTE_ERECTION") === true,
    finishGalvanized: lookup.get("FINISH_GALVANIZED") === true,
    finishPaintedBlack: lookup.get("FINISH_PAINTEDBLACK") === true,
    finishUnfinished: lookup.get("FINISH_UNFINISHED") === true,
  };
}
