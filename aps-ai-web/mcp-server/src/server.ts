import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { z } from "zod";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";
import { readSelectedElements } from "./tools/readSelectedElements.js";
import { getElementParameters } from "./tools/getElementParameters.js";
import { searchElements } from "./tools/searchElements.js";
import { listModelViews } from "./tools/listModelViews.js";
import { fitOrIsolate } from "./tools/fitOrIsolate.js";
import { proposeParameterWrite } from "./tools/proposeParameterWrite.js";
import { aecQuery } from "./tools/aecQuery.js";

const ToolArgsSchema = z.record(z.string(), z.unknown());

function textResult(payload: unknown) {
  return {
    content: [{ type: "text", text: JSON.stringify(payload) }],
  };
}

export function buildServer() {
  const server = new Server(
    { name: "aps-ai-web-mcp-server", version: "0.1.0" },
    { capabilities: { tools: {} } },
  );

  server.setRequestHandler(ListToolsRequestSchema, async () => ({
    tools: [
      {
        name: "get_selected_elements",
        description:
          "Read selected elements and optionally filter returned properties.",
        inputSchema: {
          type: "object",
          properties: {
            selectedElements: { type: "array" },
            includeAll: { type: "boolean" },
            requestedFields: { type: "array", items: { type: "string" } },
          },
          required: ["selectedElements"],
        },
      },
      {
        name: "get_element_parameters",
        description:
          "Return parameter values for selected elements using query aliases.",
        inputSchema: {
          type: "object",
          properties: {
            selectedElements: { type: "array" },
            dbIds: { type: "array", items: { type: "number" } },
            parameterQueries: { type: "array", items: { type: "string" } },
          },
          required: ["selectedElements"],
        },
      },
      {
        name: "search_elements",
        description:
          "Search selected elements by name/property text and return matching dbIds.",
        inputSchema: {
          type: "object",
          properties: {
            selectedElements: { type: "array" },
            query: { type: "string" },
          },
          required: ["query"],
        },
      },
      {
        name: "list_model_views",
        description: "List already-available model views from caller context.",
        inputSchema: {
          type: "object",
          properties: {
            views: { type: "array" },
          },
          required: [],
        },
      },
      {
        name: "viewer_action",
        description: "Return fit/isolate/clear actions for caller to execute.",
        inputSchema: {
          type: "object",
          properties: {
            action: { type: "string" },
            dbIds: { type: "array", items: { type: "number" } },
          },
          required: ["action"],
        },
      },
      {
        name: "aec_query",
        description:
          "Query AEC Data Model API and return flattened rows for table display.",
        inputSchema: {
          type: "object",
          properties: {
            accessToken: { type: "string" },
            hubId: { type: "string" },
            projectId: { type: "string" },
            itemId: { type: "string" },
          },
          required: ["accessToken", "hubId", "projectId"],
        },
      },
      {
        name: "set_element_parameters_guarded",
        description:
          "Two-step guarded write for parameter edits (preview then explicit confirmation token).",
        inputSchema: {
          type: "object",
          properties: {
            apply: { type: "boolean" },
            confirmationToken: { type: "string" },
            changes: {
              type: "array",
              items: {
                type: "object",
                properties: {
                  dbId: { type: "number" },
                  parameter: { type: "string" },
                  value: { type: "string" },
                },
                required: ["dbId", "parameter", "value"],
              },
            },
          },
          required: ["changes"],
        },
      },
    ],
  }));

  server.setRequestHandler(CallToolRequestSchema, async (request) => {
    const name = request.params.name;
    const args = ToolArgsSchema.parse(request.params.arguments ?? {});

    if (name === "get_selected_elements") {
      return textResult(readSelectedElements(args));
    }
    if (name === "get_element_parameters") {
      return textResult(getElementParameters(args));
    }
    if (name === "search_elements") {
      return textResult(searchElements(args));
    }
    if (name === "list_model_views") {
      return textResult(listModelViews(args));
    }
    if (name === "viewer_action") {
      return textResult(fitOrIsolate(args));
    }
    if (name === "aec_query") {
      return textResult(await aecQuery(args));
    }
    if (name === "set_element_parameters_guarded") {
      return textResult(proposeParameterWrite(args));
    }

    throw new Error(`Unknown tool: ${name}`);
  });

  return server;
}
