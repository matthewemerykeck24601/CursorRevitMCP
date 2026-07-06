import { z } from "zod";
import { getParameters, setParameters } from "../bridgeClient.js";
import type { IParameterSource } from "../data/IParameterSource.js";
import type { JsonObject } from "../types/contracts.js";

const SyncRequestSchema = z.object({
  parameterSetId: z.string().min(1),
  documentName: z.string().optional(),
  assemblyName: z.string().optional(),
});

export const parameterToolDefs = [
  {
    name: "inventor_get_parameters",
    description: "Get Inventor user/model parameters from active document.",
    inputSchema: {
      type: "object",
      properties: {
        includeModelParameters: { type: "boolean" },
        includeUserParameters: { type: "boolean" },
      },
      additionalProperties: false,
    },
  },
  {
    name: "inventor_set_parameters",
    description: "Set Inventor parameters and optionally run one iLogic rule.",
    inputSchema: {
      type: "object",
      properties: {
        parameters: {
          type: "array",
          items: {
            type: "object",
            properties: {
              name: { type: "string" },
              units: { type: "string" },
              expression: { type: "string" },
              value: { type: ["string", "number", "boolean"] },
              isKey: { type: "boolean" },
              comment: { type: "string" },
            },
            required: ["name"],
            additionalProperties: false,
          },
        },
        runRuleAfterSet: { type: "string" },
      },
      required: ["parameters"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_pull_parameters_from_excel",
    description: "Pull parameter set from Excel source.",
    inputSchema: {
      type: "object",
      properties: {
        parameterSetId: { type: "string" },
        documentName: { type: "string" },
        assemblyName: { type: "string" },
      },
      required: ["parameterSetId"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_push_parameters_to_excel",
    description: "Push parameter set into Excel source.",
    inputSchema: {
      type: "object",
      properties: {
        parameterSetId: { type: "string" },
        documentName: { type: "string" },
        assemblyName: { type: "string" },
        parameters: { type: "array", items: { type: "object" } },
      },
      required: ["parameterSetId", "parameters"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_sync_parameters",
    description: "Pull parameters from source and apply to active Inventor document.",
    inputSchema: {
      type: "object",
      properties: {
        parameterSetId: { type: "string" },
        documentName: { type: "string" },
        assemblyName: { type: "string" },
        runRuleAfterSet: { type: "string" },
      },
      required: ["parameterSetId"],
      additionalProperties: false,
    },
  },
] as const;

export async function invokeParameterTool(
  name: string,
  args: JsonObject,
  source: IParameterSource,
): Promise<JsonObject | null> {
  if (name === "inventor_get_parameters") {
    const rows = await getParameters({
      includeModelParameters: args.includeModelParameters === true,
      includeUserParameters: args.includeUserParameters !== false,
    });
    return {
      outcome: `Retrieved ${rows.length} parameter(s).`,
      parameters: rows,
    };
  }

  if (name === "inventor_set_parameters") {
    const parameters = Array.isArray(args.parameters) ? args.parameters : [];
    const outcome = await setParameters({
      parameters: parameters as never,
      runRuleAfterSet:
        typeof args.runRuleAfterSet === "string" ? args.runRuleAfterSet : undefined,
    });
    return outcome;
  }

  if (name === "inventor_pull_parameters_from_excel") {
    const request = SyncRequestSchema.parse(args);
    const rows = await source.pull(request);
    return {
      source: source.sourceName,
      parameterSetId: request.parameterSetId,
      pulled: rows.length,
      parameters: rows,
    };
  }

  if (name === "inventor_push_parameters_to_excel") {
    const request = SyncRequestSchema.extend({
      parameters: z.array(z.object({ name: z.string().min(1) }).passthrough()),
    }).parse(args);

    const outcome = await source.push({
      ...request,
      parameters: request.parameters,
    });
    return {
      source: source.sourceName,
      parameterSetId: request.parameterSetId,
      updated: outcome.updated,
    };
  }

  if (name === "inventor_sync_parameters") {
    const request = SyncRequestSchema.parse(args);
    const pulled = await source.pull(request);
    const applied = await setParameters({
      parameters: pulled,
      runRuleAfterSet:
        typeof args.runRuleAfterSet === "string" ? args.runRuleAfterSet : undefined,
    });
    return {
      source: source.sourceName,
      parameterSetId: request.parameterSetId,
      pulled: pulled.length,
      applied,
    };
  }

  return null;
}
